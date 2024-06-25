namespace Furesoft.PrattParser.Nodes.Operators;

/// <summary>
/// A postfix unary arithmetic expression like "a!"
/// </summary>
public record PostfixOperatorNode(AstNode Expr, Symbol Operator) : AstNode
{
}
