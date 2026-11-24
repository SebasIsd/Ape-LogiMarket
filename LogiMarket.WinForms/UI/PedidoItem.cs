using LogiMarket.Domain.Entities;

namespace LogiMarket.WinForms.UI;

/// <summary>
/// Elemento legible para los ComboBox de pedidos: "#A1B2C3D4 · Cliente · $59.00 · 23/11 19:49".
/// Evita mostrar el GUID completo y permite recuperar el Id con SelectedValue.
/// </summary>
public sealed class PedidoItem
{
    public Guid Id { get; }
    public string Texto { get; }

    public PedidoItem(Pedido p)
    {
        Id = p.Id;
        var corto = p.Id.ToString("N")[..8].ToUpperInvariant();
        Texto = $"#{corto} · {p.Cliente?.Nombre ?? "Cliente"} · $ {p.Total.Amount:0.00} · {p.FechaCreacion.ToLocalTime():dd/MM HH:mm}";
    }

    public static List<PedidoItem> De(IEnumerable<Pedido> pedidos) => pedidos.Select(p => new PedidoItem(p)).ToList();

    public override string ToString() => Texto;
}
