using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Xml;
using PythonCoreRuntime.Parser.AST;

namespace PythonCoreRuntime.Parser;

public class PythonCoreParser
{
    readonly IPythonCoreTokenizer _tokenizer;
    
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
                    throw right is DictionaryExpressionNode || right == null
                        ? new SyntaxError("Expecting '}' in dictionary!", _tokenizer.CurPosition)
                        : new SyntaxError("Expecting '}' in set!", _tokenizer.CurPosition);
                }

                var symbol2 = _tokenizer.CurSymbol;
                _tokenizer.Advance();
                
                return right is DictionaryExpressionNode || right == null ? 
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
        if (_tokenizer.CurSymbol.Code != TokenCode.PyIf)
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
    
    private ExpressionNode ParseDictorSetMaker()
    {
        throw new NotImplementedException();
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
    
    
    private ExpressionNode ParseVarArgsList()
    {
        throw new NotImplementedException();
    }
    
    #endregion
    
    
    #region Statement Rules

    private StatementNode ParseTestListStarExpr()
    {
        throw new NotImplementedException();
    }
    
    #endregion
}