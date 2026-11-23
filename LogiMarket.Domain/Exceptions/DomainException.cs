namespace LogiMarket.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string mensaje) : base(mensaje) { }
    public DomainException(string mensaje, Exception inner) : base(mensaje, inner) { }
}

public class EstadoInvalidoException : DomainException
{
    public EstadoInvalidoException(string mensaje) : base(mensaje) { }
}

public class StockInsuficienteException : DomainException
{
    public StockInsuficienteException(string sku)
        : base($"Stock insuficiente para el SKU {sku}.") { }
}

public class ReferenciaCircularException : DomainException
{
    public ReferenciaCircularException(string skuKit)
        : base($"Referencia circular detectada en el kit {skuKit}.") { }
}

public class OperacionNoPermitidaException : DomainException
{
    public OperacionNoPermitidaException(string mensaje) : base(mensaje) { }
}