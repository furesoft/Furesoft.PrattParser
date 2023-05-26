using System.Collections.Generic;
using Furesoft.PrattParser.Parselets;
using Furesoft.PrattParser.Parselets.Operators;

namespace Furesoft.PrattParser;

public abstract class Parser<T>
{
    private Lexer _lexer;
    private List<Token> _read = new();
    private Dictionary<Symbol, IPrefixParselet<T>> _prefixParselets = new();
    private Dictionary<Symbol, IInfixParselet<T>> _infixParselets = new();

    public void Register(Symbol token, IPrefixParselet<T> parselet)
    {
        _prefixParselets.Add(token, parselet);
    }

    public void Register(Symbol token, IInfixParselet<T> parselet)
    {
        _infixParselets.Add(token, parselet);
    }

    public void Register(string token, IInfixParselet<T> parselet)
    {
        _infixParselets.Add(PredefinedSymbols.Pool.Get(token), parselet);
    }

    public void Group(Symbol leftToken, Symbol rightToken)
    {
        Register(leftToken, (IPrefixParselet<T>)new GroupParselet(rightToken));
    }

    public void Group(string left, string right)
    {
        Group(PredefinedSymbols.Pool.Get(left), PredefinedSymbols.Pool.Get(right));
    }

    public void Block(Symbol seperator, Symbol terminator)
    {
        Register(seperator, (IInfixParselet<T>)new BlockParselet(seperator, terminator));
    }
    
    public static TranslationUnit<T> Parse<TParser>(string source, string filename = "syntethic.dsl") 
        where TParser : Parser<T>, new()
    {
        var lexer = new Lexer(source, filename);

        var parser = new TParser();
        parser!._lexer = lexer;
        
        parser.InitLexer(lexer);

        return new(parser.Parse(), lexer.Document);
    }

    public T Parse(int precedence)
    {
        var token = Consume();

        if (!_prefixParselets.TryGetValue(token.Type, out var prefix))
        {
            token.Document.Messages.Add(Message.Error(("Could not parse \"" + token.Text + "\".")));
        }

        var left = prefix.Parse(this, token);

        while (precedence < GetBindingPower())
        {
            token = Consume();

            if (!_infixParselets.TryGetValue(token.Type, out var infix))
            {
                token.Document.Messages.Add(Message.Error(("Could not parse \"" + token.Text + "\".")));
            }

            left = infix.Parse(this, left, token);
        }

        return left;
    }

    public T Parse()
    {
        return Parse(0);
    }

    protected abstract void InitLexer(Lexer lexer);
    
    public List<T> ParseSeperated(Symbol seperator, Symbol terminator)
    {
        var args = new List<T>();

        if (Match(terminator))
        {
            return new();
        }

        do
        {
            args.Add(Parse());
        } while (Match(seperator));

        Consume(terminator);

        return args;
    }

    public bool Match(Symbol expected)
    {
        if (!IsMatch(expected))
        {
            return false;
        }

        Consume();
        
        return true;
    }

    public bool IsMatch(Symbol expected, uint distance = 0)
    {
        var token = LookAhead(distance);

        return token.Type.Equals(expected);
    }

    public bool IsMatch(params Symbol[] expected)
    {
        var result = true;
        for (uint i = 0; i < expected.Length; i++)
        {
            result &= IsMatch(expected[i], i);
        }

        return result;
    }

    public Token Consume(Symbol expected)
    {
        var token = LookAhead(0);

        if (!token.Type.Equals(expected))
        {
            token.Document.Messages.Add(Message.Error($"Expected token {expected} and found {token.Type}({token})"));
        }

        return Consume();
    }

    public Token Consume()
    {
        // Make sure we've read the token.
        var token = LookAhead(0);
        _read.Remove(token);

        return token;
    }

    public Token[] ConsumeMany(uint count)
    {
        var result = new List<Token>();
        for (int i = 0; i < count; i++)
        {
            result.Add(Consume());
        }

        return result.ToArray();
    }

    public Token LookAhead(uint distance)
    {
        // Read in as many as needed.
        while (distance >= _read.Count)
        {
            _read.Add(_lexer.Next());
        }

        // Get the queued token.
        return _read[(int)distance];
    }

    private int GetBindingPower()
    {
        if (_infixParselets.TryGetValue(LookAhead(0).Type, out var parselet))
        {
            return parselet.GetBindingPower();
        }

        return 0;
    }

    /// <summary>
    /// Registers a postfix unary operator parselet for the given token and binding power.
    /// </summary>
    public void Postfix(Symbol token, int bindingPower)
    {
        Register(token, (IInfixParselet<T>)new PostfixOperatorParselet(bindingPower));
    }
    

    /// <summary>
    /// Registers a prefix unary operator parselet for the given token and binding power.
    /// </summary>
    public void Prefix(Symbol token, int bindingPower)
    {
        Register(token, (IPrefixParselet<T>)new PrefixOperatorParselet(bindingPower));
    }

    /// <summary>
    ///  Registers a left-associative binary operator parselet for the given token and binding power.
    /// </summary>
    public void InfixLeft(Symbol token, int bindingPower)
    {
        Register(token, (IInfixParselet<T>)new BinaryOperatorParselet(bindingPower, false));
    }

    /// <summary>
    /// Registers a right-associative binary operator parselet for the given token and binding power.
    /// </summary>
    public void InfixRight(Symbol token, int bindingPower)
    {
        Register(token, (IInfixParselet<T>)new BinaryOperatorParselet(bindingPower, true));
    }

    /// <summary>
    /// Register a ternary operator like the :? operator
    /// </summary>
    /// <param name="firstSymbol"></param>
    /// <param name="secondSymbol"></param>
    /// <param name="bindingPower"></param>
    public void Ternary(Symbol firstSymbol, Symbol secondSymbol, int bindingPower)
    {
        Register(firstSymbol, (IInfixParselet<T>)new TernaryOperatorParselet(secondSymbol, bindingPower));
    }
}
