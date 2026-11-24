using LogiMarket.Domain.Entities;

namespace LogiMarket.WinForms.UI;

public class CarritoSesion
{
    public Pedido? PedidoActual { get; set; }
    public Pedido? BorradorOriginal { get; set; }
    public Stack<Pedido> PilaDeshacer { get; } = new();
    public Stack<Pedido> PilaRehacer { get; } = new();
}