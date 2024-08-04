﻿using System;
using System.Collections.Generic;
using System.Linq;
using Silverfly.Lexing;
using Silverfly.Lexing.IgnoreMatcher;
using Silverfly.Lexing.Matcher;
using Silverfly.Lexing.NameAdvancers;

namespace Silverfly;

public class LexerConfig
{
    internal INameAdvancer NameAdvancer = new DefaultNameAdvancer();
    internal Dictionary<string, Symbol> Punctuators = [];
    internal readonly List<IMatcher> Parts = [];
    internal readonly List<IIgnoreMatcher> IgnoreMatchers = [];

    public LexerConfig()
    {
        // Register all of the Symbols that are explicit punctuators.
        foreach (var type in PredefinedSymbols.Pool)
        {
            var punctuator = type.Punctuator();

            if (punctuator != "\0")
            {
                Punctuators.Add(punctuator, type);
            }
        }
    }

    // sort punctuators longest -> smallest to make it possible to use symbols with more than one character
    internal void OrderSymbols()
    {
        Punctuators = new(Punctuators.OrderByDescending(_ => _.Key.Length));
    }

    public void AddSymbol(string symbol)
    {
        if (Punctuators.ContainsKey(symbol))
        {
            return;
        }

        Punctuators.Add(symbol, PredefinedSymbols.Pool.Get(symbol));
    }

    public void AddMatcher(IMatcher matcher)
    {
        Parts.Add(matcher);
    }

    public void UseNameAdvancer(INameAdvancer advancer)
    {
        NameAdvancer = advancer;
    }

    public void Ignore(IIgnoreMatcher matcher)
    {
        IgnoreMatchers.Add(matcher);
    }

    /// <summary>
    /// Adds a matcher to identify strings enclosed between <paramref name="leftSymbol"/> and <paramref name="rightSymbol"/>.
    /// </summary>
    /// <param name="leftSymbol">The symbol marking the start of the string.</param>
    /// <param name="rightSymbol">The symbol marking the end of the string.</param>
    /// <param name="allowEscapeChars">Flag indicating whether escape characters are allowed within the string.</param>
    /// <param name="allowUnicodeChars">Flag indicating whether Unicode escape sequences are allowed within the string.</param>
    public void MatchString(Symbol leftSymbol, Symbol rightSymbol, bool allowEscapeChars = true, bool allowUnicodeChars = true)
    {
        AddMatcher(new StringMatcher(leftSymbol, rightSymbol, allowEscapeChars, allowUnicodeChars));
    }

    /// <summary>
    /// Adds a matcher to identify numbers, with options to recognize hexadecimal, binary, and floating-point formats.
    /// </summary>
    /// <param name="allowHex">Flag indicating whether hexadecimal numbers are allowed.</param>
    /// <param name="allowBin">Flag indicating whether binary numbers are allowed.</param>
    /// <param name="floatingPointSymbol">Optional symbol indicating the floating-point separator.</param>
    /// <param name="separatorSymbol">Optional symbol indicating the number separator.</param>
    public void MatchNumber(bool allowHex, bool allowBin, Symbol floatingPointSymbol = null, Symbol separatorSymbol = null)
    {
        AddMatcher(new NumberMatcher(allowHex, allowBin, floatingPointSymbol ?? PredefinedSymbols.Dot,
            separatorSymbol ?? PredefinedSymbols.Underscore));
    }

    /// <summary>
    /// Adds symbols to the lexer for tokenization.
    /// </summary>
    /// <param name="symbols">The symbols to add.</param>
    public void AddSymbols(params string[] symbols)
    {
        foreach (var symbol in symbols)
        {
            AddSymbol(symbol);
        }
    }

    /// <summary>
    /// Adds a matcher to identify boolean values ('true' and 'false').
    /// </summary>
    /// <param name="ignoreCasing">Flag indicating whether casing should be ignored when matching.</param>
    public void MatchBoolean(bool ignoreCasing = false)
    {
        AddMatcher(new BooleanMatcher(ignoreCasing));
    }

    /// <summary>
    /// Adds a matcher to ignore a specific character.
    /// </summary>
    /// <param name="c">The character to ignore.</param>
    public void Ignore(char c)
    {
        Ignore(new PunctuatorIgnoreMatcher(c.ToString()));
    }

    /// <summary>
    /// Adds a matcher to ignore characters based on a custom predicate.
    /// </summary>
    /// <param name="predicate">The predicate function determining which characters to ignore.</param>
    public void Ignore(Predicate<char> predicate)
    {
        Ignore(new PredicateIgnoreMatcher(predicate));
    }

    /// <summary>
    /// Adds a matcher to ignore whitespace characters.
    /// </summary>
    public void IgnoreWhitespace()
    {
        Ignore(char.IsWhiteSpace);
    }

    /// <summary>
    /// Adds matchers to ignore multiple characters.
    /// </summary>
    /// <param name="chars">The characters to ignore.</param>
    public void Ignore(params char[] chars)
    {
        foreach (var c in chars)
        {
            Ignore(c);
        }
    }

    /// <summary>
    /// Adds a matcher to ignore a specific string.
    /// </summary>
    /// <param name="c">The string to ignore.</param>
    public void Ignore(string c)
    {
        Ignore(new PunctuatorIgnoreMatcher(c));
    }

    /// <summary>
    /// Adds matchers to ignore multiple strings.
    /// </summary>
    /// <param name="symbols">The strings to ignore.</param>
    public void Ignore(params string[] symbols)
    {
        foreach (var symbol in symbols)
        {
            Ignore(symbol);
        }
    }
}
