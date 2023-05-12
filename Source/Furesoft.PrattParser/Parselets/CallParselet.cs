using System.Collections.Generic;
using System.Text;
using Furesoft.PrattParser.Expressions;

namespace Furesoft.PrattParser.Parselets;

/// <summary>
/// Parselet to parse a function call like "a(b, c, d)".
/// </summary>
public class CallParselet : IInfixParselet<IExpression> {
   public IExpression Parse(Parser<IExpression> parser, IExpression left, Token token) {
      // Parse the comma-separated arguments until we hit, ')'.
      var args = parser.ParseSeperated(TokenType.Comma, TokenType.RightParen);

      return new CallExpression(left, args);
   }

   public int GetBindingPower() {
      return BindingPower.Call;
   }
}
