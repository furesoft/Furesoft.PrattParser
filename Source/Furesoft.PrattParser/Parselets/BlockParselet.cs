﻿using Furesoft.PrattParser.Nodes;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Furesoft.PrattParser.Parselets;
public class BlockParselet(Symbol terminator, Symbol seperator = null, bool wrapExpressions = false, Symbol tag = null) : IStatementParselet
{
    public Symbol Terminator { get; } = terminator;
    public Symbol Seperator { get; } = seperator;
    public Symbol Tag { get; } = tag;

    public AstNode Parse(Parser parser, Token token)
    {
        var block = new BlockNode(Seperator, Terminator);
        var children = new List<AstNode>();

        while (!parser.Match(Terminator))
        {
            var node = parser.ParseStatement(wrapExpressions);
            children.Add(node with { Parent = block });

            if (Seperator != null && parser.IsMatch(Seperator))
            {
                parser.Consume(Seperator);
            }
        }

        return block
            .WithChildren(children.ToImmutableList())
            .WithRange(token, parser.LookAhead(0));
    }
}
