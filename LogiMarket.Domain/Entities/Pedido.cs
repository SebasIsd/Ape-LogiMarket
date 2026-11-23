using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.States;
using LogiMarket.Domain.ValueObjects;

namespace LogiMarket.Domain.Entities;

public class Pedido : ICloneable
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public Cliente? Cliente { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaPago { get; private set; }
    public DateTime? FechaDespacho { get; private set; }
    public DateTime? FechaEntrega { get; private set; }

    public Money Subtotal { get; private set; }
    public Money Descuento { get; private set; }
    public Money TarifaEnvio { get; private set; }
    public Money Total { get; private set; }

    public string TipoEnvio { get; private set; } = default!;
    public string EstrategiaDescuentoAplicada { get; private set; } = "NORMAL";

    private IEstadoPedido _estado = default!;
    public string EstadoActual { get; private set; } = "BORRADOR";

    private readonly List<PedidoDetalle> _detalles = new();
    public IReadOnlyCollection<PedidoDetalle> Detalles => _detalles.AsReadOnly();

    public Pago? Pago { get; private set; }
    public Despacho? Despacho { get; private set; }
    public Devolucion? Devolucion { get; private set; }

    public int Version { get; private set; } // concurrencia optimista

    protected Pedido() { _estado = new BorradorState(); } // EF

    public Pedido(Guid clienteId, Cliente cliente, TipoEnvio tipoEnvio)
    {
        if (clienteId == Guid.Empty) throw new DomainException("Cliente inválido.");
        if (cliente is null) throw new DomainException("Cliente nulo.");
        if (!cliente.Activo) throw new OperacionNoPermitidaException("Cliente inactivo.");

        Id = Guid.NewGuid();
        ClienteId = clienteId;
        Cliente = cliente;
        FechaCreacion = DateTime.UtcNow;
        TipoEnvio = tipoEnvio.ToString();
        _estado = new BorradorState();
    }

    public void AgregarLinea(IElementoCatalogo elemento, int cantidad)
    {
        AsegurarEditable();
        if (elemento is null) throw new DomainException("Elemento nulo.");
        if (cantidad <= 0) throw new DomainException("Cantidad inválida.");

        _detalles.Add(new PedidoDetalle(elemento, cantidad));
        RecalcularTotales();
    }

    public void ActualizarCantidad(string sku, int nuevaCantidad)
    {
        AsegurarEditable();
        var linea = _detalles.FirstOrDefault(d => d.Sku == sku)
            ?? throw new DomainException($"Línea {sku} no encontrada.");
        linea.ActualizarCantidad(nuevaCantidad);
        RecalcularTotales();
    }

    public void EliminarLinea(string sku)
    {
        AsegurarEditable();
        var linea = _detalles.FirstOrDefault(d => d.Sku == sku)
            ?? throw new DomainException($"Línea {sku} no encontrada.");
        _detalles.Remove(linea);
        RecalcularTotales();
    }

    public void AplicarDescuento(Money descuento, string estrategia)
    {
        if (descuento.Amount < 0) throw new DomainException("Descuento negativo.");
        Descuento = descuento;
        EstrategiaDescuentoAplicada = estrategia;
        RecalcularTotal();
    }

    public void AplicarTarifa(Money tarifa)
    {
        if (tarifa.Amount < 0) throw new DomainException("Tarifa negativa.");
        TarifaEnvio = tarifa;
        RecalcularTotal();
    }

    private void RecalcularTotales()
    {
        Subtotal = _detalles.Aggregate(Money.Zero, (acc, d) => acc.Add(d.SubtotalLinea));
        RecalcularTotal();
    }

    private void RecalcularTotal()
    {
        Total = Subtotal.Subtract(Descuento).Add(TarifaEnvio);
    }

    public int UnidadesFisicasFinales()
    {
        int total = 0;
        foreach (var d in _detalles)
        {
            //aqui suma las cantidades de cada detalle del pedido, sin importar si es un kit o un producto simple
            total += d.Cantidad; 
        }
        return total;
    }

    // --- Transiciones de estado (State Pattern) ---
    public void Validar() => _estado.Validar(new PedidoContext(this, CambiarEstado));
    public void Pagar(Pago pago)
    {
        _estado.Pagar(new PedidoContext(this, CambiarEstado));
        Pago = pago ?? throw new DomainException("Pago nulo.");
        FechaPago = DateTime.UtcNow;
    }
    public void Despachar(Despacho despacho)
    {
        _estado.Despachar(new PedidoContext(this, CambiarEstado));
        Despacho = despacho ?? throw new DomainException("Despacho nulo.");
        FechaDespacho = DateTime.UtcNow;
    }
    public void Entregar()
    {
        _estado.Entregar(new PedidoContext(this, CambiarEstado));
        FechaEntrega = DateTime.UtcNow;
    }
    public void Cancelar() => _estado.Cancelar(new PedidoContext(this, CambiarEstado));
    // metodo para cambiar el estado del pedido, se llama desde los estados concretos
    private void CambiarEstado(IEstadoPedido nuevo)
    {
        _estado = nuevo;
        EstadoActual = nuevo.Nombre;
        Version++;
    }

    private void AsegurarEditable()
    {
        if (_estado.Nombre != "BORRADOR")
            throw new OperacionNoPermitidaException(
                $"El pedido en estado {_estado.Nombre} no es editable.");
    }

    // --- Prototype: clonación profunda del borrador ---
    public object Clone()
    {
        if (_estado.Nombre != "BORRADOR")
            throw new OperacionNoPermitidaException("Solo se clonan pedidos en BORRADOR.");

        var clon = (Pedido)MemberwiseClone();
        clon.Id = Guid.NewGuid(); // nuevo ID
        clon.FechaCreacion = DateTime.UtcNow;
        clon.Pago = null;
        clon.Despacho = null;
        clon.Devolucion = null;
        clon.Version = 0;
        clon._estado = new BorradorState();

        // Copia profunda de la colección mutable
        var detallesCopiados = new List<PedidoDetalle>();
        foreach (var d in _detalles)
        {
            var nuevo = new PedidoDetalle(d.Sku, d.Nombre, d.EsKit, d.Cantidad, d.PrecioUnitarioInstantanea);
            detallesCopiados.Add(nuevo);
        }
        clon.GetType().GetField("_detalles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(clon, detallesCopiados);

        return clon;
    }
}