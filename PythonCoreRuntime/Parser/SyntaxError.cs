using System.Runtime.Serialization;

namespace PythonCoreRuntime.Parser;


public class SyntaxError : Exception
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //
    
    public int Position { get; init; }

    public SyntaxError()
    {
        Position = -1;
    }

    public SyntaxError(string message, int position) : base(message)
    {
        Position = position;
    }

    public SyntaxError(string message, int position, Exception inner) : base(message, inner)
    {
        Position = position;
    }

    protected SyntaxError(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}