using LogiMarket.Application.Ports.Documents;
using LogiMarket.Domain.Entities;

namespace LogiMarket.Infrastructure.Documents;

public class ComprobanteVenta : IComprobante
{
    private readonly Pedido _pedido;
    public ComprobanteVenta(Pedido pedido) { _pedido = pedido; }

    public string Tipo => "COMPROBANTE DE VENTA";

    public Documentosalida GenerarContenido()
    {
        var lineas = new List<string>
        {
            $"Pedido: {_pedido.Id}",
            $"Cliente: {_pedido.Cliente?.Nombre}",
            $"Estado: {_pedido.EstadoActual}",
            $"Fecha: {_pedido.FechaCreacion:yyyy-MM-dd HH:mm}",
            string.Empty,
            "Detalle:"
        };
        foreach (var d in _pedido.Detalles)
            lineas.Add($"  {d.Sku} - {d.Nombre} x{d.Cantidad} @ {d.PrecioUnitarioInstantanea.Amount:0.00} = {d.SubtotalLinea.Amount:0.00}");
        lineas.Add(string.Empty);
        lineas.Add($"Subtotal:   {_pedido.Subtotal.Amount:0.00}");
        lineas.Add($"Descuento:  {_pedido.Descuento.Amount:0.00} ({_pedido.EstrategiaDescuentoAplicada})");
        lineas.Add($"Tarifa:     {_pedido.TarifaEnvio.Amount:0.00}");
        lineas.Add($"TOTAL:      {_pedido.Total.Amount:0.00}");
        if (_pedido.Pago is not null)
            lineas.Add($"Pago: {_pedido.Pago.Referencia}");

        return new Documentosalida($"Venta-{_pedido.Id}", Tipo, lineas.AsReadOnly(), DateTime.UtcNow);
    }
}