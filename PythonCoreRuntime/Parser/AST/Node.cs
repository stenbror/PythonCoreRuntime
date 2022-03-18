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
public record UnaryBitInvertExpressionNode(int StartPosition, int EndPosition, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record MulExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record DivExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record FloorDivExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record ModuloExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record MatriceExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record PlusExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record MinusExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record ShiftLeftExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record ShiftRightExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record BitwiseAndExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record BitwiseXorExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record BitwiseOrExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record StarExpressionNode(int StartPosition, int EndPosition, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record ComparisonLessExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record ComparisonLessEqualExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record ComparisonEqualExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record ComparisonNotEqualExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record ComparisonGreaterEqualExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record ComparisonGreaterExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record ComparisonNotInExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol1, Token Symbol2, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record ComparisonInExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record ComparisonIsExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record ComparisonIsNotExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol1, Token Symbol2, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record NotTestExpressionNode(int StartPosition, int EndPosition, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);

public record StatementNode(int StartPosition, int EndPosition) : Node(StartPosition, EndPosition);
public record EmptyStatementNode() : ExpressionNode(-1, -1);
