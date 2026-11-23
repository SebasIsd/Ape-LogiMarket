using LogiMarket.Application.Facades;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Application.Ports.Payments;
using LogiMarket.Domain.Entities;
using LogiMarket.WinForms.Mediator;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public class FrmCobro : UserControl, IViewRefrescable
{
    // --- Componentes ---
    private Label lbPedido;
    private ComboBox cmbPedidos;
    private Button btnCargar;
    private GroupBox grpResumen;
    private Label lbSubtotal;
    private Label lbDescuento;
    private Label lbTarifa;
    private Label lbTotal;
    private Label lbEstado;
    private Label lbEstrategia;
    private Label lbIdempotencia;
    private TextBox txtIdempotencia;
    private Button btnGenerarClave;
    private Button btnCobrar;
    private Button btnCancelar;
    private GroupBox grpTrazabilidad;
    private TextBox txtLog;
    private TableLayoutPanel pnlRaiz;

    private readonly ConfirmacionPedidoFacade _facade;
    private readonly IRepositorioPedido _repo;
    private readonly ISesionUsuario _sesion;
    private readonly IFormularioMediator _mediator;

    public FrmCobro(ConfirmacionPedidoFacade facade, IRepositorioPedido repo,
        ISesionUsuario sesion, IFormularioMediator mediator)
    {
        _facade = facade;
        _repo = repo;
        _sesion = sesion;
        _mediator = mediator;
        _mediator.Registrar(this);

        InicializarComponentes();
        Load += CargarPedidosValidados;
    }

    private void InicializarComponentes()
    {
        BackColor = Color.FromArgb(245, 247, 250);

        pnlRaiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(15)
        };
        pnlRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        pnlRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

        // --- Fila 0: selector de pedido ---
        lbPedido = new Label { Text = "Pedido VALIDADO:", AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(0, 10, 5, 0) };
        cmbPedidos = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
        btnCargar = new Button { Text = "Cargar", Width = 80, Height = 30, BackColor = Color.FromArgb(30, 120, 200), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnCargar.FlatAppearance.BorderSize = 0;
        btnCargar.Click += async (_, _) => await CargarPedidoAsync();

        var pnlSelector = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 5, 0, 5) };
        pnlSelector.Controls.AddRange(new Control[] { lbPedido, cmbPedidos, btnCargar });
        pnlRaiz.Controls.Add(pnlSelector, 0, 0);
        pnlRaiz.SetColumnSpan(pnlSelector, 2);

        // --- Fila 1, col 0: Resumen ---
        grpResumen = new GroupBox { Text = "Resumen del pedido", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(30, 60, 90) };
        var pnlResumen = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, Padding = new Padding(10) };
        pnlResumen.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        pnlResumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        lbSubtotal = new Label { Text = "Subtotal: —", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
        lbDescuento = new Label { Text = "Descuento: —", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
        lbEstrategia = new Label { Text = "Estrategia: —", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
        lbTarifa = new Label { Text = "Tarifa: —", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
        lbTotal = new Label { Text = "TOTAL: —", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(30, 120, 200) };
        lbEstado = new Label { Text = "Estado: —", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };

        var lblSub = new Label { Text = "Subtotal:", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9) };
        var lblDesc = new Label { Text = "Descuento:", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9) };
        var lblEstr = new Label { Text = "Estrategia:", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9) };
        var lblTar = new Label { Text = "Tarifa:", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9) };
        var lblTot = new Label { Text = "TOTAL:", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        var lblEst = new Label { Text = "Estado:", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9) };

        pnlResumen.Controls.Add(lblSub, 0, 0); pnlResumen.Controls.Add(lbSubtotal, 1, 0);
        pnlResumen.Controls.Add(lblDesc, 0, 1); pnlResumen.Controls.Add(lbDescuento, 1, 1);
        pnlResumen.Controls.Add(lblEstr, 0, 2); pnlResumen.Controls.Add(lbEstrategia, 1, 2);
        pnlResumen.Controls.Add(lblTar, 0, 3); pnlResumen.Controls.Add(lbTarifa, 1, 3);
        pnlResumen.Controls.Add(lblTot, 0, 4); pnlResumen.Controls.Add(lbTotal, 1, 4);
        pnlResumen.Controls.Add(lblEst, 0, 5); pnlResumen.Controls.Add(lbEstado, 1, 5);
        grpResumen.Controls.Add(pnlResumen);
        pnlRaiz.Controls.Add(grpResumen, 0, 1);

        // --- Fila 1, col 1: Trazabilidad (Decorator + Proxy) ---
        grpTrazabilidad = new GroupBox { Text = "Trazabilidad de cobro (Decorator/Proxy/Adapter)", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(30, 60, 90) };
        txtLog = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 9), BackColor = Color.FromArgb(20, 20, 20), ForeColor = Color.FromArgb(0, 255, 100) };
        grpTrazabilidad.Controls.Add(txtLog);
        pnlRaiz.Controls.Add(grpTrazabilidad, 1, 1);

        // --- Fila 2: idempotencia + botones ---
        var pnlIdempotencia = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 10, 0, 5) };
        lbIdempotencia = new Label { Text = "Clave idempotencia:", AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), Margin = new Padding(0, 8, 5, 0) };
        txtIdempotencia = new TextBox { Width = 250, Font = new Font("Consolas", 10) };
        btnGenerarClave = new Button { Text = "Generar", Width = 80, Height = 28, BackColor = Color.FromArgb(120, 80, 180), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnGenerarClave.FlatAppearance.BorderSize = 0;
        btnGenerarClave.Click += (_, _) => txtIdempotencia.Text = Guid.NewGuid().ToString("N");

        pnlIdempotencia.Controls.AddRange(new Control[] { lbIdempotencia, txtIdempotencia, btnGenerarClave });
        pnlRaiz.Controls.Add(pnlIdempotencia, 0, 2);
        pnlRaiz.SetColumnSpan(pnlIdempotencia, 2);

        var pnlBotonera = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 5, 0, 5) };
        btnCancelar = new Button { Text = "Cancelar", Width = 100, Height = 34, BackColor = Color.FromArgb(230, 230, 230), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };
        btnCancelar.FlatAppearance.BorderSize = 0;
        btnCancelar.Click += (_, _) => { cmbPedidos.SelectedIndex = -1; Limpiar(); };

        btnCobrar = new Button { Text = "💳 Cobrar", Width = 120, Height = 34, BackColor = Color.FromArgb(40, 160, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        btnCobrar.FlatAppearance.BorderSize = 0;
        btnCobrar.Click += async (_, _) => await CobrarAsync();

        pnlBotonera.Controls.AddRange(new Control[] { btnCancelar, btnCobrar });
        pnlRaiz.Controls.Add(pnlBotonera, 1, 2);

        Controls.Add(pnlRaiz);
    }

    private async void CargarPedidosValidados(object? sender, EventArgs e)
    {
        var pedidos = await _repo.ListarAsync();
        var validados = pedidos.Where(p => p.EstadoActual == "VALIDADO").ToList();
        cmbPedidos.DataSource = validados;
        cmbPedidos.DisplayMember = "Id";
        cmbPedidos.ValueMember = "Id";
    }

    private async Task CargarPedidoAsync()
    {
        if (cmbPedidos.SelectedItem is not Guid id) return;
        var pedido = await _repo.ObtenerPorIdAsync(id);
        if (pedido is null) return;

        lbSubtotal.Text = $"$ {pedido.Subtotal.Amount:0.00}";
        lbDescuento.Text = $"$ {pedido.Descuento.Amount:0.00}";
        lbEstrategia.Text = pedido.EstrategiaDescuentoAplicada;
        lbTarifa.Text = $"$ {pedido.TarifaEnvio.Amount:0.00}";
        lbTotal.Text = $"$ {pedido.Total.Amount:0.00}";
        lbEstado.Text = pedido.EstadoActual;

        txtLog.AppendText($"[INFO] Pedido {pedido.Id:N} cargado. Rol actual: {_sesion.Rol}\r\n");
    }

    private void Limpiar()
    {
        lbSubtotal.Text = lbDescuento.Text = lbTarifa.Text = lbTotal.Text = "—";
        lbEstrategia.Text = lbEstado.Text = "—";
        txtIdempotencia.Clear();
        txtLog.Clear();
    }

    private async Task CobrarAsync()
    {
        if (cmbPedidos.SelectedItem is not Guid id)
        {
            MessageBox.Show("Seleccione un pedido.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(txtIdempotencia.Text))
        {
            MessageBox.Show("Genere una clave de idempotencia.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnCobrar.Enabled = false;
        txtLog.AppendText($"[INFO] Iniciando cobro de {id:N}...\r\n");

        try
        {
            var resultado = await _facade.ConfirmarAsync(id, txtIdempotencia.Text.Trim());
            if (resultado.Exito)
            {
                txtLog.AppendText($"[ÉXITO] Cobro aprobado. Datos: {resultado.Datos}\r\n");
                MessageBox.Show("Cobro realizado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                _mediator.NotificarPedidoConfirmado(id);
                _mediator.NotificarStockModificado();
                CargarPedidosValidados(null, EventArgs.Empty);
            }
            else
            {
                txtLog.AppendText($"[RECHAZO] {resultado.Mensaje}\r\n");
                MessageBox.Show($"Cobro rechazado: {resultado.Mensaje}", "Rechazo",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            txtLog.AppendText($"[ERROR] {ex.Message}\r\n");
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnCobrar.Enabled = true;
        }
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) { }
    public void OnStockModificado() { }
    public void OnCatalogoActualizado() { }
    public void OnDespachoRegistrado(Guid pedidoId) { }
    public void OnDevolucionProcesada(Guid pedidoId) { }
}