using System.Collections.Immutable;
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



    private ExpressionNode ParseTrailer()
    {
        throw new NotImplementedException();
    }
    
    #endregion
}