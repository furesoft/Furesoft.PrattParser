﻿using System;

namespace Furesoft.PrattParser.Matcher;

public class NumberMatcher : ILexerMatcher
{
    private bool _allowHex;
    private bool _allowBin;

    public NumberMatcher(bool allowHex, bool allowBin)
    {
        _allowHex = allowHex;
        _allowBin = allowBin;
    }

    public bool Match(Lexer lexer, char c)
    {
        var isnegative = c == '-' && char.IsDigit(lexer.Peek(1));
        var isDigit = char.IsDigit(lexer.Peek(0));
        var isHexDigit = lexer.IsMatch("0x");
        var isBinaryDigit = lexer.IsMatch("0b");

        return (isHexDigit && _allowHex) || (isBinaryDigit && _allowBin) || isnegative || isDigit;
    }

    public Token Build(Lexer lexer, ref int index, ref int column, ref int line)
    {
        var oldColumn = column;
        var oldIndex = index;

        if (lexer.IsMatch("0x"))
        {
            AdvanceHexNumber(lexer, ref index);
            goto createToken;
        }

        if (lexer.IsMatch("0b"))
        {
            AdvanceBinNumber(lexer, ref index);
            goto createToken;
        }

        AdvanceNumber(lexer, ref index, char.IsDigit);

        LexFloatingPointNumber(lexer, ref index, ref column);

        createToken:
        return new(PredefinedSymbols.Number, lexer.Document.Source.Substring(oldIndex, index - oldIndex),
            line, oldColumn);
    }

    private void AdvanceBinNumber(Lexer lexer, ref int index)
    {
        AdvanceNumber(lexer, ref index, IsValidBinChar, 2);
    }

    private void AdvanceHexNumber(Lexer lexer, ref int index)
    {
        AdvanceNumber(lexer, ref index, IsValidHexChar, 2);
    }
    
    private bool IsValidBinChar(char c)
    {
        return c is '1' or '0';
    }


    private static void LexFloatingPointNumber(Lexer lexer, ref int index, ref int column)
    {
        if (lexer.Peek(0) == '.')
        {
            AdvanceNumber(lexer, ref index, char.IsDigit, 1);

            // Handle E-Notation
            if (lexer.Peek(0) == 'e' || lexer.Peek(0) == 'E')
            {
                AdvanceNumber(lexer, ref index, char.IsDigit, 1);
            }
        }
    }

    private static void AdvanceNumber(Lexer lexer, ref int index, Predicate<char> charPredicate, int preskip = 0)
    {
        for (int i = 0; i < preskip; i++)
        {
            lexer.Advance();
        }
        
        do
        {
            lexer.Advance();
        } while (index < lexer.Document.Source.Length && charPredicate(lexer.Document.Source[index]));
    }

    private bool IsValidHexChar(char c) => char.IsDigit(c) || c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z';
}
