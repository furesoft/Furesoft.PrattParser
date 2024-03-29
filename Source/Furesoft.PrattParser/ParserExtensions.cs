﻿using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Parselets;
using Furesoft.PrattParser.Parselets.Literals;

namespace Furesoft.PrattParser;

public static class ParserExtensions
{
    public static void AddArithmeticOperators(this Parser parser)
    {
        parser.Prefix("+", (int)BindingPower.Prefix);
        parser.Prefix("-", (int)BindingPower.Prefix);
        
        parser.Group("(", ")");
        
        parser.InfixLeft("+", (int)BindingPower.Sum);
        parser.InfixLeft("-", (int)BindingPower.Sum);
        parser.InfixLeft("*", (int)BindingPower.Product);
        parser.InfixLeft("/", (int)BindingPower.Product);
    }

    /// <summary>
    /// Return true if a given symbol was found and consumed
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="optionalSymbol"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool Optional(this Parser parser, Symbol optionalSymbol)
    {
        if (!parser.IsMatch(optionalSymbol))
        {
            return false;
        }

        parser.Consume();
            
        return true;

    }
    
    public static void AddLogicalOperators(this Parser parser)
    {
        parser.Prefix("!", (int)BindingPower.Prefix);
        
        parser.InfixLeft("&&", (int)BindingPower.Product);
        parser.InfixLeft("||", (int)BindingPower.Sum);
    }
    
    public static void AddBitOperators(this Parser parser)
    {
        parser.Prefix("~", (int)BindingPower.Prefix);
        
        parser.InfixLeft("&", (int)BindingPower.Product);
        parser.InfixLeft("|", (int)BindingPower.Sum);
        
        parser.InfixLeft("<<", (int)BindingPower.Product);
        parser.InfixLeft(">>", (int)BindingPower.Product);
    }

    public static void AddCommonLiterals(this Parser parser)
    {
        parser.Register(PredefinedSymbols.Number, new NumberParselet());
        parser.Register(PredefinedSymbols.Boolean, new BooleanLiteralParselet());
        parser.Register(PredefinedSymbols.String, new StringLiteralParselet());
    }

    public static void AddCommonAssignmentOperators(this Parser parser)
    {
        parser.InfixLeft("=", BindingPower.Assignment);
        parser.InfixLeft("+=", BindingPower.Assignment);
        parser.InfixLeft("-=", BindingPower.Assignment);
        parser.InfixLeft("*=", BindingPower.Assignment);
        parser.InfixLeft("/=", BindingPower.Assignment);
        
        parser.Prefix("++", (int)BindingPower.Prefix);
        parser.Prefix("--", (int)BindingPower.Prefix);
        
        parser.Postfix("--", (int)BindingPower.PostFix);
        parser.Postfix("++", (int)BindingPower.PostFix);
    }
}
