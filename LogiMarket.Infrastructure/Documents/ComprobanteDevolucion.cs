using LogiMarket.Application.Ports.Documents;
using LogiMarket.Domain.Entities;

namespace LogiMarket.Infrastructure.Documents;

public class ComprobanteDevolucion : IComprobante
{
    private readonly Pedido _pedido;
    private readonly Devolucion _devolucion;
    public ComprobanteDevolucion(Pedido pedido, Devolucion devolucion) { _pedido = pedido; _devolucion = devolucion; }

    public string Tipo => "COMPROBANTE DE DEVOLUCIÓN";

    public Documentosalida GenerarContenido()
    {
        var lineas = new List<string>
        {
            $"Devolución: {_devolucion.Id}",
            $"Pedido: {_pedido.Id}",
            $"Cliente: {_pedido.Cliente?.Nombre}",
            $"Motivo: {_devolucion.Motivo}",
            $"Aceptada: {(_devolucion.Aceptada ? "Sí" : "No")}",
            $"Reembolso: {_devolucion.Reembolso.Amount:0.00}",
            $"Fecha: {_devolucion.FechaSolicitud:yyyy-MM-dd HH:mm}"
        };
        return new Documentosalida($"Devolucion-{_devolucion.Id}", Tipo, lineas.AsReadOnly(), DateTime.UtcNow);
    }
}