using LogiMarket.Application.Ports.Payments;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Application.Shipping;
using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Enums;
using LogiMarket.WinForms.Mediator;
using LogiMarket.WinForms.UI;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public class FrmDespacho : UserControl, IViewRefrescable
{
    // --- Componentes ---
    private ComboBox cmbPedidosPagados;
    private Button btnCargar;
    private GroupBox grpResumen;
    private Label lbId;
    private Label lbCliente;
    private Label lbTotal;
    private GroupBox grpEnvio;
    private ComboBox cmbTipoEnvio;
    private Label lbTarifa;
    private Label lbPlazo;
    private Label lbGuia;
    private Label lbEtiqueta;
    private Button btnDespachar;
    private Button btnEntregar;
    private DataGridView dgvDespachos;
    private TableLayoutPanel pnlRaiz;

    private readonly IRepositorioPedido _repo;
    private readonly IEnvioAbstractFactory _envioFactory;
    private readonly IFormularioMediator _mediator;
    private readonly IRepositorioAuditoria _auditoria;
    private readonly ISesionUsuario _sesion;
    private Pedido? _pedidoSeleccionado;

    public FrmDespacho(IRepositorioPedido repo, IEnvioAbstractFactory envioFactory, IFormularioMediator mediator,
        IRepositorioAuditoria auditoria, ISesionUsuario sesion)
    {
        _auditoria = auditoria;
        _sesion = sesion;
        _repo = repo;
        _envioFactory = envioFactory;
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
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(15)
        };
        pnlRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        pnlRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        // --- Fila 0: Selector ---
        var pnlSelector = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 10, 0, 5)
        };
        var lbPedido = UIHelper.CrearLabel("Pedido PAGADO:", negrita: true);
        lbPedido.Margin = new Padding(0, 8, 5, 0);
        cmbPedidosPagados = UIHelper.CrearComboBox(430);
        cmbPedidosPagados.Margin = new Padding(0, 5, 10, 0);
        btnCargar = UIHelper.CrearBotonPrimario("Cargar", 90);
        btnCargar.Click += async (_, _) => await CargarPedidoAsync();
        cmbPedidosPagados.SelectionChangeCommitted += async (_, _) => await CargarPedidoAsync();

        pnlSelector.Controls.AddRange(new Control[] { lbPedido, cmbPedidosPagados, btnCargar });
        pnlRaiz.Controls.Add(pnlSelector, 0, 0);
        pnlRaiz.SetColumnSpan(pnlSelector, 2);

        // --- Fila 1, col 0: Resumen ---
        grpResumen = UIHelper.CrearGroupBox("Resumen del pedido");
        grpResumen.Dock = DockStyle.Fill;
        var pnlResumen = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(15, 10, 15, 10)
        };
        pnlResumen.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        pnlResumen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        pnlResumen.Controls.Add(UIHelper.CrearLabel("ID:", negrita: true), 0, 0);
        lbId = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteNormal, TextAlign = ContentAlignment.MiddleLeft };
        pnlResumen.Controls.Add(lbId, 1, 0);

        pnlResumen.Controls.Add(UIHelper.CrearLabel("Cliente:", negrita: true), 0, 1);
        lbCliente = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteNormal, TextAlign = ContentAlignment.MiddleLeft };
        pnlResumen.Controls.Add(lbCliente, 1, 1);

        pnlResumen.Controls.Add(UIHelper.CrearLabel("Total:", negrita: true), 0, 2);
        lbTotal = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteGrande, ForeColor = UIHelper.ColorPrimario, TextAlign = ContentAlignment.MiddleLeft };
        pnlResumen.Controls.Add(lbTotal, 1, 2);

        grpResumen.Controls.Add(pnlResumen);
        pnlRaiz.Controls.Add(grpResumen, 0, 1);

        // --- Fila 1, col 1: Envío (Abstract Factory) ---
        grpEnvio = UIHelper.CrearGroupBox("Familia de envío (Abstract Factory)");
        grpEnvio.Dock = DockStyle.Fill;
        var pnlEnvio = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(15, 10, 15, 10)
        };
        pnlEnvio.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        pnlEnvio.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        pnlEnvio.Controls.Add(UIHelper.CrearLabel("Tipo:", negrita: true), 0, 0);
        cmbTipoEnvio = UIHelper.CrearComboBox();
        cmbTipoEnvio.Items.AddRange(new object[] { "ESTÁNDAR", "EXPRÉS" });
        cmbTipoEnvio.SelectedIndex = 0;
        cmbTipoEnvio.SelectedIndexChanged += ActualizarFamiliaEnvio;
        pnlEnvio.Controls.Add(cmbTipoEnvio, 1, 0);

        pnlEnvio.Controls.Add(UIHelper.CrearLabel("Tarifa:", negrita: true), 0, 1);
        lbTarifa = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteNormal, TextAlign = ContentAlignment.MiddleLeft };
        pnlEnvio.Controls.Add(lbTarifa, 1, 1);

        pnlEnvio.Controls.Add(UIHelper.CrearLabel("Plazo:", negrita: true), 0, 2);
        lbPlazo = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteNormal, TextAlign = ContentAlignment.MiddleLeft };
        pnlEnvio.Controls.Add(lbPlazo, 1, 2);

        pnlEnvio.Controls.Add(UIHelper.CrearLabel("Guía:", negrita: true), 0, 3);
        lbGuia = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteNormal, TextAlign = ContentAlignment.MiddleLeft };
        pnlEnvio.Controls.Add(lbGuia, 1, 3);

        grpEnvio.Controls.Add(pnlEnvio);
        pnlRaiz.Controls.Add(grpEnvio, 1, 1);

        // --- Fila 2: Botones + Grid ---
        var pnlBotones = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 10, 0, 5)
        };
        btnDespachar = UIHelper.CrearBotonSecundario("🚚 Despachar", 150);
        btnDespachar.Click += async (_, _) => await DespacharAsync();
        btnEntregar = UIHelper.CrearBotonPrimario("✓ Entregar", 150);
        btnEntregar.Click += async (_, _) => await EntregarAsync();

        pnlBotones.Controls.AddRange(new Control[] { btnDespachar, btnEntregar });
        pnlRaiz.Controls.Add(pnlBotones, 0, 2);

        dgvDespachos = UIHelper.CrearDataGridView();
        dgvDespachos.Dock = DockStyle.Fill;
        dgvDespachos.Columns.Add("colId", "ID");
        dgvDespachos.Columns.Add("colPedido", "Pedido");
        dgvDespachos.Columns.Add("colGuia", "Guía");
        dgvDespachos.Columns.Add("colTipo", "Tipo");
        dgvDespachos.Columns.Add("colFecha", "Fecha despacho");
        pnlRaiz.Controls.Add(dgvDespachos, 1, 2);

        Controls.Add(pnlRaiz);
    }

    private async Task CargarPedidosAsync()
    {
        var pedidos = await _repo.ListarAsync();
        var pagados = pedidos.Where(p => p.EstadoActual == "PAGADO" || p.EstadoActual == "DESPACHADO").ToList();
        cmbPedidosPagados.DataSource = PedidoItem.De(pagados);
        cmbPedidosPagados.DisplayMember = "Texto";
        cmbPedidosPagados.ValueMember = "Id";
        if (cmbPedidosPagados.SelectedValue is Guid) await CargarPedidoAsync();
        else _pedidoSeleccionado = null;
    }

    private async Task CargarPedidoAsync()
    {
        if (cmbPedidosPagados.SelectedValue is not Guid id) return;
        _pedidoSeleccionado = await _repo.ObtenerPorIdAsync(id);
        if (_pedidoSeleccionado is null) return;

        lbId.Text = _pedidoSeleccionado.Id.ToString("N")[..8];
        lbCliente.Text = _pedidoSeleccionado.Cliente?.Nombre ?? "—";
        lbTotal.Text = $"$ {_pedidoSeleccionado.Total.Amount:0.00}";
        ActualizarFamiliaEnvio(null, EventArgs.Empty);
    }

    private void ActualizarFamiliaEnvio(object? sender, EventArgs e)
    {
        var tipo = cmbTipoEnvio.SelectedIndex == 0 ? TipoEnvio.Estandar : TipoEnvio.Expres;
        var familia = _envioFactory.Crear(tipo);
        lbTarifa.Text = $"$ {familia.CalcularTarifa().Amount:0.00}";
        lbPlazo.Text = $"{familia.PlazoDias} día(s)";
        lbGuia.Text = familia.GenerarGuia();
    }

    private async Task DespacharAsync()
    {
        if (_pedidoSeleccionado is null)
        {
            MessageBox.Show("Seleccione un pedido.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var tipo = cmbTipoEnvio.SelectedIndex == 0 ? TipoEnvio.Estandar : TipoEnvio.Expres;
            var familia = _envioFactory.Crear(tipo);
            var despacho = new Despacho(_pedidoSeleccionado.Id, familia.GenerarGuia(), familia.Nombre, familia.PlazoDias);
            _pedidoSeleccionado.Despachar(despacho);
            await _repo.ActualizarAsync(_pedidoSeleccionado);
            await _auditoria.AgregarAsync(new Auditoria(_sesion.Username, "DESPACHO",
                $"Pedido {_pedidoSeleccionado.Id.ToString("N")[..8].ToUpperInvariant()} despachado. Guía {despacho.Guia}"));
            MessageBox.Show("Pedido despachado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _mediator.NotificarDespachoRegistrado(_pedidoSeleccionado.Id);
            await CargarPedidosAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task EntregarAsync()
    {
        if (_pedidoSeleccionado is null)
        {
            MessageBox.Show("Seleccione un pedido.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _pedidoSeleccionado.Entregar();
            await _repo.ActualizarAsync(_pedidoSeleccionado);
            await _auditoria.AgregarAsync(new Auditoria(_sesion.Username, "ENTREGA",
                $"Pedido {_pedidoSeleccionado.Id.ToString("N")[..8].ToUpperInvariant()} entregado"));
            MessageBox.Show("Pedido entregado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _mediator.NotificarDespachoRegistrado(_pedidoSeleccionado.Id);
            await CargarPedidosAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) => CargarPedidosAsync().ConfigureAwait(false);
    public void OnStockModificado() { }
    public void OnCatalogoActualizado() { }
    public void OnDespachoRegistrado(Guid pedidoId) => CargarPedidosAsync().ConfigureAwait(false);
    public void OnDevolucionProcesada(Guid pedidoId) { }
}