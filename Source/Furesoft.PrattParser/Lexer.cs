using System;
using System.Collections.Generic;
using System.Linq;
using Furesoft.PrattParser.Lexing;
using Furesoft.PrattParser.Lexing.IgnoreMatcher;
using Furesoft.PrattParser.Lexing.Matcher;
using Furesoft.PrattParser.Text;

namespace Furesoft.PrattParser;

//ToDo: Add Ability to use a word combination as single keyword e.g. "12 unless if true"
public sealed class Lexer
{
    private Dictionary<string, Symbol> _punctuators = [];
    private readonly List<IMatcher> _parts = [];
    private readonly List<IIgnoreMatcher> _ignoreMatcher = [];
    private int _index;
    private int _line = 1, _column = 1;

    public SourceDocument Document { get; }

    /// <summary>
    /// Creates a new <see cref="Lexer"/> to tokenize the given string.
    /// </summary>
    /// <param name="source">String to tokenize</param>
    public Lexer(string source, string filename = "tmp.synthetic")
    {
        _index = -1;
        Document = new() { Filename = filename, Source = source.AsMemory() };

        // Register all of the Symbols that are explicit punctuators.
        foreach (var type in PredefinedSymbols.Pool)
        {
            var punctuator = type.Punctuator();

            if (punctuator != "\0")
            {
                _punctuators.Add(punctuator, type);
            }
        }

        // sort punctuators longest -> smallest to make it possible to use symbols with more than one character
        OrderSymbols();
    }

    private void OrderSymbols()
    {
        _punctuators = new(_punctuators.OrderByDescending(_ => _.Key.Length));
    }

    public void AddSymbol(string symbol)
    {
        if (_punctuators.ContainsKey(symbol))
        {
            return;
        }

        _punctuators.Add(symbol, PredefinedSymbols.Pool.Get(symbol));

        OrderSymbols();
    }

    public void AddMatcher(IMatcher matcher)
    {
        _parts.Add(matcher);
    }

    public void MatchString(Symbol leftSymbol, Symbol rightSymbol)
    {
        AddMatcher(new StringMatcher(leftSymbol, rightSymbol));
    }

    public void MatchNumber(bool allowHex, bool allowBin, Symbol floatingPointSymbol = null,
        Symbol seperatorSymbol = null)
    {
        AddMatcher(new NumberMatcher(allowHex, allowBin, floatingPointSymbol ?? PredefinedSymbols.Dot,
            seperatorSymbol ?? PredefinedSymbols.Underscore));
    }

    public void MatchBoolean(bool ignoreCasing = false)
    {
        AddMatcher(new BooleanMatcher(ignoreCasing));
    }

    public void Ignore(char c)
    {
        Ignore(new PunctuatorIgnoreMatcher(c.ToString()));
    }

    public void Ignore(params char[] chars)
    {
        foreach (var c in chars)
        {
            Ignore(c);
        }
    }

    public void Ignore(string c)
    {
        Ignore(new PunctuatorIgnoreMatcher(c));
    }

    public void Ignore(IIgnoreMatcher matcher)
    {
        _ignoreMatcher.Add(matcher);
    }

    public char Peek(int distance)
    {
        if (_index + distance >= Document.Source.Length)
        {
            return '\0';
        }

        return Document.Source.Slice(_index + distance, 1).Span[0];
    }

    public bool IsBetween(char first, char second) => Peek(0) >= first && Peek(0) <= second;

    public bool IsMatch(Symbol token, bool ignoreCase = false)
    {
        if (string.IsNullOrEmpty(token.Name))
        {
            return false;
        }

        if (_index + token.Name.Length > Document.Source.Length)
        {
            return false;
        }

        var nameSpan = token.Name.AsMemory().Span;
        var documentSliceSpan = Document.Source.Slice(_index, token.Name.Length).Span;

        var comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        return nameSpan.CompareTo(documentSliceSpan, comparisonType) == 0;
    }

    public Token Next()
    {
        if (_index == -1)
        {
            _index++;

            return new Token(PredefinedSymbols.SOF, _line, _column).WithDocument(Document);
        }

        while (_index < Document.Source.Length)
        {
            var c = Peek(0);

            RecognizeLine(c);

            if (AdvanceIgnoreMatcher(c))
            {
                continue;
            }

            if (InvokeParts(c, out var token))
            {
                return token;
            }

            if (InvokePunctuators(out var next))
            {
                return next;
            }

            if (char.IsLetter(c))
            {
                return LexName().WithDocument(Document);
            }

            Document.Messages.Add(Message.Error($"Invalid Character '{c}'", SourceRange.From(Document, _line, _column, _line, _column)));

            return Token.Invalid(c, _line, _column).WithDocument(Document);
        }

        return new Token(PredefinedSymbols.EOF, _line, _column).WithDocument(Document);
    }

    private void RecognizeLine(char c)
    {
        if (c != '\r')
        {
            return;
        }

        _line++;
        _column = 1;
    }

    private bool InvokePunctuators(out Token token)
    {
        foreach (var punctuator in _punctuators)
        {
            if (!IsMatch(punctuator.Key))
            {
                continue;
            }

            {
                token = LexSymbol(punctuator.Key).WithDocument(Document);
                return true;
            }
        }

        token = default;
        return false;
    }

    private bool InvokeParts(char c, out Token token)
    {
        foreach (var part in _parts)
        {
            if (part.Match(this, c))
            {
                {
                    token = part.Build(this, ref _index, ref _column, ref _line).WithDocument(Document);
                    return true;
                }
            }
        }

        token = default;
        return false;
    }

    private bool AdvanceIgnoreMatcher(char c)
    {
        foreach (var ignoreMatcher in _ignoreMatcher)
        {
            if (ignoreMatcher.Match(this, c))
            {
                ignoreMatcher.Advance(this);

                return true;
            }
        }

        return false;
    }

    private Token LexSymbol(string punctuatorKey)
    {
        var oldColumn = _column;

        _column += punctuatorKey.Length;
        _index += punctuatorKey.Length;

        return new(punctuatorKey, _line, oldColumn);
    }

    private Token LexName()
    {
        var oldColumn = _column;

        var start = _index;
        while (_index < Document.Source.Length)
        {
            if (!char.IsLetter(Peek(0)))
            {
                break;
            }

            Advance();
        }

        var nameSlice = Document.Source[start.._index];
        var name = nameSlice.ToString();

        if (_punctuators.ContainsKey(name))
        {
            return new(name, nameSlice, _line, oldColumn);
        }

        return new(PredefinedSymbols.Name, nameSlice, _line, oldColumn);
    }

    public void Advance(int distance = 1)
    {
        _index += distance;
        _column += distance;
    }

    public bool ContainsSymbol(string tokenName)
    {
        return _punctuators.ContainsKey(tokenName);
    }

    public void AddSymbols(params string[] symbols)
    {
        foreach (var symbol in symbols)
        {
            AddSymbol(symbol);
        }
    }
}
