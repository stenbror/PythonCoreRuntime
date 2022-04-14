using System.Collections.Immutable;

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
    ///     Get next valid token into CurSymbol and start of it into CurPosition!
    /// </summary>
    public void Advance()
    {

    }

}