namespace LogiMarket.Domain.ValueObjects;

/// <summary>
/// Objeto de valor monetario inmutable. Usa MidpointRounding.AwayFromZero
/// conforme a la regla de negocio oficial.
/// </summary>
public readonly record struct Money(decimal Amount) : IComparable<Money>
{
    public static Money Zero => new(0m);

    public Money Add(Money other) => new(Amount + other.Amount);
    public Money Subtract(Money other) => new(Amount - other.Amount);

    public Money Multiply(decimal factor) =>
        new(Math.Round(Amount * factor, 2, MidpointRounding.AwayFromZero));

    public Money Multiply(int factor) =>
        new(Math.Round(Amount * factor, 2, MidpointRounding.AwayFromZero));
    //esta es la forma de multiplicar un Money por un factor decimal o entero, redondeando a 2 decimales usando MidpointRounding.AwayFromZero
    public static Money operator +(Money a, Money b) => a.Add(b);
    public static Money operator -(Money a, Money b) => a.Subtract(b);
    public static bool operator >(Money a, Money b) => a.Amount > b.Amount;
    public static bool operator <(Money a, Money b) => a.Amount < b.Amount;
    public static bool operator >=(Money a, Money b) => a.Amount >= b.Amount;
    public static bool operator <=(Money a, Money b) => a.Amount <= b.Amount;

    public int CompareTo(Money other) => Amount.CompareTo(other.Amount);

    public override string ToString() => Amount.ToString("0.00");
}