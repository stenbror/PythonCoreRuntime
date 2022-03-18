using System.Collections.Immutable;

namespace PythonCoreRuntime.Parser.AST;

public record Node(int StartPosition, int EndPosition);
public record ExpressionNode(int StartPosition, int EndPosition) : Node(StartPosition, EndPosition);
public record EmptyExpressionNode() : ExpressionNode(-1, -1);
public record NoneExpressionNode(int StartPosition, int EndPosition, Token Symbol) : ExpressionNode(StartPosition, EndPosition);
public record FalseExpressionNode(int StartPosition, int EndPosition, Token Symbol) : ExpressionNode(StartPosition, EndPosition);
public record TrueExpressionNode(int StartPosition, int EndPosition, Token Symbol) : ExpressionNode(StartPosition, EndPosition);
public record ElipsisExpressionNode(int StartPosition, int EndPosition, Token Symbol) : ExpressionNode(StartPosition, EndPosition);
public record NameExpressionNode(int StartPosition, int EndPosition, Token Symbol) : ExpressionNode(StartPosition, EndPosition);
public record NumberExpressionNode(int StartPosition, int EndPosition, Token Symbol) : ExpressionNode(StartPosition, EndPosition);
public record StringExpressionNode(int StartPosition, int EndPosition, ImmutableArray<Token> Symbols) : ExpressionNode(StartPosition, EndPosition);

public record AtomExpressionNode(int StartPosition, int EndPosition, Token? Await, ExpressionNode Left, ImmutableArray<ExpressionNode> Trailers) 
    : ExpressionNode(StartPosition, EndPosition);
public record PowerExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record UnaryPlusExpressionNode(int StartPosition, int EndPosition, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record UnaryMinusExpressionNode(int StartPosition, int EndPosition, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record UnaryBitInverExpressionNode(int StartPosition, int EndPosition, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);




public record StatementNode(int StartPosition, int EndPosition) : Node(StartPosition, EndPosition);
public record EmptyStatementNode() : ExpressionNode(-1, -1);
