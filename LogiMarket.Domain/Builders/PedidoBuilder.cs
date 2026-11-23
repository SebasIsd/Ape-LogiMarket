using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Domain.Builders;

/// <summary>
/// Builder del agregado Pedido. Garantiza que no se puedan crear pedidos inválidos
/// (sin cliente, sin líneas, con tarifa negativa, etc.).
/// </summary>
public class PedidoBuilder
{
    private Cliente? _cliente;
    private TipoEnvio _tipoEnvio = TipoEnvio.Estandar;
    private readonly List<(IElementoCatalogo Elemento, int Cantidad)> _lineas = new();
    private Money? _descuento;
    private string _estrategia = "NORMAL";
    private Money? _tarifa;

    public PedidoBuilder ConCliente(Cliente cliente)
    {
        _cliente = cliente ?? throw new DomainException("Cliente nulo.");
        return this;
    }

    public PedidoBuilder ConEnvio(TipoEnvio tipo)
    {
        _tipoEnvio = tipo;
        return this;
    }

    public PedidoBuilder AgregarLinea(IElementoCatalogo elemento, int cantidad)
    {
        if (elemento is null) throw new DomainException("Elemento nulo.");
        if (cantidad <= 0) throw new DomainException("Cantidad inválida.");
        _lineas.Add((elemento, cantidad));
        return this;
    }

    public PedidoBuilder ConDescuento(Money descuento, string estrategia)
    {
        if (descuento.Amount < 0) throw new DomainException("Descuento negativo.");
        _descuento = descuento;
        _estrategia = estrategia;
        return this;
    }

    public PedidoBuilder ConTarifa(Money tarifa)
    {
        if (tarifa.Amount < 0) throw new DomainException("Tarifa negativa.");
        _tarifa = tarifa;
        return this;
    }

    public Pedido Construir()
    {
        if (_cliente is null)
            throw new DomainException("El pedido requiere un cliente.");
        if (_lineas.Count == 0)
            throw new DomainException("El pedido requiere al menos una línea.");

        var pedido = new Pedido(_cliente.Id, _cliente, _tipoEnvio);
        foreach (var (elem, cant) in _lineas)
            pedido.AgregarLinea(elem, cant);

        if (_descuento.HasValue)
            pedido.AplicarDescuento(_descuento.Value, _estrategia);
        if (_tarifa.HasValue)
            pedido.AplicarTarifa(_tarifa.Value);

        return pedido;
    }
}