using LogiMarket.WinForms.Mediator;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public partial class FrmCarrito : UserControl, IViewRefrescable
{
    public FrmCarrito()
    {
        var lbl = new Label { Text = "🛒 Carrito / Borrador (pendiente de implementar)", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(Font, FontStyle.Bold) };
        Controls.Add(lbl);
    }
    public void OnPedidoConfirmado(Guid pedidoId) { }
    public void OnStockModificado() { }
    public void OnCatalogoActualizado() { }
    public void OnDespachoRegistrado(Guid pedidoId) { }
    public void OnDevolucionProcesada(Guid pedidoId) { }
}