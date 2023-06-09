﻿namespace Furesoft.PrattParser.Lexing;

public interface ILexerMatcher
{
    bool Match(Lexer lexer, char c);

    Token Build(Lexer lexer, ref int index, ref int column, ref int line);
}
