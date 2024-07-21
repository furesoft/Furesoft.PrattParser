using Silverfly.Nodes;
using Silverfly.Parselets;
using Silverfly.Sample.Func.Nodes;
using Silverfly.Text;

namespace Silverfly.Sample.Func.Parselets;

public class AnnotationParselet : IPrefixParselet
{
    public AstNode Parse(Parser parser, Token token)
    {
        //@name
        //@name()
        //@name(1)
        var call = parser.ParseExpression();

        if (call is NameNode name)
        {
            call = new CallNode(call, []);
        }

        if (call is not CallNode)
        {
            call.AddMessage(MessageSeverity.Error, "Annottion can only be a name or call");
            return new InvalidNode(token).WithRange(token, parser.LookAhead(0));
        }

        var expr = parser.ParseExpression();
        if (expr is AnnotatedNode a)
        {
            a.Annotations = a.Annotations.Add((CallNode)call);
        }
        else
        {
            expr.AddMessage(MessageSeverity.Error, "Annotation cannot be used for " + expr.GetType());
        }

        return expr;
    }
}
