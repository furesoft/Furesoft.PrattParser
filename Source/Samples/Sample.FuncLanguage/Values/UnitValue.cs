namespace Sample.FuncLanguage;

public record UnitValue() : Value
{
    public static readonly Value Shared = new UnitValue();

    public override bool IsTruthy() => false;

    public override string ToString() => "()";
}
