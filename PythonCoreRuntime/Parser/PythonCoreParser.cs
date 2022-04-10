using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using PythonCoreRuntime.Parser.AST;

namespace PythonCoreRuntime.Parser;

public class PythonCoreParser
{
    readonly IPythonCoreTokenizer _tokenizer;
    private int _FlowLevel = 0;
    private int _FuncLevel = 0;
    
    public PythonCoreParser(IPythonCoreTokenizer tokenizer)
    {
        _tokenizer = tokenizer;
    }
    
    
    
    
    #region Expression rules
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns> Expression node </returns>
    private ExpressionNode ParseAtom()
    {
        var start = _tokenizer.CurPosition;
        var symbol = _tokenizer.CurSymbol;
        
        switch (_tokenizer.CurSymbol.Code)
        {
            case TokenCode.PyNone:
                _tokenizer.Advance();
                return new NoneExpressionNode(start, _tokenizer.CurPosition, symbol);
            case TokenCode.PyFalse:
                _tokenizer.Advance();
                return new FalseExpressionNode(start, _tokenizer.CurPosition, symbol);
            case TokenCode.PyTrue:
                _tokenizer.Advance();
                return new TrueExpressionNode(start, _tokenizer.CurPosition, symbol);
            case TokenCode.PyElipsis:
                _tokenizer.Advance();
                return new ElipsisExpressionNode(start, _tokenizer.CurPosition, symbol);
            case TokenCode.Name:
                _tokenizer.Advance();
                return new NameExpressionNode(start, _tokenizer.CurPosition, symbol);
            case TokenCode.Number:
                _tokenizer.Advance();
                return new NumberExpressionNode(start, _tokenizer.CurPosition, symbol);
            case TokenCode.String:
            {
                _tokenizer.Advance();
                var elements = new List<Token>();
                elements.Add(symbol);
                
                while (_tokenizer.CurSymbol.Code == TokenCode.String)
                {
                    elements.Add(_tokenizer.CurSymbol);
                    _tokenizer.Advance();
                }

                return new StringExpressionNode(start, _tokenizer.CurPosition, elements.ToImmutableArray());
            }
            case TokenCode.PyLeftParen:
            {
                _tokenizer.Advance();
                var right = _tokenizer.CurSymbol.Code == TokenCode.PyRightParen ? null :
                    _tokenizer.CurSymbol.Code == TokenCode.PyYield ? ParseYieldExpr() : ParseTestListComp();
                if (_tokenizer.CurSymbol.Code != TokenCode.PyRightParen)
                    throw new SyntaxError("Expecting ')' in tuple!", _tokenizer.CurPosition);
                var symbol2 = _tokenizer.CurSymbol;
                _tokenizer.Advance();

                return new TupleExpressionNode(start, _tokenizer.CurPosition, symbol, right, symbol2);
            }
            case TokenCode.PyLeftBracket:
            {
                _tokenizer.Advance();
                var right = _tokenizer.CurSymbol.Code == TokenCode.PyRightBracket ? null :
                    ParseTestListComp();
                if (_tokenizer.CurSymbol.Code != TokenCode.PyRightBracket)
                    throw new SyntaxError("Expecting ']' in list!", _tokenizer.CurPosition);
                var symbol2 = _tokenizer.CurSymbol;
                _tokenizer.Advance();

                return new ListExpressionNode(start, _tokenizer.CurPosition, symbol, right, symbol2);
            }
            case TokenCode.PyLeftCurly:
            {
                _tokenizer.Advance();
                var right = _tokenizer.CurSymbol.Code == TokenCode.PyRightCurly ? null :
                    ParseDictorSetMaker();
                if (_tokenizer.CurSymbol.Code != TokenCode.PyRightBracket)
                {
                    throw right is DictionaryContainerExpressionNode || right == null
                        ? new SyntaxError("Expecting '}' in dictionary!", _tokenizer.CurPosition)
                        : new SyntaxError("Expecting '}' in set!", _tokenizer.CurPosition);
                }

                var symbol2 = _tokenizer.CurSymbol;
                _tokenizer.Advance();
                
                return right is DictionaryContainerExpressionNode || right == null ? 
                    new DictionaryExpressionNode(start, _tokenizer.CurPosition, symbol, right, symbol2) :
                    new SetExpressionNode(start, _tokenizer.CurPosition, symbol, right, symbol2);
            }
            
            default:
                throw new SyntaxError("Illegal literal found in source!", _tokenizer.CurPosition);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseAtomExpr()
    {
        var start = _tokenizer.CurPosition;
        var symbol = _tokenizer.CurSymbol.Code == TokenCode.PyAwait ? _tokenizer.CurSymbol : null;
        if (symbol != null) _tokenizer.Advance();
        var left = ParseAtom();

        if (_tokenizer.CurSymbol.Code == TokenCode.PyDot ||
            _tokenizer.CurSymbol.Code == TokenCode.PyLeftParen ||
            _tokenizer.CurSymbol.Code == TokenCode.PyLeftBracket)
        {
            var elements = new List<ExpressionNode>();
            while (_tokenizer.CurSymbol.Code == TokenCode.PyDot ||
                   _tokenizer.CurSymbol.Code == TokenCode.PyLeftParen ||
                   _tokenizer.CurSymbol.Code == TokenCode.PyLeftBracket)
            {
                elements.Add(ParseTrailer());
            }

            return new AtomExpressionNode(start, _tokenizer.CurPosition, symbol, left, elements.ToImmutableArray());
        }

        return symbol == null
            ? left
            : new AtomExpressionNode(start, _tokenizer.CurPosition, symbol, left, ImmutableArray<ExpressionNode>.Empty);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParsePower()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseAtomExpr();
        if (_tokenizer.CurSymbol.Code == TokenCode.PyPower)
        {
            var symbol = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            var right = ParseFactor();
            return new PowerExpressionNode(start, _tokenizer.CurPosition, left, symbol, right);
        }

        return left;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseFactor()
    {
        var start = _tokenizer.CurPosition;
        var symbol = _tokenizer.CurSymbol;
        switch (_tokenizer.CurSymbol.Code)
        {
            case TokenCode.PyPlus:
                _tokenizer.Advance();
                var right1 = ParseFactor();
                return new UnaryPlusExpressionNode(start, _tokenizer.CurPosition, symbol, right1);
                
            case TokenCode.PyMinus:
                _tokenizer.Advance();
                var right2 = ParseFactor();
                return new UnaryMinusExpressionNode(start, _tokenizer.CurPosition, symbol, right2);
                
            case TokenCode.PyBitInvert:
                _tokenizer.Advance();
                var right3 = ParseFactor();
                return new UnaryBitInvertExpressionNode(start, _tokenizer.CurPosition, symbol, right3);
                
            default:
                return ParsePower();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseTerm()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseFactor();
        var symbol = _tokenizer.CurSymbol;

        while (symbol.Code == TokenCode.PyMul || 
               symbol.Code == TokenCode.PyDiv || 
               symbol.Code == TokenCode.PyFloorDiv || 
               symbol.Code == TokenCode.PyModulo || 
               symbol.Code == TokenCode.PyMatrice)
        {
            switch (symbol.Code)
            {
                case TokenCode.PyMul:
                    _tokenizer.Advance();
                    var right1 = ParseFactor();
                    left = new MulExpressionNode(start, _tokenizer.CurPosition, left, symbol, right1);
                    break;
                case TokenCode.PyDiv:
                    _tokenizer.Advance();
                    var right2 = ParseFactor();
                    left = new DivExpressionNode(start, _tokenizer.CurPosition, left, symbol, right2);
                    break;
                case TokenCode.PyFloorDiv:
                    _tokenizer.Advance();
                    var right3 = ParseFactor();
                    left = new FloorDivExpressionNode(start, _tokenizer.CurPosition, left, symbol, right3);
                    break;
                case TokenCode.PyModulo:
                    _tokenizer.Advance();
                    var right4 = ParseFactor();
                    left = new ModuloExpressionNode(start, _tokenizer.CurPosition, left, symbol, right4);
                    break;
                case TokenCode.PyMatrice:
                    _tokenizer.Advance();
                    var right5 = ParseFactor();
                    left = new MatriceExpressionNode(start, _tokenizer.CurPosition, left, symbol, right5);
                    break;
            }

            symbol = _tokenizer.CurSymbol;
        }

        return left;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseArith()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseTerm();
        var symbol = _tokenizer.CurSymbol;

        while (symbol.Code == TokenCode.PyPlus || 
               symbol.Code == TokenCode.PyMinus )
        {
            switch (symbol.Code)
            {
                case TokenCode.PyPlus:
                    _tokenizer.Advance();
                    var right1 = ParseTerm();
                    left = new PlusExpressionNode(start, _tokenizer.CurPosition, left, symbol, right1);
                    break;
                case TokenCode.PyMinus:
                    _tokenizer.Advance();
                    var right2 = ParseTerm();
                    left = new MinusExpressionNode(start, _tokenizer.CurPosition, left, symbol, right2);
                    break;
            }

            symbol = _tokenizer.CurSymbol;
        }

        return left;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseShift()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseArith();
        var symbol = _tokenizer.CurSymbol;

        while (symbol.Code == TokenCode.PyShiftLeft || 
               symbol.Code == TokenCode.PyShiftRight )
        {
            switch (symbol.Code)
            {
                case TokenCode.PyShiftLeft:
                    _tokenizer.Advance();
                    var right1 = ParseArith();
                    left = new ShiftLeftExpressionNode(start, _tokenizer.CurPosition, left, symbol, right1);
                    break;
                case TokenCode.PyShiftRight:
                    _tokenizer.Advance();
                    var right2 = ParseArith();
                    left = new ShiftRightExpressionNode(start, _tokenizer.CurPosition, left, symbol, right2);
                    break;
            }

            symbol = _tokenizer.CurSymbol;
        }

        return left;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseBitwiseAnd()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseShift();

        while (_tokenizer.CurSymbol.Code == TokenCode.PyBitAnd)
        {
            var symbol = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            var right = ParseShift();
            left = new BitwiseAndExpressionNode(start, _tokenizer.CurPosition, left, symbol, right);
        }
        
        return left;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseBitwiseXor()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseBitwiseAnd();

        while (_tokenizer.CurSymbol.Code == TokenCode.PyBitXor)
        {
            var symbol = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            var right = ParseBitwiseAnd();
            left = new BitwiseXorExpressionNode(start, _tokenizer.CurPosition, left, symbol, right);
        }
        
        return left;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseBitwiseOr()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseBitwiseXor();

        while (_tokenizer.CurSymbol.Code == TokenCode.PyBitOr)
        {
            var symbol = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            var right = ParseBitwiseXor();
            left = new BitwiseOrExpressionNode(start, _tokenizer.CurPosition, left, symbol, right);
        }
        
        return left;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseStarExpr()
    {
        var start = _tokenizer.CurPosition;
        var symbol = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var right = ParseBitwiseOr();

        return new StarExpressionNode(start, _tokenizer.CurPosition, symbol, right);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseComparison()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseBitwiseOr();
        var symbol = _tokenizer.CurSymbol;

        while (symbol.Code == TokenCode.PyLess || 
               symbol.Code == TokenCode.PyLessEqual || 
               symbol.Code == TokenCode.PyEqual || 
               symbol.Code == TokenCode.PyGreater || 
               symbol.Code == TokenCode.PyGreaterEqual ||
               symbol.Code == TokenCode.PyNotEqual ||
               symbol.Code == TokenCode.PyNot ||
               symbol.Code == TokenCode.PyIn ||
               symbol.Code == TokenCode.PyIs)
        {
            switch (symbol.Code)
            {
                case TokenCode.PyLess:
                    _tokenizer.Advance();
                    var right1 = ParseBitwiseOr();
                    left = new ComparisonLessExpressionNode(start, _tokenizer.CurPosition, left, symbol, right1);
                    break;
                case TokenCode.PyLessEqual:
                    _tokenizer.Advance();
                    var right2 = ParseBitwiseOr();
                    left = new ComparisonLessEqualExpressionNode(start, _tokenizer.CurPosition, left, symbol, right2);
                    break;
                case TokenCode.PyEqual:
                    _tokenizer.Advance();
                    var right3 = ParseBitwiseOr();
                    left = new ComparisonEqualExpressionNode(start, _tokenizer.CurPosition, left, symbol, right3);
                    break;
                case TokenCode.PyGreater:
                    _tokenizer.Advance();
                    var right4 = ParseBitwiseOr();
                    left = new ComparisonGreaterExpressionNode(start, _tokenizer.CurPosition, left, symbol, right4);
                    break;
                case TokenCode.PyGreaterEqual:
                    _tokenizer.Advance();
                    var right5 = ParseBitwiseOr();
                    left = new ComparisonGreaterEqualExpressionNode(start, _tokenizer.CurPosition, left, symbol, right5);
                    break;
                case TokenCode.PyNotEqual:
                    _tokenizer.Advance();
                    var right6 = ParseBitwiseOr();
                    left = new ComparisonNotEqualExpressionNode(start, _tokenizer.CurPosition, left, symbol, right6);
                    break;
                case TokenCode.PyNot:
                    _tokenizer.Advance();
                    if (_tokenizer.CurSymbol.Code != TokenCode.PyIn) throw new SyntaxError("Expecting 'in' after 'not' in comparison!", _tokenizer.CurPosition);
                    var symbol2 = _tokenizer.CurSymbol;
                    _tokenizer.Advance();
                    var right7 = ParseBitwiseOr();
                    left = new ComparisonNotInExpressionNode(start, _tokenizer.CurPosition, left, symbol, symbol2, right7);
                    break;
                case TokenCode.PyIn:
                    _tokenizer.Advance();
                    var right8 = ParseBitwiseOr();
                    left = new ComparisonInExpressionNode(start, _tokenizer.CurPosition, left, symbol, right8);
                    break;
                case TokenCode.PyIs:
                    _tokenizer.Advance();
                    if (_tokenizer.CurSymbol.Code == TokenCode.PyNot)
                    {
                        var symbol3 = _tokenizer.CurSymbol;
                        _tokenizer.Advance();
                        var right9 = ParseBitwiseOr();
                        left = new ComparisonIsNotExpressionNode(start, _tokenizer.CurPosition, left, symbol, symbol3, right9);
                    }
                    else
                    {
                        var right9 = ParseBitwiseOr();
                        left = new MatriceExpressionNode(start, _tokenizer.CurPosition, left, symbol, right9);
                    }
                    break;
            }

            symbol = _tokenizer.CurSymbol;
        }
        
        return left;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseNotTest()
    {
        var start = _tokenizer.CurPosition;
        if (_tokenizer.CurSymbol.Code == TokenCode.PyNot)
        {
            var symbol = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            var right = ParseNotTest();

            return new NotTestExpressionNode(start, _tokenizer.CurPosition, symbol, right);
        }

        return ParseComparison();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseAndTest()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseNotTest();

        while (_tokenizer.CurSymbol.Code == TokenCode.PyAnd)
        {
            var symbol = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            var right = ParseNotTest();
            left = new AndTestExpressionNode(start, _tokenizer.CurPosition, left, symbol, right);
        }
        
        return left;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseOrTest()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseAndTest();

        while (_tokenizer.CurSymbol.Code == TokenCode.PyOr)
        {
            var symbol = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            var right = ParseAndTest();
            left = new OrTestExpressionNode(start, _tokenizer.CurPosition, left, symbol, right);
        }
        
        return left;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isCond"></param>
    /// <returns></returns>
    private ExpressionNode ParseLambda(bool isCond)
    {
        var start = _tokenizer.CurPosition;
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var left = _tokenizer.CurSymbol.Code == TokenCode.PyColon ? null : ParseVarArgsList();
        if (_tokenizer.CurSymbol.Code != TokenCode.PyColon) 
            throw new SyntaxError("Expecting ':' in 'lambda' expression!", _tokenizer.CurPosition);
        var symbol2 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var right = ParseTest(isCond);

        return new LambdaExpressionNode(start, _tokenizer.CurPosition, isCond, symbol1, left, symbol2, right);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isCond"></param>
    /// <returns></returns>
    private ExpressionNode ParseTest(bool isCond)
    {
        if (_tokenizer.CurSymbol.Code == TokenCode.PyLambda) return ParseLambda(isCond);
        if (!isCond) return ParseOrTest();
        var start = _tokenizer.CurPosition;
        var left = ParseOrTest();
        if (_tokenizer.CurSymbol.Code == TokenCode.PyIf)
        {
            var symbol1 = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            var right = ParseOrTest();
            if (_tokenizer.CurSymbol.Code != TokenCode.PyElse) throw new SyntaxError("Expecting 'else' in test expression!", _tokenizer.CurPosition);
            var symbol2 = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            var next = ParseTest(true);

            return new TestExpressionNode(start, _tokenizer.CurPosition, left, symbol1, right, symbol2, next);
        }

        return left;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseNamedExpr()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseTest(true);
        if (_tokenizer.CurSymbol.Code == TokenCode.PyColonAssign)
        {
            var symbol = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            var right = ParseTest(true);

            return new NamedExpressionNode(start, _tokenizer.CurPosition, left, symbol, right);
        }
        
        return left;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseTestListComp()
    {
        var start = _tokenizer.CurPosition;
        var firstNode = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseNamedExpr();

        if (_tokenizer.CurSymbol.Code == TokenCode.PyAsync || _tokenizer.CurSymbol.Code == TokenCode.PyFor)
        {
            var nodes = new List<ExpressionNode>();
            nodes.Add(firstNode);
            nodes.Add(ParseCompFor());
            
            return new TestListExpressionNode(start, _tokenizer.CurPosition, nodes.ToImmutableArray(), ImmutableArray<Token>.Empty);
        }
        else if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
        {
            var nodes = new List<ExpressionNode>();
            var separators = new List<Token>();
            nodes.Add(firstNode);

            while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
            {
                separators.Add(_tokenizer.CurSymbol);
                _tokenizer.Advance();
                if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
                    throw new SyntaxError("Unexpected ',' in list!", _tokenizer.CurPosition);
                if (_tokenizer.CurSymbol.Code == TokenCode.PyRightParen ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyRightBracket) break;
                nodes.Add(_tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseNamedExpr());
            }

            return new TestListExpressionNode(start, _tokenizer.CurPosition, nodes.ToImmutableArray(),
                separators.ToImmutableArray());
        }

        return firstNode;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseTrailer()
    {
        var start = _tokenizer.CurPosition;
        var symbol = _tokenizer.CurSymbol;

        switch (symbol.Code)
        {
            case TokenCode.PyDot:
            {
                _tokenizer.Advance();
                if (_tokenizer.CurSymbol.Code != TokenCode.Name)
                    throw new SyntaxError("Expecting NAME literal after '.'", _tokenizer.CurPosition);
                var name = _tokenizer.CurSymbol;
                _tokenizer.Advance();

                return new DotNameExpressionNode(start, _tokenizer.CurPosition, symbol, name);
            }
            case TokenCode.PyLeftParen:
            {
                _tokenizer.Advance();
                var right = _tokenizer.CurSymbol.Code == TokenCode.PyRightParen ? null : ParseArgList();
                if (_tokenizer.CurSymbol.Code != TokenCode.PyRightParen)
                    throw new SyntaxError("Expecting ')' in call!", _tokenizer.CurPosition);
                var symbol2 = _tokenizer.CurSymbol;
                _tokenizer.Advance();

                return new CallExpressionNode(start, _tokenizer.CurPosition, symbol, right, symbol2);
            }
            case TokenCode.PyLeftBracket:
            {
                _tokenizer.Advance();
                var right = _tokenizer.CurSymbol.Code == TokenCode.PyRightParen ? null : ParseSubscriptList();
                if (_tokenizer.CurSymbol.Code != TokenCode.PyRightBracket)
                    throw new SyntaxError("Expecting ']' in index!", _tokenizer.CurPosition);
                var symbol2 = _tokenizer.CurSymbol;
                _tokenizer.Advance();

                return new IndexExpressionNode(start, _tokenizer.CurPosition, symbol, right, symbol2);
            }
            default:
                return new EmptyExpressionNode(); // Never happens.
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseSubscriptList()
    {
        var start = _tokenizer.CurPosition;
        var firstNode = ParseSubscript();

        if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
        {
            var nodes = new List<ExpressionNode>();
            var separators = new List<Token>();
            nodes.Add(firstNode);

            while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
            {
                separators.Add(_tokenizer.CurSymbol);
                _tokenizer.Advance();
                if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
                    throw new SyntaxError("Unexpected ',' in subscript list!", _tokenizer.CurPosition);
                if (_tokenizer.CurSymbol.Code == TokenCode.PyRightBracket) break;
                nodes.Add(ParseSubscript());
            }

            return new SubscriptListExpressionNode(start, _tokenizer.CurPosition, nodes.ToImmutableArray(), 
                separators.ToImmutableArray());
        }

        return firstNode;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseSubscript()
    {
        var start = _tokenizer.CurPosition;
        ExpressionNode? left = null, right = null, next = null;
        Token? symbol1 = null, symbol2 = null;
        if (_tokenizer.CurSymbol.Code != TokenCode.PyColon) left = ParseTest(true);
        if (_tokenizer.CurSymbol.Code == TokenCode.PyColon)
        {
            symbol1 = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            if (_tokenizer.CurSymbol.Code != TokenCode.PyColon &&
                _tokenizer.CurSymbol.Code != TokenCode.PyComma &&
                _tokenizer.CurSymbol.Code != TokenCode.PyRightBracket) right = ParseTest(true);
            if (_tokenizer.CurSymbol.Code == TokenCode.PyColon)
            {
                symbol2 = _tokenizer.CurSymbol;
                _tokenizer.Advance();
                if (_tokenizer.CurSymbol.Code != TokenCode.PyComma &&
                    _tokenizer.CurSymbol.Code != TokenCode.PyRightBracket) next = ParseTest(true);
            }
        }

        return new SubscriptExpressionNode(start, _tokenizer.CurPosition, left, symbol1, right, symbol2, next);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseExprList()
    {
        var start = _tokenizer.CurPosition;
        var firstNode = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseBitwiseOr();

        if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
        {
            var nodes = new List<ExpressionNode>();
            var separators = new List<Token>();
            nodes.Add(firstNode);

            while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
            {
                separators.Add(_tokenizer.CurSymbol);
                _tokenizer.Advance();
                if (_tokenizer.CurSymbol.Code == TokenCode.PyIn) break;
                if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
                    throw new SyntaxError("Unexpected ',' in expression list!", _tokenizer.CurPosition);
                nodes.Add(_tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseBitwiseOr());
            }

            return new ExprListExpressionNode(start, _tokenizer.CurPosition, nodes.ToImmutableArray(),
                separators.ToImmutableArray());
        }

        return firstNode;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private ExpressionNode ParseTestList()
    {
        var start = _tokenizer.CurPosition;
        var firstNode = ParseTest(true);

        if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
        {
            var nodes = new List<ExpressionNode>();
            var separators = new List<Token>();
            nodes.Add(firstNode);

            while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
            {
                separators.Add(_tokenizer.CurSymbol);
                _tokenizer.Advance();
                if (_tokenizer.CurSymbol.Code == TokenCode.PySemiColon || 
                    _tokenizer.CurSymbol.Code == TokenCode.Newline) break;
                if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
                    throw new SyntaxError("Unexpected ',' in test list!", _tokenizer.CurPosition);
                nodes.Add(ParseTest(true));
            }

            return new TestListExpressionNode(start, _tokenizer.CurPosition, nodes.ToImmutableArray(),
                separators.ToImmutableArray());
        }

        return firstNode;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private ExpressionNode ParseArgList()
    {
        var start = _tokenizer.CurPosition;
        var firstNode = ParseArgument();

        if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
        {
            var nodes = new List<ExpressionNode>();
            var separators = new List<Token>();
            nodes.Add(firstNode);

            while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
            {
                separators.Add(_tokenizer.CurSymbol);
                _tokenizer.Advance();
                if (_tokenizer.CurSymbol.Code == TokenCode.PyRightParen) break;
                if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
                    throw new SyntaxError("Unexpected ',' in argument list!", _tokenizer.CurPosition);
                nodes.Add(ParseArgument());
            }

            return new ArgListExpressionNode(start, _tokenizer.CurPosition, nodes.ToImmutableArray(),
                separators.ToImmutableArray());
        }

        return firstNode;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private ExpressionNode ParseArgument()
    {
        var start = _tokenizer.CurPosition;
        ExpressionNode? right = null;
        Token? left = null, symbol = null;

        switch (_tokenizer.CurSymbol.Code)
        {
            case TokenCode.PyMul:
            case TokenCode.PyPower:
                symbol = _tokenizer.CurSymbol;
                _tokenizer.Advance();
                right = ParseTest(true);
                break;
            case TokenCode.Name:
                left = _tokenizer.CurSymbol;
                _tokenizer.Advance();
                switch (_tokenizer.CurSymbol.Code)
                {
                    case TokenCode.PyFor:
                        right = ParseCompFor();
                        break;
                    case TokenCode.PyColonAssign:
                    case TokenCode.PyAssign:
                        symbol = _tokenizer.CurSymbol;
                        _tokenizer.Advance();
                        right = ParseTest(true);
                        break;
                    
                    default:
                        return new NameExpressionNode(start, _tokenizer.CurPosition, left);
                }
                break;
            default:
                throw new SyntaxError("Expecting name literal in argument!", _tokenizer.CurPosition);
        }

        return new ArgumentExpressionNode(start, _tokenizer.CurPosition, left, symbol, right);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private ExpressionNode ParseDictorSetMaker()
    {
        var start = _tokenizer.CurPosition;
        var isDictionary = true;
        ExpressionNode key = new EmptyExpressionNode();
        var symbol = new Token(-1, -1, TokenCode.Eof, ImmutableArray<Trivia>.Empty);
        ExpressionNode value = new EmptyExpressionNode();

        switch (_tokenizer.CurSymbol.Code)
        {
            case TokenCode.PyMul:
                isDictionary = false;
                key = ParseStarExpr();
                break;
            case TokenCode.PyPower:
            {
                var start2 = _tokenizer.CurPosition;
                symbol = _tokenizer.CurSymbol;
                _tokenizer.Advance();
                value = ParseTest(true);
                key = new PowerKeyExpressionNode(start2, _tokenizer.CurPosition, symbol, value);
                break;
            }
            default:
                key = ParseTest(true);
                if (_tokenizer.CurSymbol.Code == TokenCode.PyColon)
                {
                    symbol = _tokenizer.CurSymbol;
                    _tokenizer.Advance();
                    value = ParseTest(true);
                }
                else isDictionary = false;

                break;
        }

        var nodes = new List<ExpressionNode>();
        var separators = new List<Token>();

        if (isDictionary)
        {
            if (key is PowerKeyExpressionNode) nodes.Add(key);
            else if (symbol.Code != TokenCode.PyColon) 
                throw new SyntaxError("Expecting ':' in key/value!", _tokenizer.CurPosition);
            else nodes.Add(new KeyValueExpressionNode(start, _tokenizer.CurPosition, key, symbol, value));

            if (_tokenizer.CurSymbol.Code == TokenCode.PyFor || _tokenizer.CurSymbol.Code == TokenCode.PyAsync)
                nodes.Add(ParseCompFor());
            else
            {
                while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
                {
                    separators.Add(_tokenizer.CurSymbol);
                    _tokenizer.Advance();

                    switch (_tokenizer.CurSymbol.Code)
                    {
                        case TokenCode.PyRightCurly: break;
                        case TokenCode.PyComma:
                            throw new SyntaxError("Unexpected ',' in dictionary!", _tokenizer.CurPosition);
                        case TokenCode.PyPower:
                        {
                            var start2 = _tokenizer.CurPosition;
                            symbol = _tokenizer.CurSymbol;
                            _tokenizer.Advance();
                            value = ParseTest(true);
                            nodes.Add(new PowerKeyExpressionNode(start2, _tokenizer.CurPosition, symbol, value));
                            break;
                        }
                        default:
                            key = ParseTest(true);
                            if (_tokenizer.CurSymbol.Code != TokenCode.PyColon)
                                throw new SyntaxError("Expecting ':' in key/value element of dictionary!", _tokenizer.CurPosition);
                            symbol = _tokenizer.CurSymbol;
                            _tokenizer.Advance();
                            value = ParseTest(true);
                            nodes.Add(new KeyValueExpressionNode(start, _tokenizer.CurPosition, key, symbol, value));
                            break;
                    }
                }
            }
            
            return new DictionaryContainerExpressionNode(start, _tokenizer.CurPosition, nodes.ToImmutableArray(),
                separators.ToImmutableArray());
        }
        
        nodes.Add(key);
        
        if (_tokenizer.CurSymbol.Code == TokenCode.PyFor || _tokenizer.CurSymbol.Code == TokenCode.PyAsync)
            nodes.Add(ParseCompFor());
        else
        {
            while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
            {
                separators.Add(_tokenizer.CurSymbol);
                _tokenizer.Advance();

                switch (_tokenizer.CurSymbol.Code)
                {
                    case TokenCode.PyRightCurly: break;
                    case TokenCode.PyComma:
                        throw new SyntaxError("Unexpected ',' in Set!", _tokenizer.CurPosition);
                    default:
                        nodes.Add(_tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true));
                        break;
                }
            }
        }
        
        return new SetContainerExpressionNode(start, _tokenizer.CurPosition, nodes.ToImmutableArray(),
            separators.ToImmutableArray());
        
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode? ParseCompIter()
    {
        return _tokenizer.CurSymbol.Code == TokenCode.PyAsync ||
               _tokenizer.CurSymbol.Code == TokenCode.PyFor ? ParseCompFor() :
            _tokenizer.CurSymbol.Code == TokenCode.PyIf ? ParseCompIf() : null;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private ExpressionNode ParseSyncCompFor()
    {
        var start = _tokenizer.CurPosition;
        if (_tokenizer.CurSymbol.Code != TokenCode.PyFor)
            throw new SyntaxError("Expecting 'for' in comprehension for expression!", _tokenizer.CurPosition);
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var left = ParseExprList();
        if (_tokenizer.CurSymbol.Code != TokenCode.PyIn)
            throw new SyntaxError("Expecting 'in' in comprehension for expression!", _tokenizer.CurPosition);
        var symbol2 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var right = ParseOrTest();
        var next = ParseCompIter();

        return new CompSyncForExpressionNode(start, _tokenizer.CurPosition, symbol1, left, symbol2, right, next);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private ExpressionNode ParseCompFor()
    {
        var start = _tokenizer.CurPosition;
        if (_tokenizer.CurSymbol.Code != TokenCode.PyAsync)
            throw new SyntaxError("Expecting 'async' in comprehension for expression!", _tokenizer.CurPosition);
        var symbol = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var right = ParseSyncCompFor();

        return new CompForExpressionNode(start, _tokenizer.CurPosition, symbol, right);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private ExpressionNode ParseCompIf()
    {
        var start = _tokenizer.CurPosition;
        if (_tokenizer.CurSymbol.Code != TokenCode.PyIf)
            throw new SyntaxError("Expecting 'if' in comprehension if expression!", _tokenizer.CurPosition);
        var symbol = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var right = ParseTest(false);
        var next = ParseCompIter();

        return new CompIfExpressionNode(start, _tokenizer.CurPosition, symbol, right, next);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseYieldExpr()
    {
        var start = _tokenizer.CurPosition;
        if (_tokenizer.CurSymbol.Code != TokenCode.PyYield)
            throw new SyntaxError("Expecting 'yield' in yield expression!", _tokenizer.CurPosition);
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        if (_tokenizer.CurSymbol.Code == TokenCode.PyFrom)
        {
            var symbol2 = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            var right2 = ParseTest(true);

            return new YieldFromExpressionNode(start, _tokenizer.CurPosition, symbol1, symbol2, right2);
        }

        var right = ParseTestListStarExpr();
        return new YieldExpressionNode(start, _tokenizer.CurPosition, symbol1, right);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private ExpressionNode ParseVarArgsList()
    {
        var start = _tokenizer.CurPosition;
        var nodes = new List<ExpressionNode>();
        var separators = new List<Token>();
        Token? mulOp = null, powerOp = null;
        ExpressionNode? mulNode = null, powerNode = null;
        ExpressionNode? left = null, right = null;
        Token? symbol = null;

        switch (_tokenizer.CurSymbol.Code)
        {
            case TokenCode.PyPower:
    _power:
                powerOp = _tokenizer.CurSymbol;
                _tokenizer.Advance();
                powerNode = ParseVfpDef();
                if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
                {
                    separators.Add(_tokenizer.CurSymbol);
                    _tokenizer.Advance();
                }
                break;
            case TokenCode.PyMul:
    _mul:
                mulOp = _tokenizer.CurSymbol;
                _tokenizer.Advance();
                mulNode = ParseVfpDef();
                while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
                {
                    separators.Add(_tokenizer.CurSymbol);
                    _tokenizer.Advance();
                    if (_tokenizer.CurSymbol.Code == TokenCode.PyPower) goto _power;
                    left = ParseVfpDef();
                    if (_tokenizer.CurSymbol.Code == TokenCode.PyAssign)
                    {
                        symbol = _tokenizer.CurSymbol;
                        _tokenizer.Advance();
                        right = ParseTest(true);
                        nodes.Add(new VarArgsAssignExpressionNode(start, _tokenizer.CurPosition, left, symbol, right));
                        continue;
                    }
                    nodes.Add(left);
                }
                break;
            default:
                left = ParseVfpDef();
                if (_tokenizer.CurSymbol.Code == TokenCode.PyAssign)
                {
                    symbol = _tokenizer.CurSymbol;
                    _tokenizer.Advance();
                    right = ParseTest(true);
                    nodes.Add(new VarArgsAssignExpressionNode(start, _tokenizer.CurPosition, left, symbol, right));
                }
                else nodes.Add(left);

                while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
                {
                    separators.Add(_tokenizer.CurSymbol);
                    _tokenizer.Advance();

                    switch (_tokenizer.CurSymbol.Code)
                    {
                        case TokenCode.PyMul: 
                            goto _mul;
                        case TokenCode.PyPower: 
                            goto _power;
                        default:
                            left = ParseVfpDef();
                            if (_tokenizer.CurSymbol.Code == TokenCode.PyAssign)
                            {
                                symbol = _tokenizer.CurSymbol;
                                _tokenizer.Advance();
                                right = ParseTest(true);
                                nodes.Add(new VarArgsAssignExpressionNode(start, _tokenizer.CurPosition, left, symbol, right));
                            }
                            else nodes.Add(left);
                            break;
                    }
                }
                break;
        }
        
        return new VarArgsListExpressionNode(start, _tokenizer.CurPosition,
            mulOp, mulNode, powerOp, powerNode,
            nodes.ToImmutableArray(), separators.ToImmutableArray());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private ExpressionNode ParseVfpDef()
    {
        var start = _tokenizer.CurPosition;
        if (_tokenizer.CurSymbol.Code != TokenCode.Name)
            throw new SyntaxError("Expecting NAME literal in argument list!", _tokenizer.CurPosition);
        var symbol = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        return new NameExpressionNode(start, _tokenizer.CurPosition, symbol);
    }
    
    #endregion
    
    
    #region Statement Rules

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public StatementNode ParseEvalInput()
    {
        _tokenizer.Advance();
        var start = _tokenizer.CurPosition;
        var newlines = new List<Token>();
        var right = ParseTestList();
        while (_tokenizer.CurSymbol.Code == TokenCode.Newline)
        {
            newlines.Add(_tokenizer.CurSymbol);
            _tokenizer.Advance();
        }

        if (_tokenizer.CurSymbol.Code != TokenCode.Eof)
            throw new SyntaxError("Expecting End of file!", _tokenizer.CurPosition);

        return new EvalInputStatementNode(start, _tokenizer.CurPosition, right, newlines.ToImmutableArray(), _tokenizer.CurSymbol);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public StatementNode ParseFileInput()
    {
        _tokenizer.Advance();
        var start = _tokenizer.CurPosition;
        var nodes = new List<StatementNode>();
        var newlines = new List<Token>();

        while (_tokenizer.CurSymbol.Code != TokenCode.Eof)
        {
            if (_tokenizer.CurSymbol.Code == TokenCode.Newline)
            {
                newlines.Add(_tokenizer.CurSymbol);
                _tokenizer.Advance();
            }
            else
            {
                nodes.Add(ParseStmt());
            }
        }

        return new FileInputStatementNode(start, _tokenizer.CurPosition, nodes.ToImmutableArray(), 
            newlines.ToImmutableArray(), _tokenizer.CurSymbol);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    public StatementNode ParseSingleInput()
    {
        _tokenizer.Advance();
        var start = _tokenizer.CurPosition;
        switch (_tokenizer.CurSymbol.Code)
        {
            case TokenCode.Newline:
            {
                var newline = _tokenizer.CurSymbol;
                _tokenizer.Advance();

                return new SingleInputStatementNode(start, _tokenizer.CurPosition, newline, null);
            }
            case TokenCode.PyIf:
            case TokenCode.PyWhile:
            case TokenCode.PyFor:
            case TokenCode.PyTry:
            case TokenCode.PyWith:
            case TokenCode.PyDef:
            case TokenCode.PyClass:
            case TokenCode.PyMatrice:
            case TokenCode.PyAsync:
            {
                var right = ParseCompoundStmt();
                if (_tokenizer.CurSymbol.Code != TokenCode.Newline)
                    throw new SyntaxError("Expecting NEWLINE after compound statement!", _tokenizer.CurPosition);
                var symbol1 = _tokenizer.CurSymbol;
                _tokenizer.Advance();

                return new SingleInputStatementNode(start, _tokenizer.CurPosition, symbol1, right);
            }
            case TokenCode.Name:
#pragma warning disable CS8602
                if ((_tokenizer.CurSymbol as NameToken).Value == "match")
#pragma warning restore CS8602
                {
                    var right = ParseCompoundStmt();
                    return new SingleInputStatementNode(start, _tokenizer.CurPosition, null, right);
                }

                break;
        }

        var right2 = ParseSimpleStmt();

        return new SingleInputStatementNode(start, _tokenizer.CurPosition, null, right2);
    }








    /// <summary>
    ///     Dispatch Compound and Simple Statements to correct rules.
    /// </summary>
    /// <returns></returns>
    private StatementNode ParseStmt()
    {
        switch (_tokenizer.CurSymbol.Code)
        {
            case TokenCode.PyIf:
            case TokenCode.PyFor:
            case TokenCode.PyWhile:
            case TokenCode.PyTry:
            case TokenCode.PyWith:
            case TokenCode.PyAsync:
            case TokenCode.PyDef:
            case TokenCode.PyClass:
            case TokenCode.PyMatrice:
                return ParseCompoundStmt();
            case TokenCode.Name:
#pragma warning disable CS8602
                return (_tokenizer.CurSymbol as NameToken).Value == "match" ? ParseCompoundStmt() : ParseSimpleStmt();
#pragma warning restore CS8602
            
            default:
                return ParseSimpleStmt();
        }
    }

    /// <summary>
    ///     Collect list of small statement separated with semicolon and ends with newline.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private StatementNode ParseSimpleStmt()
    {
        var start = _tokenizer.CurPosition;
        var firstNode = ParseSmallStmt();

        if (_tokenizer.CurSymbol.Code == TokenCode.PySemiColon)
        {
            var nodes = new List<StatementNode>();
            var separatores = new List<Token>();

            while (_tokenizer.CurSymbol.Code == TokenCode.PySemiColon)
            {
                separatores.Add(_tokenizer.CurSymbol);
                _tokenizer.Advance();
                if (_tokenizer.CurSymbol.Code == TokenCode.Newline) break;
                nodes.Add(ParseSmallStmt());
            }
            
            if (_tokenizer.CurSymbol.Code != TokenCode.Newline)
                throw new SyntaxError("Expecting Newline after statement list!", _tokenizer.CurPosition);
            var symbol = _tokenizer.CurSymbol;
            _tokenizer.Advance();

            return new StatementListNode(start, _tokenizer.CurPosition, nodes.ToImmutableArray(),
                separatores.ToImmutableArray(), symbol);
        }

        return firstNode;
    }
    
    /// <summary>
    ///     Dispatch simple statement to correct rule.
    /// </summary>
    /// <returns></returns>
    private StatementNode ParseSmallStmt()
    {
        switch (_tokenizer.CurSymbol.Code)
        {
            case TokenCode.PyDel: return ParseDelStmt();
            case TokenCode.PyPass: return ParsePassStmt();
            case TokenCode.PyBreak:
            case TokenCode.PyContinue:
            case TokenCode.PyReturn:
            case TokenCode.PyRaise:
            case TokenCode.PyYield: return ParseFlowStmt();
            case TokenCode.PyImport:
            case TokenCode.PyFrom: return ParseImportStmt();
            case TokenCode.PyGlobal: return ParseGlobalStmt();
            case TokenCode.PyNonlocal: return ParseNonlocalStmt();
            case TokenCode.PyAssert: return ParseAssertStmt();
            default: return ParseExprStmt();
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private StatementNode ParseExprStmt()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseTestListStarExpr();
        var symbol = _tokenizer.CurSymbol;
        ExpressionNode right = new EmptyExpressionNode();
        
        switch (_tokenizer.CurSymbol.Code)
        {
            case TokenCode.PyPlusAssign:
                _tokenizer.Advance();
                right = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true);
                return new PlusAssignStatementNode(start, _tokenizer.CurPosition, left, symbol, right);
            case TokenCode.PyMinusAssign:
                _tokenizer.Advance();
                right = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true);
                return new MinusAssignStatementNode(start, _tokenizer.CurPosition, left, symbol, right);
            case TokenCode.PyMulAssign:
                _tokenizer.Advance();
                right = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true);
                return new MulAssignStatementNode(start, _tokenizer.CurPosition, left, symbol, right);
            case TokenCode.PyPowerAssign:
                _tokenizer.Advance();
                right = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true);
                return new PowerAssignStatementNode(start, _tokenizer.CurPosition, left, symbol, right);
            case TokenCode.PyDivAssign:
                _tokenizer.Advance();
                right = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true);
                return new DivAssignStatementNode(start, _tokenizer.CurPosition, left, symbol, right);
            case TokenCode.PyFloorDivAssign:
                _tokenizer.Advance();
                right = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true);
                return new FloorDivAssignStatementNode(start, _tokenizer.CurPosition, left, symbol, right);
            case TokenCode.PyModuloAssign:
                _tokenizer.Advance();
                right = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true);
                return new ModuloAssignStatementNode(start, _tokenizer.CurPosition, left, symbol, right);
            case TokenCode.PyMatriceAssign:
                _tokenizer.Advance();
                right = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true);
                return new MatriceAssignStatementNode(start, _tokenizer.CurPosition, left, symbol, right);
            case TokenCode.PyBitAndAssign:
                _tokenizer.Advance();
                right = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true);
                return new BitAndAssignStatementNode(start, _tokenizer.CurPosition, left, symbol, right);
            case TokenCode.PyBitOrAssign:
                _tokenizer.Advance();
                right = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true);
                return new BitOrAssignStatementNode(start, _tokenizer.CurPosition, left, symbol, right);
            case TokenCode.PyBitXorAssign:
                _tokenizer.Advance();
                right = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true);
                return new BitXorAssignStatementNode(start, _tokenizer.CurPosition, left, symbol, right);
            case TokenCode.PyShiftLeftAssign:
                _tokenizer.Advance();
                right = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true);
                return new ShiftLeftAssignStatementNode(start, _tokenizer.CurPosition, left, symbol, right);
            case TokenCode.PyShiftRightAssign:
                _tokenizer.Advance();
                right = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true);
                return new ShiftRightAssignStatementNode(start, _tokenizer.CurPosition, left, symbol, right);
            case TokenCode.PyColon:
            {
                _tokenizer.Advance();
                right = ParseTest(true);
                Token? symbol2 = null;
                ExpressionNode? next = null;
                if (_tokenizer.CurSymbol.Code == TokenCode.PyAssign)
                {
                    symbol2 = _tokenizer.CurSymbol;
                    _tokenizer.Advance();
                    next = _tokenizer.CurSymbol.Code == TokenCode.PyYield ? ParseYieldExpr() : ParseTestListStarExpr();
                }

                return new AnnAssignStatementNode(start, _tokenizer.CurPosition, left, symbol, right, symbol2, next);
            }
            case TokenCode.PyAssign:
            {
                var res = left as Node;
                while (_tokenizer.CurSymbol.Code == TokenCode.PyAssign)
                {
                    symbol = _tokenizer.CurSymbol;
                    _tokenizer.Advance();
                    right = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true);
                    res = new AssignmentStatementNode(start, _tokenizer.CurPosition, res, symbol, right);
                }

                if (_tokenizer.CurSymbol.Code == TokenCode.TypeComment)
                {
                    var node = res as AssignmentStatementNode;
                    if (node != null) node.TypeComment = _tokenizer.CurSymbol;
                    _tokenizer.Advance();
                }

                return (StatementNode) res;
            }
            default:
                return new ExpressionStatementNode(start, _tokenizer.CurPosition, left);
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private ExpressionNode ParseTestListStarExpr()
    {
        var start = _tokenizer.CurPosition;
        var firstNode = _tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true);

        if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
        {
            var nodes = new List<ExpressionNode>();
            var separators = new List<Token>();
            nodes.Add(firstNode);

            while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
            {
                separators.Add(_tokenizer.CurSymbol);
                _tokenizer.Advance();
                if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
                    throw new SyntaxError("unexpected ',' in expression list!", _tokenizer.CurPosition);
                if (_tokenizer.CurSymbol.Code == TokenCode.PyPlusAssign ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyMinusAssign ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyMulAssign ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyPowerAssign ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyModuloAssign ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyMatriceAssign ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyBitAndAssign ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyBitOrAssign ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyBitXorAssign ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyShiftLeftAssign ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyShiftRightAssign ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyDivAssign ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyFloorDivAssign ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyColon ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyAssign ||
                    _tokenizer.CurSymbol.Code == TokenCode.PySemiColon ||
                    _tokenizer.CurSymbol.Code == TokenCode.Newline ||
                    _tokenizer.CurSymbol.Code == TokenCode.PyRightParen) break;
                nodes.Add(_tokenizer.CurSymbol.Code == TokenCode.PyMul ? ParseStarExpr() : ParseTest(true));
            }
            
            return new TestListExpressionNode(start, _tokenizer.CurPosition, nodes.ToImmutableArray(),
                separators.ToImmutableArray());
        }
        
        return firstNode;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private StatementNode ParseDelStmt()
    {
        var start = _tokenizer.CurPosition;
        var symbol = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var right = ParseExprList();
        
        return new DelStatementNode(start, _tokenizer.CurPosition, symbol, right);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private StatementNode ParsePassStmt()
    {
        var start = _tokenizer.CurPosition;
        var symbol = _tokenizer.CurSymbol;
        _tokenizer.Advance();

        return new PassStatementNode(start, _tokenizer.CurPosition, symbol);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private StatementNode ParseFlowStmt()
    {
        switch (_tokenizer.CurSymbol.Code)
        {
            case TokenCode.PyBreak:
            {
                if (_FlowLevel <= 0) 
                    throw new SyntaxError("Using 'break' outside of loop statement!", _tokenizer.CurPosition);
                return ParseBreakStmt();
            }
            case TokenCode.PyContinue:
            {
                if (_FlowLevel <= 0) 
                    throw new SyntaxError("Using 'continue' outside of loop statement!", _tokenizer.CurPosition);
                return ParseBreakStmt();
            }
            case TokenCode.PyYield:
            {
                if (_FlowLevel <= 0) 
                    throw new SyntaxError("Using 'yield' outside of loop statement!", _tokenizer.CurPosition);
                return ParseYieldStmt();
            }
            case TokenCode.PyReturn:
            {
                if (_FuncLevel <= 0) 
                    throw new SyntaxError("Using 'return' outside of function declaration!", _tokenizer.CurPosition);
                return ParseReturnStmt();
            }
            case TokenCode.PyRaise: return ParseRaiseStmt();
            default:
                throw new SyntaxError("Internal Parser Error! Should not happend!", _tokenizer.CurPosition);
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private StatementNode ParseBreakStmt()
    {
        var start = _tokenizer.CurPosition;
        var symbol = _tokenizer.CurSymbol;
        _tokenizer.Advance();

        return new BreakStatementNode(start, _tokenizer.CurPosition, symbol);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private StatementNode ParseContinueStmt()
    {
        var start = _tokenizer.CurPosition;
        var symbol = _tokenizer.CurSymbol;
        _tokenizer.Advance();

        return new ContinueStatementNode(start, _tokenizer.CurPosition, symbol);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private StatementNode ParseYieldStmt()
    {
        var start = _tokenizer.CurPosition;
        var right = ParseYieldExpr();
        
        return new YieldStatementNode(start, _tokenizer.CurPosition, right);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private StatementNode ParseRaiseStmt()
    {
        var start = _tokenizer.CurPosition;
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        ExpressionNode? left = null, right = null;
        Token? symbol2 = null;
        if (_tokenizer.CurSymbol.Code != TokenCode.Newline && _tokenizer.CurSymbol.Code != TokenCode.PySemiColon)
        {
            left = ParseTest(true);
            if (_tokenizer.CurSymbol.Code == TokenCode.PyFrom)
            {
                symbol2 = _tokenizer.CurSymbol;
                _tokenizer.Advance();
                right = ParseTest(true);
            }
        }
        
        return new RaiseStatementNode(start, _tokenizer.CurPosition, symbol1, left, symbol2, right);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private StatementNode ParseReturnStmt()
    {
        var start = _tokenizer.CurPosition;
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();

        if (_tokenizer.CurSymbol.Code != TokenCode.Newline && _tokenizer.CurSymbol.Code == TokenCode.PySemiColon)
        {
            var right = ParseTestListStarExpr();

            return new ReturnStatementNode(start, _tokenizer.CurPosition, symbol1, right);

        }

        return new ReturnStatementNode(start, _tokenizer.CurPosition, symbol1, null);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private StatementNode ParseImportStmt()
    {
        return _tokenizer.CurSymbol.Code == TokenCode.PyImport ? ParseImportName() : ParseImportFrom();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private StatementNode ParseImportName()
    {
        var start = _tokenizer.CurPosition;
        var symbol = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var right = ParseDottedAsNames();

        return new ImportNameStatementNode(start, _tokenizer.CurPosition, symbol, right);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private StatementNode ParseImportFrom()
    {
        var start = _tokenizer.CurPosition;
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var dots = new List<Token>();
        while (_tokenizer.CurSymbol.Code == TokenCode.PyDot || _tokenizer.CurSymbol.Code == TokenCode.PyElipsis)
        {
            dots.Add(_tokenizer.CurSymbol);
            _tokenizer.Advance();
        }

        if (dots.Count == 0 && _tokenizer.CurSymbol.Code == TokenCode.PyImport)
            throw new SyntaxError("Found 'import' in 'from' statement without name or dots!", _tokenizer.CurPosition);

        var left = _tokenizer.CurSymbol.Code != TokenCode.PyImport ? ParseDottedName() : null;
        if (_tokenizer.CurSymbol.Code != TokenCode.PyImport)
            throw new SyntaxError("Expecting 'import' in 'from' statement!", _tokenizer.CurPosition);
        var symbol2 = _tokenizer.CurSymbol;
        _tokenizer.Advance();

        Token? symbol3 = null, symbol4 = null;
        StatementNode? right = null;

        switch (_tokenizer.CurSymbol.Code)
        {
            case TokenCode.PyMul:
                symbol3 = _tokenizer.CurSymbol;
                _tokenizer.Advance();
                break;
            case TokenCode.PyLeftParen:
                symbol3 = _tokenizer.CurSymbol;
                _tokenizer.Advance();
                right = ParseImportAsNames();
                if (_tokenizer.CurSymbol.Code != TokenCode.PyRightParen)
                    throw new SyntaxError("Missing ')' in 'import' statement!", _tokenizer.CurPosition);
                symbol4 = _tokenizer.CurSymbol;
                _tokenizer.Advance();
                break;
            default:
                right = ParseImportAsNames();
                break;
        }

        return new ImportFromStatementNode(start, _tokenizer.CurPosition, symbol1, left, dots.ToImmutableArray(), 
            symbol2, symbol3, right, symbol4);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private StatementNode ParseImportAsName()
    {
        var start = _tokenizer.CurPosition;
        if (_tokenizer.CurSymbol.Code != TokenCode.Name) 
            throw new SyntaxError("Expecting Name literal in 'import' statement!", _tokenizer.CurPosition);
        var left = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        if (_tokenizer.CurSymbol.Code == TokenCode.PyAs)
        {
            var symbol = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            if (_tokenizer.CurSymbol.Code != TokenCode.Name) 
                throw new SyntaxError("Expecting Name literal in 'import' statement after 'as'!", _tokenizer.CurPosition);
            var right = _tokenizer.CurSymbol;
            _tokenizer.Advance();

            return new ImportAsNameStatementNode(start, _tokenizer.CurPosition, left, symbol, right);
        }

        return new NameLiteralStatementNode(start, _tokenizer.CurPosition, left);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private StatementNode ParseDottedAsName()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseDottedName();
        if (_tokenizer.CurSymbol.Code == TokenCode.PyAs)
        {
            var symbol = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            if (_tokenizer.CurSymbol.Code != TokenCode.Name) 
                throw new SyntaxError("Expecting Name literal in 'import' statement after 'as'!", _tokenizer.CurPosition);
            var right = _tokenizer.CurSymbol;
            _tokenizer.Advance();

            return new DottedAsNameStatementNode(start, _tokenizer.CurPosition, left, symbol, right);
        }

        return left;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private StatementNode ParseImportAsNames()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseImportAsName();
        if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
        {
            var separators = new List<Token>();
            var nodes = new List<StatementNode>();
            nodes.Add(left);
            while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
            {
                separators.Add(_tokenizer.CurSymbol);
                _tokenizer.Advance();
                nodes.Add(ParseImportAsName());
            }

            return new ImportAsNamesStatementNode(start, _tokenizer.CurPosition,
                nodes.ToImmutableArray(), separators.ToImmutableArray());
        }
        
        return left;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private StatementNode ParseDottedAsNames()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseDottedAsName();
        if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
        {
            var separators = new List<Token>();
            var nodes = new List<StatementNode>();
            nodes.Add(left);
            while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
            {
                separators.Add(_tokenizer.CurSymbol);
                _tokenizer.Advance();
                nodes.Add(ParseDottedAsName());
            }

            return new DottedAsNamesStatementNode(start, _tokenizer.CurPosition,
                nodes.ToImmutableArray(), separators.ToImmutableArray());
        }
        
        return left;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private StatementNode ParseDottedName()
    {
        var start = _tokenizer.CurPosition;
        if (_tokenizer.CurSymbol.Code != TokenCode.Name) 
            throw new SyntaxError("Expecting Name literal in 'import' statement!", _tokenizer.CurPosition);
        var nodes = new List<Token>();
        var separators = new List<Token>();
        nodes.Add(_tokenizer.CurSymbol);
        _tokenizer.Advance();

        while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
        {
            separators.Add(_tokenizer.CurSymbol);
            _tokenizer.Advance();
            if (_tokenizer.CurSymbol.Code != TokenCode.Name) 
                throw new SyntaxError("Expecting Name literal in 'import' statement after ','!", _tokenizer.CurPosition);
            
            nodes.Add(_tokenizer.CurSymbol);
            _tokenizer.Advance();
        }

        return new DottedNameStatementNode(start, _tokenizer.CurPosition, 
            nodes.ToImmutableArray(), separators.ToImmutableArray());
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private StatementNode ParseGlobalStmt()
    {
        var start = _tokenizer.CurPosition;
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var nodes = new List<Token>();
        var separators = new List<Token>();
        if (_tokenizer.CurSymbol.Code != TokenCode.Name) 
            throw new SyntaxError("Expecting Name literal in 'global' statement!", _tokenizer.CurPosition);
        var node = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        nodes.Add(node);
        while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
        {
            separators.Add(_tokenizer.CurSymbol);
            _tokenizer.Advance();
            if (_tokenizer.CurSymbol.Code != TokenCode.Name) 
                throw new SyntaxError("Expecting Name literal in 'global' statement!", _tokenizer.CurPosition);
            node = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            nodes.Add(node);
        }

        return new GlobalStatementNode(start, _tokenizer.CurPosition, nodes.ToImmutableArray(),
            separators.ToImmutableArray());
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private StatementNode ParseNonlocalStmt()
    {
        var start = _tokenizer.CurPosition;
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var nodes = new List<Token>();
        var separators = new List<Token>();
        if (_tokenizer.CurSymbol.Code != TokenCode.Name) 
            throw new SyntaxError("Expecting Name literal in 'nonlocal' statement!", _tokenizer.CurPosition);
        var node = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        nodes.Add(node);
        while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
        {
            separators.Add(_tokenizer.CurSymbol);
            _tokenizer.Advance();
            if (_tokenizer.CurSymbol.Code != TokenCode.Name) 
                throw new SyntaxError("Expecting Name literal in 'nonlocal' statement!", _tokenizer.CurPosition);
            node = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            nodes.Add(node);
        }

        return new NonlocalStatementNode(start, _tokenizer.CurPosition, nodes.ToImmutableArray(),
            separators.ToImmutableArray());
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private StatementNode ParseAssertStmt()
    {
        var start = _tokenizer.CurPosition;
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var left = ParseTest(true);
        if (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
        {
            var symbol2 = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            var right = ParseTest(true);

            return new AssertStatementNode(start, _tokenizer.CurPosition, symbol1, left, symbol2, right);
        }

        return new AssertStatementNode(start, _tokenizer.CurPosition, symbol1, left, null, null);
    }
    
    private StatementNode ParseCompoundStmt()
    {
        StatementNode? res = null;
        
        switch (_tokenizer.CurSymbol.Code)
        {
            case TokenCode.PyIf:
                res = ParseIfStatement();
                break;
            case TokenCode.PyFor:
                res = ParseForStatement();
                break;
            case TokenCode.PyWhile:
                res = ParseWhileStatement();
                break;
            case TokenCode.PyTry:
                res = ParseTryStatement();
                break;
            case TokenCode.PyWith:
                res = ParseWithStatement();
                break;
            case TokenCode.PyAsync:
                res = ParseAsyncStatement();
                break;
            case TokenCode.PyDef:
                res = ParseFuncDefStatement();
                break;
            case TokenCode.PyClass:
                res = ParseClassStatement();
                break;
            case TokenCode.PyMatrice:
                res = ParseDecoratedStatement();
                break;
            default:
                throw new NotImplementedException(); // match is comming here!
        }

        return res;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private StatementNode ParseIfStatement()
    {
        var start = _tokenizer.CurPosition;
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var left = ParseNamedExpr();
        if (_tokenizer.CurSymbol.Code != TokenCode.PyColon)
            throw new SyntaxError("Expecting ':' in 'if' statement!", _tokenizer.CurPosition);
        var symbol2 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var right = ParseSuiteStatement();
        if (_tokenizer.CurSymbol.Code == TokenCode.PyElif || _tokenizer.CurSymbol.Code == TokenCode.PyElse)
        {
            var nodes = new List<StatementNode>();
            
            while (_tokenizer.CurSymbol.Code == TokenCode.PyElif)
            {
                var start2 = _tokenizer.CurPosition;
                var symbol3 = _tokenizer.CurSymbol;
                _tokenizer.Advance();
                var left2 = ParseNamedExpr();
                if (_tokenizer.CurSymbol.Code != TokenCode.PyColon)
                    throw new SyntaxError("Expecting ':' in 'elif' statement!", _tokenizer.CurPosition);
                var symbol4 = _tokenizer.CurSymbol;
                _tokenizer.Advance();
                var righ2 = ParseSuiteStatement();
                
                nodes.Add(new ElifStatementNode(start2, _tokenizer.CurPosition, symbol3, left2, 
                    symbol4, righ2));
            }

            var elseNode = _tokenizer.CurSymbol.Code == TokenCode.PyElse ? ParseElseStatement() : null;
            
            return new IfStatementNode(start, _tokenizer.CurPosition, symbol1, left, symbol2, right, 
                nodes.ToImmutableArray(), elseNode);
        }
        
        return new IfStatementNode(start, _tokenizer.CurPosition, symbol1, left, symbol2, right, 
            new ImmutableArray<StatementNode>(), null);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private StatementNode ParseElseStatement()
    {
        var start = _tokenizer.CurPosition;
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        if (_tokenizer.CurSymbol.Code != TokenCode.PyColon)
            throw new SyntaxError("Expecting ':' in 'else' statement!", _tokenizer.CurPosition);
        var symbol2 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var right = ParseSuiteStatement();

        return new ElseStatementNode(start, _tokenizer.CurPosition, symbol1, symbol2, right);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private StatementNode ParseForStatement()
    {
        var start = _tokenizer.CurPosition;
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var left = ParseExprList();
        if (_tokenizer.CurSymbol.Code != TokenCode.PyColon)
            throw new SyntaxError("Expecting 'in' in 'for' statement!", _tokenizer.CurPosition);
        var symbol2 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var right = ParseTestList();
        if (_tokenizer.CurSymbol.Code != TokenCode.PyColon)
            throw new SyntaxError("Expecting ':' in 'for' statement!", _tokenizer.CurPosition);
        var symbol3 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        Token? symbol4 = null;
        if (_tokenizer.CurSymbol.Code == TokenCode.TypeComment)
        {
            symbol4 = _tokenizer.CurSymbol;
            _tokenizer.Advance();
        }
        var next = ParseSuiteStatement();
        var elseNode = _tokenizer.CurSymbol.Code == TokenCode.PyElse ? ParseElseStatement() : null;

        return new ForStatementNode(start, _tokenizer.CurPosition, symbol1, left, symbol2, right, 
            symbol4, symbol3, next, elseNode);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private StatementNode ParseWhileStatement()
    {
        var start = _tokenizer.CurPosition;
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var left = ParseTest(true);
        if (_tokenizer.CurSymbol.Code != TokenCode.PyColon)
            throw new SyntaxError("Expecting ':' in 'while' statement!", _tokenizer.CurPosition);
        var symbol2 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var right = ParseSuiteStatement();
        var next = _tokenizer.CurSymbol.Code == TokenCode.PyElse ? ParseElseStatement() : null;

        return new WhileStatementNode(start, _tokenizer.CurPosition, symbol1, left, symbol2, right, next);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private StatementNode ParseTryStatement()
    {
        var start = _tokenizer.CurPosition;
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        if (_tokenizer.CurSymbol.Code != TokenCode.PyColon)
            throw new SyntaxError("Expecting ':' in 'try' statement!", _tokenizer.CurPosition);
        var symbol2 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var left = ParseSuiteStatement();
        var nodes = new List<StatementNode>();
        if (_tokenizer.CurSymbol.Code == TokenCode.PyFinally) goto _finally;
        
        if (_tokenizer.CurSymbol.Code != TokenCode.PyExcept)
            throw new SyntaxError("Expecting one or more 'except' statements in 'try' statement!", _tokenizer.CurPosition);
        while (_tokenizer.CurSymbol.Code == TokenCode.PyExcept)
        {
            var start3 = _tokenizer.CurPosition;
            var left2 = ParseExceptClauseStatement();
            if (_tokenizer.CurSymbol.Code != TokenCode.PyFinally)
                throw new SyntaxError("Expecting ':' in 'except' statement!", _tokenizer.CurPosition);
            var symbol5 = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            var right2 = ParseSuiteStatement();

            nodes.Add(new ExceptStatementNode(start3, _tokenizer.CurPosition, left2, symbol5, right2));
        }

_finally:
        var elseNode = _tokenizer.CurSymbol.Code == TokenCode.PyElse ? ParseElseStatement() : null;

        StatementNode? fin = null;
        if (_tokenizer.CurSymbol.Code == TokenCode.PyFinally)
        {
            var start2 = _tokenizer.CurPosition;
            var symbol3 = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            if (_tokenizer.CurSymbol.Code != TokenCode.PyFinally)
                throw new SyntaxError("Expecting ':' in 'finally' statement!", _tokenizer.CurPosition);
            var symbol4 = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            var right = ParseSuiteStatement();
            fin = new FinallyStatementNode(start2, _tokenizer.CurPosition, symbol3, 
                symbol4, right);
        }

        return new TryStatementNode(start, _tokenizer.CurPosition, symbol1, symbol2, left,
            nodes.ToImmutableArray(), elseNode, fin);
    }
    
    private StatementNode ParseExceptClauseStatement()
    {
        var start = _tokenizer.CurPosition;
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        ExpressionNode? left = null;
        Token? symbol2 = null, symbol3 = null;
        if (_tokenizer.CurSymbol.Code != TokenCode.PyColon)
        {
            left = ParseTest(true);
            if (_tokenizer.CurSymbol.Code == TokenCode.PyAs)
            {
                symbol2 = _tokenizer.CurSymbol;
                _tokenizer.Advance();
                if (_tokenizer.CurSymbol.Code != TokenCode.Name)
                    throw new SyntaxError("Expecting Name literal after 'as' in 'except' clause!", _tokenizer.CurPosition);
                symbol3 = _tokenizer.CurSymbol;
                _tokenizer.Advance();
            }
        }

        return new ExceptClauseStatementNode(start, _tokenizer.CurPosition, symbol1, left, symbol2, symbol3);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private StatementNode ParseWithStatement()
    {
        var start = _tokenizer.CurPosition;
        var symbol1 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        var nodes = new List<StatementNode>();
        var separators = new List<Token>();
        nodes.Add(ParseWithItemStatement());
        while (_tokenizer.CurSymbol.Code == TokenCode.PyComma)
        {
            separators.Add(_tokenizer.CurSymbol);
            _tokenizer.Advance();
            nodes.Add(ParseWithItemStatement());
        }

        if (_tokenizer.CurSymbol.Code != TokenCode.PyColon)
            throw new SyntaxError("Expecting ':' in 'with' statement!", _tokenizer.CurPosition);
        var symbol2 = _tokenizer.CurSymbol;
        _tokenizer.Advance();
        Token? typeComment = null;
        if (_tokenizer.CurSymbol.Code == TokenCode.TypeComment)
        {
            typeComment = _tokenizer.CurSymbol;
            _tokenizer.Advance();
        }

        var right = ParseSuiteStatement();

        return new WithStatementNode(start, _tokenizer.CurPosition, symbol1,
            nodes.ToImmutableArray(), separators.ToImmutableArray(), symbol2, typeComment, right);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private StatementNode ParseWithItemStatement()
    {
        var start = _tokenizer.CurPosition;
        var left = ParseTest(true);
        Token? symbol1 = null;
        ExpressionNode? right = null;
        if (_tokenizer.CurSymbol.Code == TokenCode.PyAs)
        {
            symbol1 = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            right = ParseBitwiseOr();
        }

        return new WithItemStatementNode(start, _tokenizer.CurPosition, left, symbol1, right);
    }
    
    private StatementNode ParseFuncDefStatement()
    {
        throw new NotImplementedException();
    }
    
    private StatementNode ParseDecoratedStatement()
    {
        throw new NotImplementedException();
    }
    
    private StatementNode ParseAsyncStatement()
    {
        throw new NotImplementedException();
    }
    
    private StatementNode ParseClassStatement()
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="SyntaxError"></exception>
    private StatementNode ParseSuiteStatement()
    {
        if (_tokenizer.CurSymbol.Code == TokenCode.Newline)
        {
            var start = _tokenizer.CurPosition;
            var symbol1 = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            if (_tokenizer.CurSymbol.Code == TokenCode.Indent)
                throw new SyntaxError("Expecting indentation of code block!", _tokenizer.CurPosition);
            var symbol2 = _tokenizer.CurSymbol;
            _tokenizer.Advance();
            var nodes = new List<StatementNode>();
            nodes.Add(ParseStmt());
            while (_tokenizer.CurSymbol.Code != TokenCode.Dedent) nodes.Add(ParseStmt());
            var symbol3 = _tokenizer.CurSymbol;
            _tokenizer.Advance();

            return new SuiteStatementNode(start, _tokenizer.CurPosition, symbol1, symbol2, 
                nodes.ToImmutableArray(), symbol3);
        }

        return ParseSimpleStmt();
    }
    
    #endregion
}