namespace Furesoft.PrattParser.Nodes.Operators;

public record TernaryOperatorNode(AstNode FirstExpr, AstNode SecondExpr, AstNode ThirdExpr) : AstNode
{
}
