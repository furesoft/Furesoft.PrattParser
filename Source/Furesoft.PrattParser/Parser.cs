using System.Collections.Generic;
using Furesoft.PrattParser.Parselets;

namespace Furesoft.PrattParser;

public class Parser<T> {
   private Lexer _tokens;
   private List<Token> _read = new();
   private Dictionary<TokenType, IPrefixParselet<T>> _prefixParselets = new();
   private Dictionary<TokenType, IInfixParselet<T>> _infixParselets = new();

   public Parser(Lexer tokens) {
      _tokens = tokens;
   }

   public void Register(TokenType token, IPrefixParselet<T> parselet) {
      _prefixParselets.Add(token, parselet);
   }

   public void Register(TokenType token, IInfixParselet<T> parselet) {
      _infixParselets.Add(token, parselet);
   }
      
   public void Group(TokenType leftToken, TokenType rightToken)
   {
      Register(leftToken, (IPrefixParselet<T>)new GroupParselet(rightToken));
   }

   public List<T> ParseSeperated(TokenType seperator, TokenType terminator)
   {
       var args = new List<T>();

       // There may be no arguments at all.
       if (!Match(terminator)) {
           do {
               args.Add(Parse());
           } while (Match(seperator));
           Consume(terminator);
       }

       return args;
   }

   public void Skip(TokenType tokenTypeToSkip)
   {
       while (LookAhead(0).Type.Equals(tokenTypeToSkip))
       {
           Consume();
       }
   }

   public T Parse(int precedence) {
      var token = Consume();

      if (!_prefixParselets.TryGetValue((TokenType)token.Type, out var prefix)) {
         throw new ParseException("Could not parse \"" + token.Text + "\".");
      }

      var left = prefix.Parse(this, token);

      while (precedence < GetBindingPower()) {
         token = Consume();

         if (!_infixParselets.TryGetValue((TokenType)token.Type, out var infix)) {
            throw new ParseException("Could not parse \"" + token.Text + "\".");
         }
         left = infix.Parse(this, left, token);
      }

      return left;
   }

   public T Parse() {
      return Parse(0);
   }

   public bool Match(TokenType expected) {
      var token = LookAhead(0);
      if ((TokenType)token.Type != expected) {
         return false;
      }

      Consume();
      return true;
   }

   public Token Consume(TokenType expected) {
      var token = LookAhead(0);
      if ((TokenType)token.Type != expected) {
         throw new ParseException("Expected token " + expected + " and found " + token.Type);
      }

      return Consume();
   }

   public Token Consume() {
      // Make sure we've read the token.
      var token = LookAhead(0);
      _read.Remove(token);
      return token;
   }

   private Token LookAhead(int distance) {
      // Read in as many as needed.
      while (distance >= _read.Count) {
         _read.Add(_tokens.Next());
      }

      // Get the queued token.
      return _read[distance];
   }

   private int GetBindingPower() {
      if (_infixParselets.TryGetValue((TokenType)LookAhead(0).Type, out var parselet)) {
         return parselet.GetBindingPower();
      }
      return 0;
   }
}
