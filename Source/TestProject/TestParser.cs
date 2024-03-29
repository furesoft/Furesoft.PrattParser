using Furesoft.PrattParser;
using Furesoft.PrattParser.Lexing.IgnoreMatcher.Comments;
using Furesoft.PrattParser.Parselets;

namespace TestProject;

public class TestParser : Parser
{
    public TestParser()
    {
        Register(PredefinedSymbols.Name, new NameParselet());

        Register("(", new CallParselet());

        Ternary("?", ":", (int)BindingPower.Conditional);

        this.AddArithmeticOperators();
        this.AddBitOperators();
        this.AddLogicalOperators();
        this.AddCommonLiterals();
        this.AddCommonAssignmentOperators();

        Prefix("not", (int)BindingPower.Prefix);

        Postfix("!", (int)BindingPower.PostFix);

        InfixRight("^", (int)BindingPower.Exponent);

        InfixLeft("->", (int)BindingPower.Product);

        Block(PredefinedSymbols.Semicolon, PredefinedSymbols.EOF);
    }

    protected override void InitLexer(Lexer lexer)
    {
        lexer.Ignore(' ', '\r', '\t');
        lexer.Ignore("\r\n");

        lexer.MatchBoolean();
        lexer.MatchString("'", "'");
        lexer.MatchNumber(true, true);

        lexer.Ignore(new SingleLineCommentIgnoreMatcher(PredefinedSymbols.SlashSlash));
        lexer.Ignore(
            new MultiLineCommentIgnoreMatcher(PredefinedSymbols.SlashAsterisk, PredefinedSymbols.AsteriskSlash));
    }
}
