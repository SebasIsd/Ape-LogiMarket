using LogiMarket.Domain.Entities;

namespace LogiMarket.Application.Validation;

public class PipelineValidacionPedido
{
    private readonly IValidadorPedido _head;

    public PipelineValidacionPedido(IEnumerable<IValidadorPedido> validadores)
    {
        var lista = validadores.ToList();
        if (lista.Count == 0) throw new ArgumentException("Pipeline sin validadores.");

        IValidadorPedido? actual = null;
        foreach (var v in lista.AsEnumerable().Reverse())
        {
            actual = CrearConSiguiente(v, actual);
        }
        _head = actual!;
    }

    private static IValidadorPedido CrearConSiguiente(IValidadorPedido v, IValidadorPedido? siguiente)
    {
        // Cada validador acepta el siguiente en su constructor
        var tipo = v.GetType();
        var ctor = tipo.GetConstructor(new[] { typeof(IValidadorPedido) });
        if (ctor is null) return v;
        return (IValidadorPedido)ctor.Invoke(new object?[] { siguiente });
    }

    public Task<ResultadoValidacion> EjecutarAsync(Pedido pedido, CancellationToken ct)
        => _head.ValidarAsync(pedido, ct);
}