using Sample.FuncLanguage.Nodes;
using Silverfly;
using Silverfly.Nodes;
using Silverfly.Parselets;

namespace Sample.FuncLanguage.Parselets;

public class ImportParselet : IPrefixParselet
{
    public AstNode Parse(Parser parser, Token token)
    {
        var arg = parser.ParseExpression();

        if (arg is LiteralNode { Value: string path })
        {
            return new ImportNode(path);
        }
        else if (arg is NameNode name)
        {
            return new ImportNode(name.Name);
        }

        return new InvalidNode(token);
    }
}