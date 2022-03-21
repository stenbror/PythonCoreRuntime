using System.Collections.Immutable;

namespace PythonCoreRuntime.Parser;

public enum TokenCode
{
    Eof,
    Indent,
    Dedent,
    Newline,
    PyFalse,
    PyNone,
    PyTrue,
    PyAnd,
    PyAs,
    PyAssert,
    PyAsync,
    PyAwait,
    PyBreak,
    PyClass,
    PyContinue,
    PyDef,
    PyDel,
    PyElif,
    PyElse,
    PyExcept,
    PyFinally,
    PyFor,
    PyFrom,
    PyGlobal,
    PyIf,
    PyImport,
    PyIn,
    PyIs,
    PyLambda,
    PyNonlocal,
    PyNot,
    PyOr,
    PyPass,
    PyRaise,
    PyReturn,
    PyTry,
    PyWhile,
    PyWith,
    PyYield,
    PyPlus,
    PyMinus,
    PyMul,
    PyPower,
    PyDiv,
    PyFloorDiv,
    PyModulo,
    PyMatrice,
    PyShiftLeft,
    PyShiftRight,
    PyBitAnd,
    PyBitOr,
    PyBitXor,
    PyBitInvert,
    PyColonAssign,
    PyColon,
    PyLess,
    PyGreater,
    PyLessEqual,
    PyGreaterEqual,
    PyEqual,
    PyNotEqual,
    PyLeftParen,
    PyLeftBracket,
    PyLeftCurly,
    PyRightParen,
    PyRightBracket,
    PyRightCurly,
    PyComma,
    PyDot,
    PyElipsis,
    PySemiColon,
    PyAssign,
    PyArrow,
    PyPlusAssign,
    PyMinusAssign,
    PyMulAssign,
    PyDivAssign,
    PyFloorDivAssign,
    PyModuloAssign,
    PyMatriceAssign,
    PyBitAndAssign,
    PyBitOrAssign,
    PyBitXorAssign,
    PyShiftLeftAssign,
    PyShiftRightAssign,
    PyPowerAssign,
    Name,
    Number,
    String,
    TypeComment
};

public record Token(int StartPosition, int EndPosition, TokenCode Code, ImmutableArray<Trivia> Trivia);

public record EofToken() : Token(-1, -1, TokenCode.Eof, ImmutableArray<Trivia>.Empty);

public record NameToken(int StartPosition, int EndPosition, string Value, ImmutableArray<Trivia> Trivia) 
    : Token(StartPosition, EndPosition, TokenCode.Name, Trivia);

public record NumberToken(int StartPosition, int EndPosition, string Value, ImmutableArray<Trivia> Trivia) 
    : Token(StartPosition, EndPosition, TokenCode.Number, Trivia);

public record StringToken(int StartPosition, int EndPosition, string Value, ImmutableArray<Trivia> Trivia) 
    : Token(StartPosition, EndPosition, TokenCode.String, Trivia);


