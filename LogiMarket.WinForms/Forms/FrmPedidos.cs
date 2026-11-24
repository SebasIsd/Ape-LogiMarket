using LogiMarket.WinForms.Mediator;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;
using LogiMarket.WinForms.UI;
using System.Windows.Forms;
using LogiMarket.WinForms.Interpreter;

namespace LogiMarket.WinForms.Forms;

public class FrmPedidos : UserControl, IViewRefrescable
{
    // --- Componentes ---
    private TextBox txtFiltro;
    private Button btnFiltrar;
    private Button btnLimpiar;
    private Button btnVerDetalle;
    private DataGridView dgvPedidos;
    private GroupBox grpDetalle;
    private Label lbId;
    private Label lbCliente;
    private Label lbEstado;
    private Label lbFecha;
    private Label lbSubtotal;
    private Label lbDescuento;
    private Label lbTarifa;
    private Label lbTotal;
    private Label lbEstrategia;
    private Label lbPago;
    private TableLayoutPanel pnlRaiz;

    private readonly IRepositorioPedido _repo;
    private readonly IFormularioMediator _mediator;
    private List<Pedido> _pedidos = new();

    public FrmPedidos(IRepositorioPedido repo, IFormularioMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
        _mediator.Registrar(this);
        InicializarComponentes();
        Load += async (_, _) => await CargarPedidosAsync();
        VisibleChanged += async (_, _) => { if (Visible) await CargarPedidosAsync(); };
    }

    private void InicializarComponentes()
    {
        BackColor = UIHelper.ColorGrisClaro;

        pnlRaiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(15)
        };
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

        // --- Panel superior: Filtro + Grid ---
        var pnlSuperior = new Panel { Dock = DockStyle.Fill };

        var lbFiltro = UIHelper.CrearLabel("Filtro (Interpreter):", negrita: true);
        lbFiltro.Location = new Point(0, 8);

        txtFiltro = UIHelper.CrearTextBox(400);
        txtFiltro.Location = new Point(130, 5);
        txtFiltro.PlaceholderText = "Ej: ESTADO = PAGADO AND TOTAL >= 50";

        btnFiltrar = UIHelper.CrearBotonPrimario("Filtrar", 90);
        btnFiltrar.Location = new Point(540, 4);
        btnFiltrar.Click += async (_, _) => await CargarPedidosAsync();

        btnLimpiar = UIHelper.CrearBotonGris("Limpiar", 80);
        btnLimpiar.Location = new Point(640, 4);
        btnLimpiar.Click += async (_, _) => { txtFiltro.Clear(); await CargarPedidosAsync(); };

        btnVerDetalle = UIHelper.CrearBotonSecundario("Ver detalle", 110);
        btnVerDetalle.Location = new Point(730, 4);
        btnVerDetalle.Click += async (_, _) => await AbrirDetalleAsync();

        dgvPedidos = UIHelper.CrearDataGridView();
        dgvPedidos.Location = new Point(0, 45);
        dgvPedidos.Size = new Size(pnlSuperior.Width - 20, pnlSuperior.Height - 55);
        dgvPedidos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        dgvPedidos.SelectionChanged += (_, _) => MostrarDetalle();
        dgvPedidos.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await AbrirDetalleAsync(); };

        pnlSuperior.Controls.AddRange(new Control[] { lbFiltro, txtFiltro, btnFiltrar, btnLimpiar, btnVerDetalle, dgvPedidos });
        pnlSuperior.Resize += (_, _) => dgvPedidos.Size = new Size(pnlSuperior.Width - 20, pnlSuperior.Height - 55);
        pnlRaiz.Controls.Add(pnlSuperior, 0, 0);

        // --- Panel inferior: Detalle ---
        grpDetalle = UIHelper.CrearGroupBox("Detalle del pedido (State)");
        grpDetalle.Dock = DockStyle.Fill;

        var pnlDetalle = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 4,
            Padding = new Padding(15, 10, 15, 10)
        };
        pnlDetalle.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        pnlDetalle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        pnlDetalle.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        pnlDetalle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        for (int i = 0; i < 4; i++)
            pnlDetalle.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

        pnlDetalle.Controls.Add(UIHelper.CrearLabel("ID:", negrita: true), 0, 0);
        lbId = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteNormal, TextAlign = ContentAlignment.MiddleLeft };
        pnlDetalle.Controls.Add(lbId, 1, 0);

        pnlDetalle.Controls.Add(UIHelper.CrearLabel("Cliente:", negrita: true), 2, 0);
        lbCliente = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteNormal, TextAlign = ContentAlignment.MiddleLeft };
        pnlDetalle.Controls.Add(lbCliente, 3, 0);

        pnlDetalle.Controls.Add(UIHelper.CrearLabel("Estado:", negrita: true), 0, 1);
        lbEstado = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteNormalNegrita, ForeColor = UIHelper.ColorPrimario, TextAlign = ContentAlignment.MiddleLeft };
        pnlDetalle.Controls.Add(lbEstado, 1, 1);

        pnlDetalle.Controls.Add(UIHelper.CrearLabel("Fecha:", negrita: true), 2, 1);
        lbFecha = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteNormal, TextAlign = ContentAlignment.MiddleLeft };
        pnlDetalle.Controls.Add(lbFecha, 3, 1);

        pnlDetalle.Controls.Add(UIHelper.CrearLabel("Subtotal:", negrita: true), 0, 2);
        lbSubtotal = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteNormal, TextAlign = ContentAlignment.MiddleLeft };
        pnlDetalle.Controls.Add(lbSubtotal, 1, 2);

        pnlDetalle.Controls.Add(UIHelper.CrearLabel("Descuento:", negrita: true), 2, 2);
        lbDescuento = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteNormal, TextAlign = ContentAlignment.MiddleLeft };
        pnlDetalle.Controls.Add(lbDescuento, 3, 2);

        pnlDetalle.Controls.Add(UIHelper.CrearLabel("Tarifa:", negrita: true), 0, 3);
        lbTarifa = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteNormal, TextAlign = ContentAlignment.MiddleLeft };
        pnlDetalle.Controls.Add(lbTarifa, 1, 3);

        pnlDetalle.Controls.Add(UIHelper.CrearLabel("TOTAL:", negrita: true), 2, 3);
        lbTotal = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteGrande, ForeColor = UIHelper.ColorPrimario, TextAlign = ContentAlignment.MiddleLeft };
        pnlDetalle.Controls.Add(lbTotal, 3, 3);

        grpDetalle.Controls.Add(pnlDetalle);
        pnlRaiz.Controls.Add(grpDetalle, 0, 1);

        Controls.Add(pnlRaiz);
    }

    private async Task CargarPedidosAsync()
    {
        try
        {
            _pedidos = (await _repo.ListarAsync()).ToList();

            // Interpreter: aplicar filtro si existe
            if (!string.IsNullOrWhiteSpace(txtFiltro.Text))
            {
                try
                {
                    var filtro = InterpreterFiltro.Parse(txtFiltro.Text.Trim());
                    _pedidos = _pedidos.Where(p => filtro.Evaluar(p)).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error en filtro: {ex.Message}", "Interpreter",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            dgvPedidos.DataSource = null;
            dgvPedidos.Columns.Clear();
            dgvPedidos.Columns.Add("colId", "ID");
            dgvPedidos.Columns.Add("colCliente", "Cliente");
            dgvPedidos.Columns.Add("colEstado", "Estado");
            dgvPedidos.Columns.Add("colFecha", "Fecha");
            dgvPedidos.Columns.Add("colTotal", "Total");

            foreach (var p in _pedidos)
            {
                dgvPedidos.Rows.Add(
                    p.Id.ToString("N")[..8],
                    p.Cliente?.Nombre ?? "—",
                    p.EstadoActual,
                    p.FechaCreacion.ToString("yyyy-MM-dd HH:mm"),
                    p.Total.Amount.ToString("0.00"));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task AbrirDetalleAsync()
    {
        try
        {
            if (dgvPedidos.CurrentRow is null)
            {
                MessageBox.Show("Seleccione un pedido de la tabla.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var idStr = dgvPedidos.CurrentRow.Cells["colId"].Value?.ToString();
            var resumen = _pedidos.FirstOrDefault(p => p.Id.ToString("N")[..8] == idStr);
            if (resumen is null) return;

            // Recarga el agregado completo (líneas, pago, despacho, devolución)
            var pedido = await _repo.ObtenerPorIdAsync(resumen.Id) ?? resumen;
            using var dlg = new FrmDetallePedido(pedido);
            dlg.ShowDialog(FindForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void MostrarDetalle()
    {
        if (dgvPedidos.CurrentRow is null) return;
        var idStr = dgvPedidos.CurrentRow.Cells["colId"].Value?.ToString();
        var pedido = _pedidos.FirstOrDefault(p => p.Id.ToString("N")[..8] == idStr);
        if (pedido is null) return;

        lbId.Text = pedido.Id.ToString("N")[..8];
        lbCliente.Text = pedido.Cliente?.Nombre ?? "—";
        lbEstado.Text = pedido.EstadoActual;
        lbFecha.Text = pedido.FechaCreacion.ToString("yyyy-MM-dd HH:mm");
        lbSubtotal.Text = $"$ {pedido.Subtotal.Amount:0.00}";
        lbDescuento.Text = $"$ {pedido.Descuento.Amount:0.00} ({pedido.EstrategiaDescuentoAplicada})";
        lbTarifa.Text = $"$ {pedido.TarifaEnvio.Amount:0.00}";
        lbTotal.Text = $"$ {pedido.Total.Amount:0.00}";
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) => CargarPedidosAsync().ConfigureAwait(false);
    public void OnStockModificado() { }
    public void OnCatalogoActualizado() { }
    public void OnDespachoRegistrado(Guid pedidoId) => CargarPedidosAsync().ConfigureAwait(false);
    public void OnDevolucionProcesada(Guid pedidoId) => CargarPedidosAsync().ConfigureAwait(false);
}