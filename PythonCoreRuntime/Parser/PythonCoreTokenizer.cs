using System.Collections.Immutable;
using Microsoft.CodeAnalysis;


namespace PythonCoreRuntime.Parser;

public interface IPythonCoreTokenizer
{
    public Token CurSymbol { get; set; }
    public int CurPosition { get; set; }
    
    public void Advance();
}

/// <summary>
///     Tokenizer for Python Grammar 3.10
/// </summary>
public class PythonCoreTokenizer : IPythonCoreTokenizer
{
    public Token CurSymbol { get; set; } = new EofToken();
    public int CurPosition { get; set; }

    private int _index = 0;

    private ImmutableDictionary<string, TokenCode> _keywords = ImmutableDictionary<string, TokenCode>.Empty
        .Add("False", TokenCode.PyFalse)
        .Add("None", TokenCode.PyNone)
        .Add("True", TokenCode.PyTrue)
        .Add("and", TokenCode.PyAnd)
        .Add("as", TokenCode.PyAs)
        .Add("assert", TokenCode.PyAssert)
        .Add("async", TokenCode.PyAsync)
        .Add("await", TokenCode.PyAwait)
        .Add("break", TokenCode.PyBreak)
        .Add("class", TokenCode.PyClass)
        .Add("continue", TokenCode.PyContinue)
        .Add("def", TokenCode.PyDef)
        .Add("del", TokenCode.PyDel)
        .Add("elif", TokenCode.PyElif)
        .Add("else", TokenCode.PyElse)
        .Add("except", TokenCode.PyExcept)
        .Add("finally", TokenCode.PyFinally)
        .Add("for", TokenCode.PyFor)
        .Add("from", TokenCode.PyFrom)
        .Add("global", TokenCode.PyGlobal)
        .Add("if", TokenCode.PyIf)
        .Add("import", TokenCode.PyImport)
        .Add("in", TokenCode.PyIn)
        .Add("is", TokenCode.PyIs)
        .Add("lambda", TokenCode.PyLambda)
        .Add("nonlocal", TokenCode.PyNonlocal)
        .Add("not", TokenCode.PyNot)
        .Add("or", TokenCode.PyOr)
        .Add("pass", TokenCode.PyPass)
        .Add("raise", TokenCode.PyRaise)
        .Add("return", TokenCode.PyReturn)
        .Add("try", TokenCode.PyTry)
        .Add("while", TokenCode.PyWhile)
        .Add("with", TokenCode.PyWith)
        .Add("yield", TokenCode.PyYield);


    /// <summary>
    ///     Checks for operators or delimiters in Python and forwards index to next character for analyzing.
    /// </summary>
    /// <param name="ch1"> This character is already collected and index forwarded one spot </param>
    /// <param name="ch2"> Peek current index position </param>
    /// <param name="ch3"> Peek next index position </param>
    /// <param name="trivias"> Whitespace collected in front of this token </param>
    /// <returns> Optional found Token or default </returns>
    private Optional<Token> GetOperatorOrDelimiter(char ch1, char ch2, char ch3, ImmutableArray<Trivia> trivias)
    {
        Optional<Token> res = default;

        switch (ch1, ch2, ch3)
        {
            case ( '*', '*', '=' ):
                _index += 2;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyPowerAssign, trivias));
                break;
            case ( '*', '*', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyPower, trivias));
                break;
            case ( '*', '=', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyMulAssign, trivias));
                break;
            case ( '*', _ , _ ):
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyMul, trivias));
                break;
            case ( '/', '/', '=' ):
                _index += 2;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyFloorDivAssign, trivias));
                break;
            case ( '/', '/', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyFloorDiv, trivias));
                break;
            case ( '/', '=', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyDivAssign, trivias));
                break;
            case ( '/', _ , _ ):
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyDiv, trivias));
                break;
            case ( '<', '<', '=' ):
                _index += 2;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyShiftLeftAssign, trivias));
                break;
            case ( '<', '<', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyShiftLeft, trivias));
                break;
            case ( '<', '=', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyLessEqual, trivias));
                break;
            case ( '<', '>', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyNotEqual, trivias));
                break;
            case ( '<', _ , _ ):
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyLess, trivias));
                break;
            case ( '>', '>', '=' ):
                _index += 2;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyShiftRightAssign, trivias));
                break;
            case ( '>', '>', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyShiftRight, trivias));
                break;
            case ( '>', '=', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyGreaterEqual, trivias));
                break;
            case ( '>', _ , _ ):
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyGreater, trivias));
                break;
            case ( '.', '.', '.' ):
                _index += 2;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyElipsis, trivias));
                break;
            case ( '.', _ , _ ):
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyDot, trivias));
                break;
            case ( '+', '=', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyPlusAssign, trivias));
                break;
            case ( '+', _ , _ ):
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyPlus, trivias));
                break;
            case ( '-', '=', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyMinusAssign, trivias));
                break;
            case ( '-', '>', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyArrow, trivias));
                break;
            case ( '-', _ , _ ):
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyMinus, trivias));
                break;
            case ( '%', '=', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyModuloAssign, trivias));
                break;
            case ( '%', _ , _ ):
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyModulo, trivias));
                break;
            case ( '&', '=', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyBitAndAssign, trivias));
                break;
            case ( '&', _ , _ ):
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyBitAnd, trivias));
                break;
            case ( '|', '=', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyBitOrAssign, trivias));
                break;
            case ( '|', _ , _ ):
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyBitOr, trivias));
                break;
            case ( '^', '=', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyBitXorAssign, trivias));
                break;
            case ( '^', _ , _ ):
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyBitXor, trivias));
                break;
            case ( '@', '=', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyMatriceAssign, trivias));
                break;
            case ( '@', _ , _ ):
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyMatrice, trivias));
                break;
            case ( '=', '=', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyEqual, trivias));
                break;
            case ( '=', _ , _ ):
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyAssign, trivias));
                break;
            case ( ':', '=', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyColonAssign, trivias));
                break;
            case ( ':', _ , _ ):
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyColon, trivias));
                break;
            case ( '!', '=', _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyNotEqual, trivias));
                break;
            case ( '~', _ , _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyBitInvert, trivias));
                break;
            case ( ',', _ , _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyComma, trivias));
                break;
            case ( ';', _ , _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PySemiColon, trivias));
                break;
            case ( '(', _ , _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyLeftParen, trivias));
                break;
            case ( '[', _ , _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyLeftBracket, trivias));
                break;
            case ( '{', _ , _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyLeftCurly, trivias));
                break;
            case ( ')', _ , _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyRightParen, trivias));
                break;
            case ( ']', _ , _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyRightBracket, trivias));
                break;
            case ( '}', _ , _ ):
                _index++;
                res = new Optional<Token>(new Token(CurPosition, _index, 
                    TokenCode.PyRightCurly, trivias));
                break;
            default:
                _index--;
                CurPosition = _index;
                break;
        }
        
        return res;
    }


       
    
    
    
    /// <summary>
    ///     Get next valid token into CurSymbol and start of it into CurPosition!
    /// </summary>
    public void Advance()
    {

    }

}