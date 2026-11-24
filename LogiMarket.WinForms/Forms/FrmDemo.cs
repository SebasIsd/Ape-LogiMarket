using LogiMarket.Application.Ports.Payments;
using LogiMarket.Domain.Enums;
using LogiMarket.WinForms.Mediator;
using LogiMarket.WinForms.UI;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public class FrmDemo : UserControl, IViewRefrescable
{
    // --- Componentes ---
    private GroupBox grpProxy;
    private ComboBox cmbRol;
    private Button btnSimularCobro;
    private GroupBox grpAdapter;
    private ComboBox cmbProveedor;
    private Button btnCambiarProveedor;
    private GroupBox grpOutbox;
    private Button btnSimularFallo;
    private TextBox txtLog;
    private TableLayoutPanel pnlRaiz;

    private readonly ISesionUsuario _sesion;
    private readonly IFormularioMediator _mediator;

    public FrmDemo(ISesionUsuario sesion, IFormularioMediator mediator)
    {
        _sesion = sesion;
        _mediator = mediator;
        _mediator.Registrar(this);
        InicializarComponentes();
    }

    private void InicializarComponentes()
    {
        BackColor = UIHelper.ColorGrisClaro;

        pnlRaiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            Padding = new Padding(15)
        };
        pnlRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        pnlRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        pnlRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        // --- P22: Proxy ---
        grpProxy = UIHelper.CrearGroupBox("P22: Proxy (autorización por rol)");
        grpProxy.Dock = DockStyle.Fill;
        var pnlProxy = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(10)
        };
        pnlProxy.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        pnlProxy.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        pnlProxy.Controls.Add(UIHelper.CrearLabel("Rol:", negrita: true), 0, 0);
        cmbRol = UIHelper.CrearComboBox();
        cmbRol.Items.AddRange(Enum.GetNames(typeof(RolUsuario)));
        cmbRol.SelectedItem = _sesion.Rol.ToString();
        pnlProxy.Controls.Add(cmbRol, 1, 0);

        btnSimularCobro = UIHelper.CrearBotonPeligro("Simular cobro", 150);
        btnSimularCobro.Click += SimularCobro;
        pnlProxy.Controls.Add(btnSimularCobro, 1, 1);

        var lbResultado = UIHelper.CrearLabel("Resultado:");
        pnlProxy.Controls.Add(lbResultado, 0, 2);

        grpProxy.Controls.Add(pnlProxy);
        pnlRaiz.Controls.Add(grpProxy, 0, 0);

        // --- P16: Adapter ---
        grpAdapter = UIHelper.CrearGroupBox("P16: Adapter (proveedor legado/alternativo)");
        grpAdapter.Dock = DockStyle.Fill;
        var pnlAdapter = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(10)
        };
        pnlAdapter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        pnlAdapter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        pnlAdapter.Controls.Add(UIHelper.CrearLabel("Proveedor:", negrita: true), 0, 0);
        cmbProveedor = UIHelper.CrearComboBox();
        cmbProveedor.Items.AddRange(new object[] { "Legado (Centavos)", "Alternativo" });
        cmbProveedor.SelectedIndex = 0;
        pnlAdapter.Controls.Add(cmbProveedor, 1, 0);

        btnCambiarProveedor = UIHelper.CrearBotonSecundario("Cambiar proveedor", 180);
        btnCambiarProveedor.Click += (_, _) => MessageBox.Show($"Proveedor cambiado a: {cmbProveedor.SelectedItem}", "Adapter", MessageBoxButtons.OK, MessageBoxIcon.Information);
        pnlAdapter.Controls.Add(btnCambiarProveedor, 1, 1);

        grpAdapter.Controls.Add(pnlAdapter);
        pnlRaiz.Controls.Add(grpAdapter, 1, 0);

        // --- P37: Outbox ---
        grpOutbox = UIHelper.CrearGroupBox("P37: Outbox (falla y recuperación)");
        grpOutbox.Dock = DockStyle.Fill;
        var pnlOutbox = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10)
        };

        btnSimularFallo = UIHelper.CrearBotonPeligro("Simular fallo de notificador", 200);
        btnSimularFallo.Click += (_, _) => MessageBox.Show("Evento Outbox marcado como fallido. Se reintentará en el próximo procesamiento.", "Outbox", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        pnlOutbox.Controls.Add(btnSimularFallo, 0, 0);

        grpOutbox.Controls.Add(pnlOutbox);
        pnlRaiz.Controls.Add(grpOutbox, 2, 0);

        // --- Log ---
        txtLog = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = UIHelper.FuenteMonospace,
            BackColor = Color.FromArgb(20, 20, 20),
            ForeColor = Color.FromArgb(0, 255, 100)
        };
        pnlRaiz.Controls.Add(txtLog, 0, 1);
        pnlRaiz.SetColumnSpan(txtLog, 3);

        Controls.Add(pnlRaiz);
    }

    private void SimularCobro(object? sender, EventArgs e)
    {
        var rolSeleccionado = (RolUsuario)Enum.Parse(typeof(RolUsuario), cmbRol.SelectedItem?.ToString() ?? "Consulta");
        var autorizado = rolSeleccionado == RolUsuario.Admin || rolSeleccionado == RolUsuario.Ventas;

        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Simulación de cobro con rol {rolSeleccionado}\r\n");
        if (autorizado)
            txtLog.AppendText($"  ✓ Autorizado: el cobro llegaría al Decorator → Adapter\r\n");
        else
            txtLog.AppendText($"  ✗ Rechazado por Proxy: rol {rolSeleccionado} no tiene permiso\r\n");
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) { }
    public void OnStockModificado() { }
    public void OnCatalogoActualizado() { }
    public void OnDespachoRegistrado(Guid pedidoId) { }
    public void OnDevolucionProcesada(Guid pedidoId) { }
}