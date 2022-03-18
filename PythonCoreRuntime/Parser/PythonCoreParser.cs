using System.Collections.Immutable;
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
            case TokenCode.PyLeftBracket:
            case TokenCode.PyLeftCurly:
                throw new NotImplementedException();
            
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
                return new UnaryBitInverExpressionNode(start, _tokenizer.CurPosition, symbol, right3);
                
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



    private ExpressionNode ParseTrailer()
    {
        throw new NotImplementedException();
    }
    
    #endregion
}