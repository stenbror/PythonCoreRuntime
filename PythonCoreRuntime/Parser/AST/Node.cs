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
public record AndTestExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record OrTestExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record LambdaExpressionNode(int StartPosition, int EndPosition, bool Conditional, Token Symbol1,
        ExpressionNode? Left, Token Symbol2, ExpressionNode Right)
    : ExpressionNode(StartPosition, EndPosition);
public record TestExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol1, ExpressionNode Right, Token Symbol2, ExpressionNode Next)
    : ExpressionNode(StartPosition, EndPosition);
public record NamedExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);

public record TestListExpressionNode(int StartPosition, int EndPosition, ImmutableArray<ExpressionNode> Nodes, ImmutableArray<Token> Separatrors) 
    : ExpressionNode(StartPosition, EndPosition);

public record TupleExpressionNode(int StartPosition, int EndPosition, Token Symbol1, ExpressionNode? Right, Token Symbol2) 
    : ExpressionNode(StartPosition, EndPosition);
public record ListExpressionNode(int StartPosition, int EndPosition, Token Symbol1, ExpressionNode? Right, Token Symbol2) 
    : ExpressionNode(StartPosition, EndPosition);
public record DictionaryExpressionNode(int StartPosition, int EndPosition, Token Symbol1, ExpressionNode? Right, Token Symbol2) 
    : ExpressionNode(StartPosition, EndPosition);
public record SetExpressionNode(int StartPosition, int EndPosition, Token Symbol1, ExpressionNode? Right, Token Symbol2) 
    : ExpressionNode(StartPosition, EndPosition);

public record DotNameExpressionNode(int StartPosition, int EndPosition, Token Symbol1, Token Symbol2) 
    : ExpressionNode(StartPosition, EndPosition);
public record CallExpressionNode(int StartPosition, int EndPosition, Token Symbol1, ExpressionNode? Right, Token Symbol2) 
    : ExpressionNode(StartPosition, EndPosition);
public record IndexExpressionNode(int StartPosition, int EndPosition, Token Symbol1, ExpressionNode? Right, Token Symbol2) 
    : ExpressionNode(StartPosition, EndPosition);
public record SubscriptListExpressionNode(int StartPosition, int EndPosition, ImmutableArray<ExpressionNode> Nodes, ImmutableArray<Token> Separatrors) 
    : ExpressionNode(StartPosition, EndPosition);
public record SubscriptExpressionNode(int StartPosition, int EndPosition, 
        ExpressionNode? Left, Token? Symbol1, ExpressionNode? Right, Token? Symbol2, ExpressionNode? Next) 
    : ExpressionNode(StartPosition, EndPosition);
public record ExprListExpressionNode(int StartPosition, int EndPosition, ImmutableArray<ExpressionNode> Nodes, ImmutableArray<Token> Separatrors) 
    : ExpressionNode(StartPosition, EndPosition);
public record ArgListExpressionNode(int StartPosition, int EndPosition, ImmutableArray<ExpressionNode> Nodes, ImmutableArray<Token> Separatrors) 
    : ExpressionNode(StartPosition, EndPosition);

public record ArgumentExpressionNode(int StartPosition, int EndPosition, Token? Left, Token? Symbol, ExpressionNode? Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record CompSyncForExpressionNode(int StartPosition, int EndPosition, Token Symbol1, ExpressionNode Left, 
    Token Symbol2, ExpressionNode Right, ExpressionNode? Next) : ExpressionNode(StartPosition, EndPosition);
public record CompForExpressionNode(int StartPosition, int EndPosition, Token Symbol, ExpressionNode Right)
    : ExpressionNode(StartPosition, EndPosition);
public record CompIfExpressionNode(int StartPosition, int EndPosition, Token Symbol, ExpressionNode Right, ExpressionNode? Next)
    : ExpressionNode(StartPosition, EndPosition);

public record YieldExpressionNode(int StartPosition, int EndPosition, Token Symbol, Node Right)
    : ExpressionNode(StartPosition, EndPosition);

public record YieldFromExpressionNode(int StartPosition, int EndPosition, Token Symbol1, Token Symbol2, ExpressionNode Right)
    : ExpressionNode(StartPosition, EndPosition);

public record PowerKeyExpressionNode(int StartPosition, int EndPosition, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record KeyValueExpressionNode(int StartPosition, int EndPosition, ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);
public record DictionaryContainerExpressionNode(int StartPosition, int EndPosition, 
        ImmutableArray<ExpressionNode> Nodes, ImmutableArray<Token> Separators) 
    : ExpressionNode(StartPosition, EndPosition);    
public record SetContainerExpressionNode(int StartPosition, int EndPosition, 
        ImmutableArray<ExpressionNode> Nodes, ImmutableArray<Token> Separators) 
    : ExpressionNode(StartPosition, EndPosition);

public record VarArgsListExpressionNode(int StartPosition, int EndPosition,
        Token? MulOperator, ExpressionNode? MulNode, Token? PowerOperator, ExpressionNode? PowerNode,
        ImmutableArray<ExpressionNode> Nodes, ImmutableArray<Token> Separators) 
    : ExpressionNode(StartPosition, EndPosition);

public record VarArgsAssignExpressionNode(int StartPosition, int EndPosition,
        ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : ExpressionNode(StartPosition, EndPosition);


public record StatementNode(int StartPosition, int EndPosition) : Node(StartPosition, EndPosition);
public record EmptyStatementNode() : ExpressionNode(-1, -1);

public record EvalInputStatementNode(int StartPosition, int EndPosition,
        ExpressionNode Right, ImmutableArray<Token> Newlines, Token Eof) 
    : StatementNode(StartPosition, EndPosition);

public record FileInputStatementNode(int StartPosition, int EndPosition, ImmutableArray<StatementNode> Nodes,
        ImmutableArray<Token> Newlines, Token Eof) 
    : StatementNode(StartPosition, EndPosition);

public record StatementListNode(int StartPosition, int EndPosition, ImmutableArray<StatementNode> Nodes,
        ImmutableArray<Token> Separators, Token Newline) 
    : StatementNode(StartPosition, EndPosition);

public record ExpressionStatementNode(int StartPosition, int EndPosition, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);

public record PlusAssignStatementNode(int StartPosition, int EndPosition,
        ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record MinusAssignStatementNode(int StartPosition, int EndPosition,
        ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record MulAssignStatementNode(int StartPosition, int EndPosition,
        ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record PowerAssignStatementNode(int StartPosition, int EndPosition,
        ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record DivAssignStatementNode(int StartPosition, int EndPosition,
        ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record FloorDivAssignStatementNode(int StartPosition, int EndPosition,
        ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record ModuloAssignStatementNode(int StartPosition, int EndPosition,
        ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record MatriceAssignStatementNode(int StartPosition, int EndPosition,
        ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record BitAndAssignStatementNode(int StartPosition, int EndPosition,
        ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record BitOrAssignStatementNode(int StartPosition, int EndPosition,
        ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record BitXorAssignStatementNode(int StartPosition, int EndPosition,
        ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record ShiftLeftAssignStatementNode(int StartPosition, int EndPosition,
        ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record ShiftRightAssignStatementNode(int StartPosition, int EndPosition,
        ExpressionNode Left, Token Symbol, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record AnnAssignStatementNode(int StartPosition, int EndPosition,
        ExpressionNode Left, Token Symbol1, ExpressionNode Right, Token? Symbol2, ExpressionNode? Next) 
    : StatementNode(StartPosition, EndPosition);

public record AssignmentStatementNode(int StartPosition, int EndPosition,
        Node Left, Token Symbol, ExpressionNode Right)
    : StatementNode(StartPosition, EndPosition)
{
    public Token? TypeComment { get; set; }
    
}
public record DelStatementNode(int StartPosition, int EndPosition, Token Symbol, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record PassStatementNode(int StartPosition, int EndPosition, Token Symbol) 
    : StatementNode(StartPosition, EndPosition);
public record BreakStatementNode(int StartPosition, int EndPosition, Token Symbol) 
    : StatementNode(StartPosition, EndPosition);
public record ContinueStatementNode(int StartPosition, int EndPosition, Token Symbol) 
    : StatementNode(StartPosition, EndPosition);
public record YieldStatementNode(int StartPosition, int EndPosition, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record RaiseStatementNode(int StartPosition, int EndPosition, Token Symbol1, ExpressionNode? Left, 
        Token? Symbol2, ExpressionNode? Right) 
    : StatementNode(StartPosition, EndPosition);
public record ReturnStatementNode(int StartPosition, int EndPosition, Token Symbol, ExpressionNode? Right) 
    : StatementNode(StartPosition, EndPosition);
public record GlobalStatementNode(int StartPosition, int EndPosition, 
        ImmutableArray<Token> Names, ImmutableArray<Token> Separators) 
    : StatementNode(StartPosition, EndPosition);
public record NonlocalStatementNode(int StartPosition, int EndPosition, 
        ImmutableArray<Token> Names, ImmutableArray<Token> Separators) 
    : StatementNode(StartPosition, EndPosition);
public record AssertStatementNode(int StartPosition, int EndPosition, Token Symbol1, ExpressionNode Left, 
        Token? Symbol2, ExpressionNode? Right) 
    : StatementNode(StartPosition, EndPosition);
public record DottedNameStatementNode(int StartPosition, int EndPosition, 
        ImmutableArray<Token> Nodes, ImmutableArray<Token> Separators) 
    : StatementNode(StartPosition, EndPosition);
public record DottedAsNamesStatementNode(int StartPosition, int EndPosition,
        ImmutableArray<StatementNode> Nodes, ImmutableArray<Token> Separators) 
    : StatementNode(StartPosition, EndPosition);
public record ImportAsNamesStatementNode(int StartPosition, int EndPosition,
        ImmutableArray<StatementNode> Nodes, ImmutableArray<Token> Separators) 
    : StatementNode(StartPosition, EndPosition);
public record DottedAsNameStatementNode(int StartPosition, int EndPosition, 
        StatementNode Left, Token Symbol, Token Right) 
    : StatementNode(StartPosition, EndPosition);
public record ImportAsNameStatementNode(int StartPosition, int EndPosition,
        Token Left, Token Symbol, Token Right) 
    : StatementNode(StartPosition, EndPosition);
public record NameLiteralStatementNode(int StartPosition, int EndPosition, Token Symbol) 
    : StatementNode(StartPosition, EndPosition);
public record ImportNameStatementNode(int StartPosition, int EndPosition,
        Token Symbol, StatementNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record ImportFromStatementNode(int StartPosition, int EndPosition, Token Symbol1, StatementNode? Left,
        ImmutableArray<Token> Dots, Token Symbol2, Token? Symbol3, StatementNode? Right, Token? Symbol4) 
    : StatementNode(StartPosition, EndPosition);
public record IfStatementNode(int StartPosition, int EndPosition, Token Symbol, ExpressionNode Left, Token Symbol2, 
        StatementNode Right, ImmutableArray<StatementNode> ElifNodes, StatementNode? ElseNode) 
    : StatementNode(StartPosition, EndPosition);
public record ElifStatementNode(int StartPosition, int EndPosition, Token Symbol1, ExpressionNode Left, 
        Token Symbol2, StatementNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record ElseStatementNode(int StartPosition, int EndPosition, Token Symbol1, Token Symbol2, StatementNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record WhileStatementNode(int StartPosition, int EndPosition, Token Symbol1, ExpressionNode Left, 
        Token Symbol2, StatementNode Right, StatementNode? Next) 
    : StatementNode(StartPosition, EndPosition);
public record ForStatementNode(int StartPosition, int EndPosition, Token Symbol1, ExpressionNode Left, Token Symbol2, 
        ExpressionNode Right, Token? TypeComment, Token Symbol3, StatementNode Next, StatementNode? ElseNode) 
    : StatementNode(StartPosition, EndPosition);
public record TryStatementNode(int StartPosition, int EndPosition, Token Symbol1, Token Symbol2, StatementNode Left,
        ImmutableArray<StatementNode> Nodes, StatementNode? ElseNode, StatementNode? FinNode) 
    : StatementNode(StartPosition, EndPosition);
public record FinallyStatementNode(int StartPosition, int EndPosition, Token Symbol1, Token Symbol2, StatementNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record ExceptStatementNode(int StartPosition, int EndPosition, 
        StatementNode Left, Token Symbol1, StatementNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record ExceptClauseStatementNode(int StartPosition, int EndPosition, Token Symbol1, ExpressionNode? Left, 
        Token? Symbol2, Token? Name) 
    : StatementNode(StartPosition, EndPosition);
public record WithStatementNode(int StartPosition, int EndPosition, Token Symbol1, 
    ImmutableArray<StatementNode> WithElements, ImmutableArray<Token> Separators, Token Symbol2, 
    Token? TypeComment, StatementNode Right) :
    StatementNode(StartPosition, EndPosition);
public record WithItemStatementNode(int StartPosition, int EndPosition, ExpressionNode Left, 
        Token? Symbol1, ExpressionNode? Right) 
    : StatementNode(StartPosition, EndPosition);
public record SuiteStatementNode(int StartPosition, int EndPosition, Token Symbol1, Token Symbol2, 
        ImmutableArray<StatementNode> Nodes, Token Symbol3) 
    : StatementNode(StartPosition, EndPosition);
public record SingleInputStatementNode(int StartPosition, int EndPosition, Token? Newline, StatementNode? Right) 
    : StatementNode(StartPosition, EndPosition);
public record FuncTypeInputStatementNode(int StartPosition, int EndPosition, 
        StatementNode Right, ImmutableArray<Token> Newlines) 
    : StatementNode(StartPosition, EndPosition);
public record DecoratorsStatementNode(int StartPosition, int EndPosition, ImmutableArray<StatementNode> Nodes) 
    : StatementNode(StartPosition, EndPosition);
public record DecoratorStatementNode(int StartPosition, int EndPosition,
        Token Symbol1, StatementNode Left, Token? Symbol2, ExpressionNode? Right, Token? Symbol3, Token Symbol4) 
    : StatementNode(StartPosition, EndPosition);
public record DecoratedStatementNode(int StartPosition, int EndPosition,
        StatementNode Decorators, StatementNode? Right) 
    : StatementNode(StartPosition, EndPosition);
public record AsyncStatementNode(int StartPosition, int EndPosition, Token Symbol, StatementNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record ClassStatementNode(int StartPosition, int EndPosition, Token Symbol1, Token Name, Token? Symbol3,
        ExpressionNode? Left, Token? Symbol4, Token Symbol5, StatementNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record AsyncFuncDefStatementNode(int StartPosition, int EndPosition, Token Symbol1, StatementNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record FuncDefStatementNode(int StartPosition, int EndPosition, Token Symbol1, Token Name, StatementNode Para, Token? Symbol3, 
        ExpressionNode? Left, Token Symbol4, Token? TypeComment, StatementNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record ParameterNode(int StartPosition, int EndPosition, Token Symbol1, StatementNode? Right, Token Symbol2) 
    : StatementNode(StartPosition, EndPosition);
public record TFPDefNode(int StartPosition, int EndPosition, StatementNode Left, Token Symbol1, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record TypedArgsAssignNode(int StartPosition, int EndPosition, StatementNode Left, 
        Token Symbol, ExpressionNode Right) 
    : StatementNode(StartPosition, EndPosition);
public record TypedArgsListNode(int StartPosition, int EndPosition, Token? mulOp, StatementNode? mulNode, Token? tcMul,
        Token? powerOp, StatementNode? powerNode, Token? tcPower, Token? slashOp, ImmutableArray<StatementNode> Nodes,
        ImmutableArray<Token> Separators, ImmutableArray<Token> TypeComments) 
    : StatementNode(StartPosition, EndPosition);