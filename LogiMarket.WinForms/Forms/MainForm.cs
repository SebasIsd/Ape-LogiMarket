using LogiMarket.Application.Metrics;
using LogiMarket.Application.Ports.Payments;
using LogiMarket.WinForms.Mediator;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public partial class MainForm : Form, IViewRefrescable
{
    private readonly IFormularioMediator _mediator;
    private readonly ISesionUsuario _sesion;
    private readonly MetricasSesion _metricas;
    private readonly IServiceProvider _sp;
    private readonly TabControl _tabs;

    public MainForm(
        IFormularioMediator mediator,
        ISesionUsuario sesion,
        MetricasSesion metricas,
        IServiceProvider sp)
    {
        _mediator = mediator;
        _sesion = sesion;
        _metricas = metricas;
        _sp = sp;

        _mediator.Registrar(this);

        Text = $"LogiMarket 360 — {_sesion.Username} ({_sesion.Rol})";
        Size = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterScreen;

        _tabs = new TabControl { Dock = DockStyle.Fill };
        Controls.Add(_tabs);

        CargarPestanas();
        CargarMenuSuperior();
    }

    private void CargarPestanas()
    {
        AgregarPestana("Panel", () => new PanelGeneralControl(_metricas, _sesion));
        AgregarPestana("Catálogo", () => _sp.GetRequiredService<FrmCatalogo>());
        AgregarPestana("Clientes", () => _sp.GetRequiredService<FrmClientes>());
        AgregarPestana("Carrito", () => _sp.GetRequiredService<FrmCarrito>());
        AgregarPestana("Pedidos", () => _sp.GetRequiredService<FrmPedidos>());
        AgregarPestana("Cobro", () => _sp.GetRequiredService<FrmCobro>());
        AgregarPestana("Despacho", () => _sp.GetRequiredService<FrmDespacho>());
        AgregarPestana("Devoluciones", () => _sp.GetRequiredService<FrmDevoluciones>());
        AgregarPestana("Inventario", () => _sp.GetRequiredService<FrmInventario>());
        AgregarPestana("Reportes", () => _sp.GetRequiredService<FrmReportes>());
        AgregarPestana("Auditoría", () => _sp.GetRequiredService<FrmAuditoria>());
        AgregarPestana("Demo", () => _sp.GetRequiredService<FrmDemo>());
    }

    private void AgregarPestana(string titulo, Func<Control> creador)
    {
        var tab = new TabPage(titulo);
        try
        {
            var control = creador();
            control.Dock = DockStyle.Fill;
            tab.Controls.Add(control);
        }
        catch (Exception ex)
        {
            tab.Controls.Add(new Label { Text = $"Error: {ex.Message}", Dock = DockStyle.Fill });
        }
        _tabs.TabPages.Add(tab);
    }

    private void CargarMenuSuperior()
    {
        var menu = new MenuStrip();
        var sesionMenu = new ToolStripMenuItem("Sesión");
        sesionMenu.DropDownItems.Add("Cerrar sesión", null, (_, _) =>
        {
            if (MessageBox.Show("¿Cerrar sesión?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _sesion.CerrarSesion();
                Close();
            }
        });
        menu.Items.Add(sesionMenu);
        MainMenuStrip = menu;
        Controls.Add(menu);
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) => MessageBox.Show($"Pedido {pedidoId:N} confirmado.", "Éxito");
    public void OnStockModificado() { }
    public void OnCatalogoActualizado() { }
    public void OnDespachoRegistrado(Guid pedidoId) { }
    public void OnDevolucionProcesada(Guid pedidoId) { }
}