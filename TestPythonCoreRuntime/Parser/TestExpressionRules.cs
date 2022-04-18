using System.Collections.Generic;
using System.Collections.Immutable;
using Xunit;
using PythonCoreRuntime.Parser;
using PythonCoreRuntime.Parser.AST;

namespace TestPythonCoreRuntime.Parser;

#pragma warning disable CS8602

/// <summary>
///     Mocked PythonCoreTokenizer for UnitTesting of Parser alone!
/// </summary>
internal class MockPythonCoreTokenizer : IPythonCoreTokenizer
{
    private int _index;
    private ImmutableArray<Token> _tokens;

    public MockPythonCoreTokenizer(ImmutableArray<Token> tokens)
    {
        _tokens = tokens;
        _index = -1;
        CurSymbol = tokens[0];
    }

    public Token CurSymbol
    {
        get => _index < _tokens.Length
            ? _tokens[_index]
            : new Token(-1, -1, TokenCode.Eof, ImmutableArray<Trivia>.Empty);
        
        set
        {
            
        }
    }

    public int CurPosition
    {
        get => _index < _tokens.Length ? _tokens[_index].StartPosition : -1;

        set
        {
            
        }
    }

    public void Advance()
    {
        if (_index < (_tokens.Length -1)) _index++;
    }
}



/// <summary>
///     UnitTests for Expression rules of Parser!
/// </summary>
public class TestExpressionRules
{
    
    /// <summary>
    ///     Test for Name literal
    /// </summary>
    [Fact]
    public void TestAtomRuleWithNameLiteral()
    {
        var tokens = new List<Token>()
        {
            new NameToken(0, 1, "a", ImmutableArray<Trivia>.Empty),
            new Token(1, 1, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(1, res.EndPosition);
        
        Assert.Equal(TokenCode.Name, 
            ((res as EvalInputStatementNode).Right as NameExpressionNode).Symbol.Code);
        Assert.Equal("a", 
            (((res as EvalInputStatementNode).Right as NameExpressionNode).Symbol as NameToken).Value);
        Assert.Equal(0, 
            (((res as EvalInputStatementNode).Right as NameExpressionNode).Symbol as NameToken).StartPosition);
        Assert.Equal(1, 
            (((res as EvalInputStatementNode).Right as NameExpressionNode).Symbol as NameToken).EndPosition);
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for Number
    /// </summary>
    [Fact]
    public void TestAtomRuleWithNumberLiteral()
    {
        var tokens = new List<Token>()
        {
            new NumberToken(0, 1, "1", ImmutableArray<Trivia>.Empty),
            new Token(1, 1, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(1, res.EndPosition);
        
        Assert.Equal(TokenCode.Number, 
            ((res as EvalInputStatementNode).Right as NumberExpressionNode).Symbol.Code);
        Assert.Equal("1", 
            (((res as EvalInputStatementNode).Right as NumberExpressionNode).Symbol as NumberToken).Value);
        Assert.Equal(0, 
            (((res as EvalInputStatementNode).Right as NumberExpressionNode).Symbol as NumberToken).StartPosition);
        Assert.Equal(1, 
            (((res as EvalInputStatementNode).Right as NumberExpressionNode).Symbol as NumberToken).EndPosition);
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for single String
    /// </summary>
    [Fact]
    public void TestAtomRuleWithSingleStringLiteral()
    {
        var tokens = new List<Token>()
        {
            new StringToken(0, 15, "'Hello, World!'", ImmutableArray<Trivia>.Empty),
            new Token(15, 15, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(15, res.EndPosition);
        
        Assert.True((((res as EvalInputStatementNode).Right as StringExpressionNode).Symbols.Length) == 1);
        
        Assert.Equal(TokenCode.String, 
            ((res as EvalInputStatementNode).Right as StringExpressionNode).Symbols[0].Code);
        Assert.Equal("'Hello, World!'", 
            (((res as EvalInputStatementNode).Right as StringExpressionNode).Symbols[0] as StringToken).Value);
        Assert.Equal(0, 
            (((res as EvalInputStatementNode).Right as StringExpressionNode).Symbols[0] as StringToken).StartPosition);
        Assert.Equal(15, 
            (((res as EvalInputStatementNode).Right as StringExpressionNode).Symbols[0] as StringToken).EndPosition);
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for multiple String
    /// </summary>
    [Fact]
    public void TestAtomRuleWithMultipleStringLiteral()
    {
        var tokens = new List<Token>()
        {
            new StringToken(0, 15, "'Hello, World!'", ImmutableArray<Trivia>.Empty),
            new StringToken(16, 24, "'Again!'", ImmutableArray<Trivia>.Empty),
            new Token(24, 24, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(24, res.EndPosition);


        Assert.True((((res as EvalInputStatementNode).Right as StringExpressionNode).Symbols.Length) == 2);
        
        Assert.Equal(TokenCode.String, 
            ((res as EvalInputStatementNode).Right as StringExpressionNode).Symbols[0].Code);
        Assert.Equal("'Hello, World!'", 
            (((res as EvalInputStatementNode).Right as StringExpressionNode).Symbols[0] as StringToken).Value);
        Assert.Equal(0, 
            (((res as EvalInputStatementNode).Right as StringExpressionNode).Symbols[0] as StringToken).StartPosition);
        Assert.Equal(15, 
            (((res as EvalInputStatementNode).Right as StringExpressionNode).Symbols[0] as StringToken).EndPosition);
        
        Assert.Equal(TokenCode.String, 
            ((res as EvalInputStatementNode).Right as StringExpressionNode).Symbols[1].Code);
        Assert.Equal("'Again!'", 
            (((res as EvalInputStatementNode).Right as StringExpressionNode).Symbols[1] as StringToken).Value);
        Assert.Equal(16, 
            (((res as EvalInputStatementNode).Right as StringExpressionNode).Symbols[1] as StringToken).StartPosition);
        Assert.Equal(24, 
            (((res as EvalInputStatementNode).Right as StringExpressionNode).Symbols[1] as StringToken).EndPosition);
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for None
    /// </summary>
    [Fact]
    public void TestAtomRuleWithNoneLiteral()
    {
        var tokens = new List<Token>()
        {
            new Token(0, 4, TokenCode.PyNone, ImmutableArray<Trivia>.Empty),
            new Token(4, 4, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(4, res.EndPosition);
        
        Assert.Equal(TokenCode.PyNone, 
            ((res as EvalInputStatementNode).Right as NoneExpressionNode).Symbol.Code);
        Assert.Equal(0, 
            (((res as EvalInputStatementNode).Right as NoneExpressionNode).Symbol).StartPosition);
        Assert.Equal(4, 
            (((res as EvalInputStatementNode).Right as NoneExpressionNode).Symbol).EndPosition);
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for False
    /// </summary>
    [Fact]
    public void TestAtomRuleWithFalseLiteral()
    {
        var tokens = new List<Token>()
        {
            new Token(0, 4, TokenCode.PyFalse, ImmutableArray<Trivia>.Empty),
            new Token(5, 5, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(5, res.EndPosition);
        
        Assert.Equal(TokenCode.PyFalse, 
            ((res as EvalInputStatementNode).Right as FalseExpressionNode).Symbol.Code);
        Assert.Equal(0, 
            (((res as EvalInputStatementNode).Right as FalseExpressionNode).Symbol).StartPosition);
        Assert.Equal(4, 
            (((res as EvalInputStatementNode).Right as FalseExpressionNode).Symbol).EndPosition);
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for True
    /// </summary>
    [Fact]
    public void TestAtomRuleWithTrueLiteral()
    {
        var tokens = new List<Token>()
        {
            new Token(0, 4, TokenCode.PyTrue, ImmutableArray<Trivia>.Empty),
            new Token(5, 5, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(5, res.EndPosition);
        
        Assert.Equal(TokenCode.PyTrue, 
            ((res as EvalInputStatementNode).Right as TrueExpressionNode).Symbol.Code);
        Assert.Equal(0, 
            (((res as EvalInputStatementNode).Right as TrueExpressionNode).Symbol).StartPosition);
        Assert.Equal(4, 
            (((res as EvalInputStatementNode).Right as TrueExpressionNode).Symbol).EndPosition);
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for empty tuple
    /// </summary>
    [Fact]
    public void TestAtomRuleWithEmptyTuple()
    {
        var tokens = new List<Token>()
        {
            new Token(0, 1, TokenCode.PyLeftParen, ImmutableArray<Trivia>.Empty),
            new Token(2, 3, TokenCode.PyRightParen, ImmutableArray<Trivia>.Empty),
            new Token(4, 4, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(4, res.EndPosition);
        
        Assert.Equal(TokenCode.PyLeftParen, 
            ((res as EvalInputStatementNode).Right as TupleExpressionNode).Symbol1.Code);
        
        Assert.Null(((res as EvalInputStatementNode).Right as TupleExpressionNode).Right);
        
        Assert.Equal(TokenCode.PyRightParen, 
            ((res as EvalInputStatementNode).Right as TupleExpressionNode).Symbol2.Code);
        
        Assert.Equal(0, ((res as EvalInputStatementNode).Right as TupleExpressionNode).StartPosition);
        Assert.Equal(4, ((res as EvalInputStatementNode).Right as TupleExpressionNode).EndPosition);
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for empty list
    /// </summary>
    [Fact]
    public void TestAtomRuleWithEmptyList()
    {
        var tokens = new List<Token>()
        {
            new Token(0, 1, TokenCode.PyLeftBracket, ImmutableArray<Trivia>.Empty),
            new Token(2, 3, TokenCode.PyRightBracket, ImmutableArray<Trivia>.Empty),
            new Token(4, 4, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(4, res.EndPosition);
        
        Assert.Equal(TokenCode.PyLeftBracket, 
            ((res as EvalInputStatementNode).Right as ListExpressionNode).Symbol1.Code);
        
        Assert.Null(((res as EvalInputStatementNode).Right as ListExpressionNode).Right);
        
        Assert.Equal(TokenCode.PyRightBracket, 
            ((res as EvalInputStatementNode).Right as ListExpressionNode).Symbol2.Code);
        
        Assert.Equal(0, ((res as EvalInputStatementNode).Right as ListExpressionNode).StartPosition);
        Assert.Equal(4, ((res as EvalInputStatementNode).Right as ListExpressionNode).EndPosition);
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for empty list
    /// </summary>
    [Fact]
    public void TestAtomRuleWithEmptyDictionary()
    {
        var tokens = new List<Token>()
        {
            new Token(0, 1, TokenCode.PyLeftCurly, ImmutableArray<Trivia>.Empty),
            new Token(2, 3, TokenCode.PyRightCurly, ImmutableArray<Trivia>.Empty),
            new Token(4, 4, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(4, res.EndPosition);
        
        Assert.Equal(TokenCode.PyLeftCurly, 
            ((res as EvalInputStatementNode).Right as DictionaryExpressionNode).Symbol1.Code);
        
        Assert.Null(((res as EvalInputStatementNode).Right as DictionaryExpressionNode).Right);
        
        Assert.Equal(TokenCode.PyRightCurly, 
            ((res as EvalInputStatementNode).Right as DictionaryExpressionNode).Symbol2.Code);
        
        Assert.Equal(0, ((res as EvalInputStatementNode).Right as DictionaryExpressionNode).StartPosition);
        Assert.Equal(4, ((res as EvalInputStatementNode).Right as DictionaryExpressionNode).EndPosition);
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for await a
    /// </summary>
    [Fact]
    public void TestAtomExprRuleWithAwait()
    {
        var tokens = new List<Token>()
        {
            new Token(0, 6, TokenCode.PyAwait, ImmutableArray<Trivia>.Empty),
            new NameToken(6, 7, "a", ImmutableArray<Trivia>.Empty),
            new Token(7, 7, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(7, res.EndPosition);
        
        Assert.Equal(TokenCode.PyAwait, 
            ((res as EvalInputStatementNode).Right as AtomExpressionNode).Await.Code);
        
        Assert.Equal(0, 
            ((res as EvalInputStatementNode).Right as AtomExpressionNode).StartPosition);
        Assert.Equal(7, 
            ((res as EvalInputStatementNode).Right as AtomExpressionNode).EndPosition);
        
        Assert.Equal(TokenCode.Name,
            (((res as EvalInputStatementNode).Right as AtomExpressionNode).Right as NameExpressionNode).Symbol.Code );
        Assert.Equal("a",
            ((((res as EvalInputStatementNode).Right as AtomExpressionNode).Right as NameExpressionNode).Symbol as NameToken).Value );
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for await a()
    /// </summary>
    [Fact]
    public void TestAtomExprRuleWithAwaitFunctionCall()
    {
        var tokens = new List<Token>()
        {
            new Token(0, 6, TokenCode.PyAwait, ImmutableArray<Trivia>.Empty),
            new NameToken(6, 7, "a", ImmutableArray<Trivia>.Empty),
            new Token(7, 8, TokenCode.PyLeftParen, ImmutableArray<Trivia>.Empty),
            new Token(8, 9, TokenCode.PyRightParen, ImmutableArray<Trivia>.Empty),
            new Token(9, 9, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(9, res.EndPosition);
        
        Assert.Equal(TokenCode.PyAwait, 
            ((res as EvalInputStatementNode).Right as AtomExpressionNode).Await.Code);
        Assert.True( ((res as EvalInputStatementNode).Right as AtomExpressionNode).Trailers.Length == 1);
        
        var element1 = (((res as EvalInputStatementNode).Right as AtomExpressionNode).Trailers[0] as CallExpressionNode);
        Assert.Equal(TokenCode.PyLeftParen, element1.Symbol1.Code);
        Assert.Null(element1.Right);
        Assert.Equal(TokenCode.PyRightParen, element1.Symbol2.Code);
        Assert.Equal(7, element1.StartPosition);
        Assert.Equal(9, element1.EndPosition);
        
        Assert.Equal(0, 
            ((res as EvalInputStatementNode).Right as AtomExpressionNode).StartPosition);
        Assert.Equal(9, 
            ((res as EvalInputStatementNode).Right as AtomExpressionNode).EndPosition);
        
        Assert.Equal(TokenCode.Name,
            (((res as EvalInputStatementNode).Right as AtomExpressionNode).Right as NameExpressionNode).Symbol.Code );
        Assert.Equal("a",
            ((((res as EvalInputStatementNode).Right as AtomExpressionNode).Right as NameExpressionNode).Symbol as NameToken).Value );
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for a[1]
    /// </summary>
    [Fact]
    public void TestAtomExprRuleWithIndexingSingleValue()
    {
        var tokens = new List<Token>()
        {
            new NameToken(0, 1, "a", ImmutableArray<Trivia>.Empty),
            new Token(1, 2, TokenCode.PyLeftBracket, ImmutableArray<Trivia>.Empty),
            new NumberToken(2, 3, "1", new ImmutableArray<Trivia>()),
            new Token(3, 4, TokenCode.PyRightBracket, ImmutableArray<Trivia>.Empty),
            new Token(4, 4, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(4, res.EndPosition);
        
        Assert.True( ((res as EvalInputStatementNode).Right as AtomExpressionNode).Trailers.Length == 1);
        
        var element1 = (((res as EvalInputStatementNode).Right as AtomExpressionNode).Trailers[0] as IndexExpressionNode);
        Assert.Equal(TokenCode.PyLeftBracket, element1.Symbol1.Code);

        var subscript1 = element1.Right as SubscriptExpressionNode;
        var number = subscript1.Left as NumberExpressionNode;
        Assert.Equal("1", (number.Symbol as NumberToken).Value);
        Assert.Null(subscript1.Right);
        Assert.Null(subscript1.Next);
        
        Assert.Equal(TokenCode.PyRightBracket, element1.Symbol2.Code);
        Assert.Equal(1, element1.StartPosition);
        Assert.Equal(4, element1.EndPosition);
        
        Assert.Equal(0, 
            ((res as EvalInputStatementNode).Right as AtomExpressionNode).StartPosition);
        Assert.Equal(4, 
            ((res as EvalInputStatementNode).Right as AtomExpressionNode).EndPosition);
        
        Assert.Equal(TokenCode.Name,
            (((res as EvalInputStatementNode).Right as AtomExpressionNode).Right as NameExpressionNode).Symbol.Code );
        Assert.Equal("a",
            ((((res as EvalInputStatementNode).Right as AtomExpressionNode).Right as NameExpressionNode).Symbol as NameToken).Value );
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for await a.b()
    /// </summary>
    [Fact]
    public void TestAtomExprRuleWithAwaitDottedNameCall()
    {
        var tokens = new List<Token>()
        {
            new Token(0, 6, TokenCode.PyAwait, ImmutableArray<Trivia>.Empty),
            new NameToken(6, 7, "a", ImmutableArray<Trivia>.Empty),
            new Token(7, 8, TokenCode.PyDot, ImmutableArray<Trivia>.Empty),
            new NameToken(8, 9, "b", ImmutableArray<Trivia>.Empty),
            new Token(9, 10, TokenCode.PyLeftParen, ImmutableArray<Trivia>.Empty),
            new Token(10, 11, TokenCode.PyRightParen, ImmutableArray<Trivia>.Empty),
            new Token(11, 11, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(11, res.EndPosition);
        
        Assert.Equal(TokenCode.PyAwait, 
            ((res as EvalInputStatementNode).Right as AtomExpressionNode).Await.Code);
        
        Assert.Equal(0, 
            ((res as EvalInputStatementNode).Right as AtomExpressionNode).StartPosition);
        Assert.Equal(11, 
            ((res as EvalInputStatementNode).Right as AtomExpressionNode).EndPosition);
        
        Assert.Equal(TokenCode.Name,
            (((res as EvalInputStatementNode).Right as AtomExpressionNode).Right as NameExpressionNode).Symbol.Code );
        Assert.Equal("a",
            ((((res as EvalInputStatementNode).Right as AtomExpressionNode).Right as NameExpressionNode).Symbol as NameToken).Value );
        
        Assert.True( ((res as EvalInputStatementNode).Right as AtomExpressionNode).Trailers.Length == 2);
        
        var element1 = (((res as EvalInputStatementNode).Right as AtomExpressionNode).Trailers[0] as DotNameExpressionNode);
        var element2 = (((res as EvalInputStatementNode).Right as AtomExpressionNode).Trailers[1] as CallExpressionNode);
        
        Assert.Equal(TokenCode.PyDot, element1.Symbol1.Code);
        Assert.Equal("b", (element1.Symbol2 as NameToken).Value);
        
        Assert.Equal(TokenCode.PyLeftParen, element2.Symbol1.Code);
        Assert.Equal(TokenCode.PyRightParen, element2.Symbol2.Code);
        Assert.Null(element2.Right);
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for a ** b
    /// </summary>
    [Fact]
    public void TestPowerOperatorSingle()
    {
        var tokens = new List<Token>()
        {
            new NameToken(0, 1, "a", ImmutableArray<Trivia>.Empty),
            new Token(2, 4, TokenCode.PyPower, ImmutableArray<Trivia>.Empty),
            new NameToken(5, 6, "b", ImmutableArray<Trivia>.Empty),
            new Token(6, 6, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(6, res.EndPosition);
        
        
        var op = ((res as EvalInputStatementNode).Right as PowerExpressionNode);
        Assert.Equal(0, op.StartPosition);
        Assert.Equal(6, op.EndPosition);
        Assert.Equal(TokenCode.PyPower, op.Symbol.Code);

        var left = (op.Left as NameExpressionNode);
        Assert.Equal("a", (left.Symbol as NameToken).Value);

        var right = (op.Right as NameExpressionNode);
        Assert.Equal("b", (right.Symbol as NameToken).Value);
        
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for a ** b ** c
    /// </summary>
    [Fact]
    public void TestPowerOperatorMultiple()
    {
        var tokens = new List<Token>()
        {
            new NameToken(0, 1, "a", ImmutableArray<Trivia>.Empty),
            new Token(2, 4, TokenCode.PyPower, ImmutableArray<Trivia>.Empty),
            new NameToken(5, 6, "b", ImmutableArray<Trivia>.Empty),
            new Token(7, 9, TokenCode.PyPower, ImmutableArray<Trivia>.Empty),
            new NameToken(10, 11, "c", ImmutableArray<Trivia>.Empty),
            new Token(11, 11, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(11, res.EndPosition);
        
        
        var op1 = ((res as EvalInputStatementNode).Right as PowerExpressionNode);
        Assert.Equal(0, op1.StartPosition);
        Assert.Equal(11, op1.EndPosition);
        Assert.Equal(TokenCode.PyPower, op1.Symbol.Code);
        
        var left = (op1.Left as NameExpressionNode);
        Assert.Equal("a", (left.Symbol as NameToken).Value);

        var op2 = (op1.Right as PowerExpressionNode);
        Assert.Equal(5, op2.StartPosition);
        Assert.Equal(11, op2.EndPosition);
        Assert.Equal(TokenCode.PyPower, op2.Symbol.Code);
        
        var left2 = (op2.Left as NameExpressionNode);
        Assert.Equal("b", (left2.Symbol as NameToken).Value);
       
        var right2 = (op2.Right as NameExpressionNode);
        Assert.Equal("c", (right2.Symbol as NameToken).Value);
        
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for +a
    /// </summary>
    [Fact]
    public void TestUnaryPlusSingle()
    {
        var tokens = new List<Token>()
        {
            new Token(0, 1, TokenCode.PyPlus, ImmutableArray<Trivia>.Empty),
            new NameToken(1, 2, "a", ImmutableArray<Trivia>.Empty),
            new Token(2, 2, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(2, res.EndPosition);
        
        
        var op = ((res as EvalInputStatementNode).Right as UnaryPlusExpressionNode);
        Assert.Equal(0, op.StartPosition);
        Assert.Equal(2, op.EndPosition);
        Assert.Equal(TokenCode.PyPlus, op.Symbol.Code);

        var right = (op.Right as NameExpressionNode);
        Assert.Equal("a", (right.Symbol as NameToken).Value);

        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for -a
    /// </summary>
    [Fact]
    public void TestUnaryMinusSingle()
    {
        var tokens = new List<Token>()
        {
            new Token(0, 1, TokenCode.PyMinus, ImmutableArray<Trivia>.Empty),
            new NameToken(1, 2, "a", ImmutableArray<Trivia>.Empty),
            new Token(2, 2, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(2, res.EndPosition);
        
        
        var op = ((res as EvalInputStatementNode).Right as UnaryMinusExpressionNode);
        Assert.Equal(0, op.StartPosition);
        Assert.Equal(2, op.EndPosition);
        Assert.Equal(TokenCode.PyMinus, op.Symbol.Code);

        var right = (op.Right as NameExpressionNode);
        Assert.Equal("a", (right.Symbol as NameToken).Value);

        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for ~a

    /// </summary>
    [Fact]
    public void TestUnaryBitInvertSingle()
    {
        var tokens = new List<Token>()
        {
            new Token(0, 1, TokenCode.PyBitInvert, ImmutableArray<Trivia>.Empty),
            new NameToken(1, 2, "a", ImmutableArray<Trivia>.Empty),
            new Token(2, 2, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(2, res.EndPosition);
        
        
        var op = ((res as EvalInputStatementNode).Right as UnaryBitInvertExpressionNode);
        Assert.Equal(0, op.StartPosition);
        Assert.Equal(2, op.EndPosition);
        Assert.Equal(TokenCode.PyBitInvert, op.Symbol.Code);

        var right = (op.Right as NameExpressionNode);
        Assert.Equal("a", (right.Symbol as NameToken).Value);

        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for --a
    /// </summary>
    [Fact]
    public void TestUnaryMinusMultiple()
    {
        var tokens = new List<Token>()
        {
            new Token(0, 1, TokenCode.PyMinus, ImmutableArray<Trivia>.Empty),
            new Token(1, 2, TokenCode.PyMinus, ImmutableArray<Trivia>.Empty),
            new NameToken(2, 3, "a", ImmutableArray<Trivia>.Empty),
            new Token(3, 3, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(3, res.EndPosition);
        
        
        var op = ((res as EvalInputStatementNode).Right as UnaryMinusExpressionNode);
        Assert.Equal(0, op.StartPosition);
        Assert.Equal(3, op.EndPosition);
        Assert.Equal(TokenCode.PyMinus, op.Symbol.Code);
        
        var op2 = (op.Right as UnaryMinusExpressionNode);
        Assert.Equal(1, op2.StartPosition);
        Assert.Equal(3, op2.EndPosition);
        Assert.Equal(TokenCode.PyMinus, op2.Symbol.Code);
        

        var right = (op2.Right as NameExpressionNode);
        Assert.Equal("a", (right.Symbol as NameToken).Value);

        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for a * b
    /// </summary>
    [Fact]
    public void TestTermMulOperatorSingle()
    {
        var tokens = new List<Token>()
        {
            new NameToken(0, 1, "a", ImmutableArray<Trivia>.Empty),
            new Token(2, 3, TokenCode.PyMul, ImmutableArray<Trivia>.Empty),
            new NameToken(4, 5, "b", ImmutableArray<Trivia>.Empty),
            new Token(5, 5, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(5, res.EndPosition);
        
        
        var op = ((res as EvalInputStatementNode).Right as MulExpressionNode);
        Assert.Equal(0, op.StartPosition);
        Assert.Equal(5, op.EndPosition);
        Assert.Equal(TokenCode.PyMul, op.Symbol.Code);

        var left = (op.Left as NameExpressionNode);
        Assert.Equal("a", (left.Symbol as NameToken).Value);

        var right = (op.Right as NameExpressionNode);
        Assert.Equal("b", (right.Symbol as NameToken).Value);
        
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for a / b
    /// </summary>
    [Fact]
    public void TestTermDivOperatorSingle()
    {
        var tokens = new List<Token>()
        {
            new NameToken(0, 1, "a", ImmutableArray<Trivia>.Empty),
            new Token(2, 3, TokenCode.PyDiv, ImmutableArray<Trivia>.Empty),
            new NameToken(4, 5, "b", ImmutableArray<Trivia>.Empty),
            new Token(5, 5, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(5, res.EndPosition);
        
        
        var op = ((res as EvalInputStatementNode).Right as DivExpressionNode);
        Assert.Equal(0, op.StartPosition);
        Assert.Equal(5, op.EndPosition);
        Assert.Equal(TokenCode.PyDiv, op.Symbol.Code);

        var left = (op.Left as NameExpressionNode);
        Assert.Equal("a", (left.Symbol as NameToken).Value);

        var right = (op.Right as NameExpressionNode);
        Assert.Equal("b", (right.Symbol as NameToken).Value);
        
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for a % b
    /// </summary>
    [Fact]
    public void TestTermModuloOperatorSingle()
    {
        var tokens = new List<Token>()
        {
            new NameToken(0, 1, "a", ImmutableArray<Trivia>.Empty),
            new Token(2, 3, TokenCode.PyModulo, ImmutableArray<Trivia>.Empty),
            new NameToken(4, 5, "b", ImmutableArray<Trivia>.Empty),
            new Token(5, 5, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(5, res.EndPosition);
        
        
        var op = ((res as EvalInputStatementNode).Right as ModuloExpressionNode);
        Assert.Equal(0, op.StartPosition);
        Assert.Equal(5, op.EndPosition);
        Assert.Equal(TokenCode.PyModulo, op.Symbol.Code);

        var left = (op.Left as NameExpressionNode);
        Assert.Equal("a", (left.Symbol as NameToken).Value);

        var right = (op.Right as NameExpressionNode);
        Assert.Equal("b", (right.Symbol as NameToken).Value);
        
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for a @ b
    /// </summary>
    [Fact]
    public void TestTermMatriceOperatorSingle()
    {
        var tokens = new List<Token>()
        {
            new NameToken(0, 1, "a", ImmutableArray<Trivia>.Empty),
            new Token(2, 3, TokenCode.PyMatrice, ImmutableArray<Trivia>.Empty),
            new NameToken(4, 5, "b", ImmutableArray<Trivia>.Empty),
            new Token(5, 5, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(5, res.EndPosition);
        
        
        var op = ((res as EvalInputStatementNode).Right as MatriceExpressionNode);
        Assert.Equal(0, op.StartPosition);
        Assert.Equal(5, op.EndPosition);
        Assert.Equal(TokenCode.PyMatrice, op.Symbol.Code);

        var left = (op.Left as NameExpressionNode);
        Assert.Equal("a", (left.Symbol as NameToken).Value);

        var right = (op.Right as NameExpressionNode);
        Assert.Equal("b", (right.Symbol as NameToken).Value);
        
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for a // b
    /// </summary>
    [Fact]
    public void TestTermFloorDivOperatorSingle()
    {
        var tokens = new List<Token>()
        {
            new NameToken(0, 1, "a", ImmutableArray<Trivia>.Empty),
            new Token(2, 4, TokenCode.PyFloorDiv, ImmutableArray<Trivia>.Empty),
            new NameToken(5, 6, "b", ImmutableArray<Trivia>.Empty),
            new Token(6, 6, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(6, res.EndPosition);
        
        
        var op = ((res as EvalInputStatementNode).Right as FloorDivExpressionNode);
        Assert.Equal(0, op.StartPosition);
        Assert.Equal(6, op.EndPosition);
        Assert.Equal(TokenCode.PyFloorDiv, op.Symbol.Code);

        var left = (op.Left as NameExpressionNode);
        Assert.Equal("a", (left.Symbol as NameToken).Value);

        var right = (op.Right as NameExpressionNode);
        Assert.Equal("b", (right.Symbol as NameToken).Value);
        
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
    
    /// <summary>
    ///     Test for a % b % c
    /// </summary>
    [Fact]
    public void TestTermModuloOperatorMultiple()
    {
        var tokens = new List<Token>()
        {
            new NameToken(0, 1, "a", ImmutableArray<Trivia>.Empty),
            new Token(2, 3, TokenCode.PyModulo, ImmutableArray<Trivia>.Empty),
            new NameToken(4, 5, "b", ImmutableArray<Trivia>.Empty),
            new Token(6, 7, TokenCode.PyModulo, ImmutableArray<Trivia>.Empty),
            new NameToken(8, 9, "c", ImmutableArray<Trivia>.Empty),
            new Token(9, 9, TokenCode.Eof, ImmutableArray<Trivia>.Empty)
        };
        
        var parser = new PythonCoreParser(new MockPythonCoreTokenizer(tokens.ToImmutableArray()));
        var res = parser.ParseEvalInput();
        
        Assert.Equal(0, res.StartPosition);
        Assert.Equal(9, res.EndPosition);
        
        
        var op = ((res as EvalInputStatementNode).Right as ModuloExpressionNode);
        Assert.Equal(0, op.StartPosition);
        Assert.Equal(9, op.EndPosition);
        Assert.Equal(TokenCode.PyModulo, op.Symbol.Code);
        
        var right = (op.Right as NameExpressionNode);
        Assert.Equal("c", (right.Symbol as NameToken).Value);
        
        var op2 = (op.Left as ModuloExpressionNode);
        Assert.Equal(0, op2.StartPosition);
        Assert.Equal(6, op2.EndPosition);
        Assert.Equal(TokenCode.PyModulo, op2.Symbol.Code);

        var left2 = (op2.Left as NameExpressionNode);
        Assert.Equal("a", (left2.Symbol as NameToken).Value);
        
        var right2 = (op2.Right as NameExpressionNode);
        Assert.Equal("b", (right2.Symbol as NameToken).Value);
        
        
        Assert.True((res as EvalInputStatementNode).Newlines.IsEmpty);
        Assert.Equal(TokenCode.Eof, 
            (res as EvalInputStatementNode).Eof.Code);
    }
}

#pragma warning restore CS8602
