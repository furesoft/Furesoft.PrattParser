using System.Collections.Immutable;
using Silverfly;
using Silverfly.Nodes;
using Silverfly.Nodes.Operators;
using Sample.FuncLanguage.Nodes;
using Sample.FuncLanguage.Values;
using Silverfly.Generator;
using Silverfly.Text;

namespace Sample.FuncLanguage;

[Visitor]
public partial class EvaluationVisitor : TaggedNodeVisitor<Value, Scope>
{

    Value Visit(ImportNode node, Scope scope)
    {
        var file = new FileInfo($"{node.Path}.f");

        var content = File.ReadAllText(file.FullName);
        var parsed = Parser.Parse<ExpressionGrammar>(content, file.FullName);
        var rewritten = parsed.Tree.Accept(new RewriterVisitor());

        rewritten.Accept(new EvaluationVisitor(), scope);

        return UnitValue.Shared;
    }


    Value Visit(BinaryOperatorNode binNode, Scope scope)
    {
        var leftVisited = Visit(binNode.LeftExpr, scope);
        var rightVisited = Visit(binNode.RightExpr, scope);

        if (binNode.Operator == (Symbol)".")
        {
            return leftVisited.Get(rightVisited);
        }

        var leftValue = ((NumberValue)leftVisited).Value;
        var rightValue = ((NumberValue)rightVisited).Value;

        var result = binNode.Operator.Name switch
        {
            "+" => leftValue + rightValue,
            "-" => leftValue - rightValue,
            "*" => leftValue * rightValue,
            "/" => leftValue / rightValue,
            _ => 0
        };

        return new NumberValue(result);
    }
    
    Value Visit(GroupNode group, Scope scope) => Visit(group.Expr, scope);
    
    Value Visit(BlockNode block, Scope scope)
    {
        for (int i = 0; i < block.Children.Count - 1; i++)
        {
            Visit(block.Children[i], scope);
        }

        return Visit(block.Children.Last(), scope); // the last expression is the return value
    }
    
    Value Visit(VariableBindingNode binding, Scope scope)
    {
        if (binding.Parameters.Count == 0)
        {
            scope.Define(binding.Name.Text.ToString(), Visit(binding.Value, scope));
        }
        else
        {
            scope.Define(binding.Name.Text.ToString(), args => CallFunction(binding.Parameters, args, binding.Value));
        }

        return UnitValue.Shared;
    }
    
    Value Visit(IfNode ifNode, Scope scope)
    {
        var evaluatedCondition = Visit(ifNode.Condition, scope);

        if (evaluatedCondition.IsTruthy())
        {
            return Visit(ifNode.TruePart, scope.NewSubScope());
        }
        else
        {
            return Visit(ifNode.FalsePart, scope.NewSubScope());
        }
    }
    
    Value Visit(LambdaNode lambda, Scope scope)
    {
        return new LambdaValue(args => CallFunction(lambda.Parameters, args, lambda.Value), lambda);
    }
    
    Value Visit(NameNode name, Scope scope)
    {
        return scope.Get(name.Name)! ?? new NameValue(name.Name);
    }
    
    Value Visit(CallNode call, Scope scope)
    {
        return call.FunctionExpr switch
        {
            NameNode func => VisitNamedFunction(func, call, scope),
            LambdaNode funcGroup => VisitLambdaFunction(funcGroup, call, scope),
            CallNode c => Visit(c, scope),
            _ => VisitOtherFunction(call, scope)
        };
    }

    [VisitorIgnore]
    Value VisitOtherFunction(CallNode call, Scope scope)
    {
        var args = call.Arguments.Select(_ => Visit(_, scope)).ToArray();
        var func = Visit(call.FunctionExpr, scope);

        if (func is LambdaValue l)
        {
            return (Value)l.Value.DynamicInvoke([args]);
        }

        return UnitValue.Shared;
    }

    [VisitorIgnore]
    private Value VisitLambdaFunction(LambdaNode funcGroup, CallNode call, Scope scope)
    {
        var args = call.Arguments.Select(_ => Visit(_, scope)).ToArray();
        var f = Visit(funcGroup, scope);

        if (f is LambdaValue l)
        {
            return l.Value(args);
        }

        return UnitValue.Shared;
    }

    [VisitorIgnore]
    private Value VisitNamedFunction(NameNode func, CallNode call, Scope scope)
    {
        var args = call.Arguments.Select(arg => Visit(arg, scope)).ToArray();
        var f = scope.Get(func.Name);

        if (f is LambdaValue lambda)
        {
            return lambda.Value(args);
        }

        func.Parent.AddMessage(MessageSeverity.Error, $"Function '{func.Name}' not found");

        return UnitValue.Shared;
    }
    
    [VisitorIgnore]
    protected override Value VisitUnknown(AstNode node, Scope tag)
    {
        if (node is InvalidNode invalid)
        {
            node.Range.Document.Messages.Add(Message.Error("Cannot evaluate " + invalid.Token.Text, invalid.Range));
        }

        return UnitValue.Shared;
    }
    
    [VisitorIgnore]
    private Value CallFunction(ImmutableList<NameNode> parameters, Value[] args, AstNode definition)
    {
        var subScope = Scope.Root.NewSubScope();
        for (int i = 0; i < parameters.Count; i++)
        {
            var index = i;
            subScope.Define(parameters[index].Name, args[index]);
        }

        return Visit(definition, subScope);
    }
    
    private Value Visit(LiteralNode literal, Scope scope)
    {
        if (literal.Value is double d)
        {
            return new NumberValue(d);
        }
        else if (literal.Value is bool b)
        {
            return new BoolValue(b);
        }
        else if (literal.Value is string s)
        {
            return new StringValue(s);
        }
        else if (literal.Value is UnitValue unit)
        {
            return unit;
        }
        else if (literal.Value is ImmutableList<AstNode> v)
        {
            return new ListValue(v.Select(_ => Visit(_, scope)).ToList());
        }

        return UnitValue.Shared;
    }
    
    Value Visit(TupleNode tuple, Scope scope)
    {
        return new TupleValue(tuple.Values.Select(_ => Visit(_, scope)).ToList());
    }
}
