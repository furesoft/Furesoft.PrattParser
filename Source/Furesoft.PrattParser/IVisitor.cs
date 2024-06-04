﻿using System.Collections.Generic;
using Furesoft.PrattParser.Nodes;

namespace Furesoft.PrattParser;

public interface IVisitor<out TReturn>
{
    TReturn Visit(AstNode node);
    public IEnumerable<TReturn> Visit(BlockNode block) {
        var result = new List<TReturn>();

        foreach (var child in block.Children)
        {
            result.Add(Visit(child));
        }

        return result;
    }
}

public interface IVisitor
{
    void Visit(AstNode node);
    void Visit(BlockNode block)
    {
        foreach (var child in block.Children)
        {
            Visit(child);
        }
    }
}
