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
}

#pragma warning restore CS8602
