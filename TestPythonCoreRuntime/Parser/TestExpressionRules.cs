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
}

#pragma warning restore CS8602
