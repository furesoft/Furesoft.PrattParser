﻿namespace Furesoft.PrattParser.Lexing.Matcher;

public class StringMatcher : ILexerMatcher
{
    private readonly Symbol _leftStr;
    private readonly Symbol _rightStr;

    public StringMatcher(Symbol leftStr, Symbol rightStr)
    {
        _leftStr = leftStr;
        _rightStr = rightStr;
    }

    public bool Match(Lexer lexer, char c)
    {
        return _leftStr != null && _rightStr != null &&
                             lexer.IsMatch(_leftStr.Name);
    }

    public Token Build(Lexer lexer, ref int index, ref int column, ref int line)
    {
        var oldColumn = column;
        var oldIndex = index;
        
        index++;
        column++;

        while (!lexer.IsMatch(_rightStr.Name))
        {
            lexer.Advance();
        }

        var text = lexer.Document.Source.Slice(oldIndex +1, index - oldIndex - 1);
        
        index++;
        column++;

        
        return new(PredefinedSymbols.String, text, line, oldColumn);
    }
}
