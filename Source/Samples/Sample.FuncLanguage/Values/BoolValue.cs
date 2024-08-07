namespace Silverfly.Sample.Func.Values;

public record BoolValue(bool Value) : Value
{
    public override string ToString() => Value.ToString();

    public override bool IsTruthy() => Value;
}
