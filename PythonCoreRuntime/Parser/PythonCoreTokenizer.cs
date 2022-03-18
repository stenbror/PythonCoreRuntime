namespace PythonCoreRuntime.Parser;

public interface IPythonCoreTokenizer
{
    public Token CurSymbol { get; set; }
    public int CurPosition { get; set; }

    public void Advance();
}

public class PythonCoreTokenizer : IPythonCoreTokenizer
{
    public Token CurSymbol { get; set; } = new EofToken();
    public int CurPosition { get; set; }

    public void Advance()
    {
        
    }
}