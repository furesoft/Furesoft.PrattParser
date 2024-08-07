﻿using System;
using System.Globalization;
using Silverfly.Nodes;

namespace Silverfly.Parselets.Literals;

public class NumberParselet : IPrefixParselet
{
    public AstNode Parse(Parser parser, Token token)
    {
        var text = token.Text.ToString();

        if (text.StartsWith("0x"))
        {
            return new LiteralNode(ulong.Parse(token.Text[2..].Span, NumberStyles.HexNumber), token).WithRange(token);
        }

        if (text.StartsWith("0b"))
        {
            return new LiteralNode(Convert.ToUInt32(token.Text[2..].ToString(), 2), token).WithRange(token);
        }

        if (!text.StartsWith('-') && !text.Contains('.'))
        {
            return new LiteralNode(ulong.Parse(token.Text.Span), token).WithRange(token);
        }

        if (text.Contains('.'))
        {
            return new LiteralNode(double.Parse(text, CultureInfo.InvariantCulture), token).WithRange(token);
        }

        return new LiteralNode(long.Parse(token.Text.Span), token).WithRange(token);
    }
}
