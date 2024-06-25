namespace Furesoft.PrattParser.Nodes.Operators;

/// <summary>
/// A binary arithmetic expression like "a + b" or "c ^ d".
/// </summary>
public record BinaryOperatorNode(AstNode LeftExpr, Symbol Operator, AstNode RightExpr) : AstNode
{
}
