using LogiMarket.Application.Commands;
using LogiMarket.Application.DTOs;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;
using LogiMarket.WinForms.Interpreter;
using LogiMarket.WinForms.Mediator;
using LogiMarket.WinForms.UI;
using System.Windows.Forms;

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
    private TableLayoutPanel pnlRaiz;

    // --- CQRS: Query para lectura de lista ---
    private readonly IQuery<IReadOnlyList<PedidoDto>> _obtenerPedidosQuery;
    // --- Repo: solo para obtener agregado completo (detalle / filtro) ---
    private readonly IRepositorioPedido _repo;
    private readonly IFormularioMediator _mediator;

    private List<PedidoDto> _pedidosDto = new();
    private List<Pedido> _pedidosDominio = new(); // cache para Interpreter

    public FrmPedidos(
        IQuery<IReadOnlyList<PedidoDto>> obtenerPedidosQuery,
        IRepositorioPedido repo,
        IFormularioMediator mediator)
    {
        _obtenerPedidosQuery = obtenerPedidosQuery;
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
        txtFiltro.Location = new Point(140, 5);
        txtFiltro.PlaceholderText = "Ej: ESTADO = PAGADO AND TOTAL >= 50";

        btnFiltrar = UIHelper.CrearBotonPrimario("Filtrar", 90);
        btnFiltrar.Location = new Point(550, 4);
        btnFiltrar.Click += async (_, _) => await CargarPedidosAsync();

        btnLimpiar = UIHelper.CrearBotonGris("Limpiar", 80);
        btnLimpiar.Location = new Point(650, 4);
        btnLimpiar.Click += async (_, _) => { txtFiltro.Clear(); await CargarPedidosAsync(); };

        btnVerDetalle = UIHelper.CrearBotonSecundario("Ver detalle", 110);
        btnVerDetalle.Location = new Point(740, 4);
        btnVerDetalle.Click += async (_, _) => await AbrirDetalleAsync();

        dgvPedidos = UIHelper.CrearDataGridView();
        dgvPedidos.Location = new Point(0, 45);
        dgvPedidos.Size = new Size(pnlSuperior.Width - 20, pnlSuperior.Height - 55);
        dgvPedidos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        dgvPedidos.SelectionChanged += (_, _) => MostrarDetalle();

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
            // CQRS: usar el Query para obtener la lista (lectura)
            _pedidosDto = (await _obtenerPedidosQuery.ExecuteAsync()).ToList();

            // Cache de dominio para Interpreter (solo si hay filtro)
            _pedidosDominio = new List<Pedido>();

            var listaMostrar = _pedidosDto.AsEnumerable();

            // Interpreter: aplicar filtro si existe (necesita dominio)
            if (!string.IsNullOrWhiteSpace(txtFiltro.Text))
            {
                try
                {
                    // Cargar dominio solo si hay filtro
                    if (_pedidosDominio.Count == 0)
                    {
                        var todos = await _repo.ListarAsync();
                        _pedidosDominio = todos.ToList();
                    }

                    var filtro = InterpreterFiltro.Parse(txtFiltro.Text.Trim());
                    var filtrados = _pedidosDominio.Where(p => filtro.Evaluar(p)).ToList();
                    var idsFiltrados = filtrados.Select(p => p.Id).ToHashSet();
                    listaMostrar = _pedidosDto.Where(p => idsFiltrados.Contains(p.Id));
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

            foreach (var p in listaMostrar)
            {
                dgvPedidos.Rows.Add(
                    p.Id.ToString("N")[..8],
                    p.ClienteNombre,
                    p.Estado,
                    p.FechaCreacion.ToString("yyyy-MM-dd HH:mm"),
                    p.Total.ToString("0.00"));
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
                MessageBox.Show("Seleccione un pedido de la tabla.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var idStr = dgvPedidos.CurrentRow.Cells["colId"].Value?.ToString();
            var resumen = _pedidosDto.FirstOrDefault(p => p.Id.ToString("N")[..8] == idStr);
            if (resumen is null) return;

            // Obtener agregado completo del repo
            var pedido = await _repo.ObtenerPorIdAsync(resumen.Id);
            if (pedido is null) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Pedido: {pedido.Id:N}");
            sb.AppendLine($"Cliente: {pedido.Cliente?.Nombre ?? "—"}");
            sb.AppendLine($"Estado: {pedido.EstadoActual}");
            sb.AppendLine($"Subtotal: $ {pedido.Subtotal.Amount:0.00}");
            sb.AppendLine($"Descuento: $ {pedido.Descuento.Amount:0.00} ({pedido.EstrategiaDescuentoAplicada})");
            sb.AppendLine($"Tarifa: $ {pedido.TarifaEnvio.Amount:0.00}");
            sb.AppendLine($"TOTAL: $ {pedido.Total.Amount:0.00}");
            sb.AppendLine();
            sb.AppendLine("Líneas:");
            foreach (var d in pedido.Detalles)
                sb.AppendLine($"  • {d.Sku} - {d.Nombre} x{d.Cantidad} = ${d.SubtotalLinea.Amount:0.00}");

            MessageBox.Show(sb.ToString(), $"Detalle del pedido {resumen.Id.ToString("N")[..8]}",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        var pedido = _pedidosDto.FirstOrDefault(p => p.Id.ToString("N")[..8] == idStr);
        if (pedido is null) return;

        lbId.Text = pedido.Id.ToString("N")[..8];
        lbCliente.Text = pedido.ClienteNombre;
        lbEstado.Text = pedido.Estado;
        lbFecha.Text = pedido.FechaCreacion.ToString("yyyy-MM-dd HH:mm");
        lbSubtotal.Text = $"$ {pedido.Subtotal:0.00}";
        lbDescuento.Text = $"$ {pedido.Descuento:0.00} ({pedido.Estrategia})";
        lbTarifa.Text = $"$ {pedido.Tarifa:0.00}";
        lbTotal.Text = $"$ {pedido.Total:0.00}";
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) => CargarPedidosAsync().ConfigureAwait(false);
    public void OnStockModificado() { }
    public void OnCatalogoActualizado() { }
    public void OnDespachoRegistrado(Guid pedidoId) => CargarPedidosAsync().ConfigureAwait(false);
    public void OnDevolucionProcesada(Guid pedidoId) => CargarPedidosAsync().ConfigureAwait(false);
}