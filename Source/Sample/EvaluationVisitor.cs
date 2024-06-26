using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Nodes.Operators;
using Sample.Nodes;

namespace Sample;

public class EvaluationVisitor : IVisitor<double>
{
    Scope currentScope = Scope.Root;

    public double Visit(AstNode node)
    {
        if (node is BlockNode block)
        {
            return Visit(block.Children.Last());
        }
        else if (node is LiteralNode literal)
        {
            return Convert.ToDouble(literal.Value);
        }
        else if (node is BinaryOperatorNode binNode)
        {
            var leftVisited = Visit(binNode.LeftExpr);
            var rightVisited = Visit(binNode.RightExpr);

            return binNode.Operator.Name switch
            {
                "+" => leftVisited + rightVisited,
                "-" => leftVisited - rightVisited,
                "*" => leftVisited * rightVisited,
                "/" => leftVisited / rightVisited,
                _ => 0
            };
        }
        else if (node is VariableBindingNode binding)
        {
            currentScope.Define(binding.Name.Text.ToString(), Visit(binding.Value));
        }
        else if (node is NameNode name)
        {
            return (double)currentScope.Get(name.Name);
        }

        return 0;
    }
}
