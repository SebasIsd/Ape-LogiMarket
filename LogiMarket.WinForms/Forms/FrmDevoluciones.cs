using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Application.UseCases.Devoluciones;
using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Enums;
using LogiMarket.WinForms.Mediator;
using LogiMarket.WinForms.UI;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public class FrmDevoluciones : UserControl, IViewRefrescable
{
    // --- Componentes ---
    private ComboBox cmbPedidosEntregados;
    private Button btnCargar;
    private GroupBox grpResumen;
    private Label lbId;
    private Label lbCliente;
    private Label lbTotal;
    private Label lbFechaEntrega;
    private GroupBox grpDevolucion;
    private ComboBox cmbMotivo;
    private Label lbDiasTranscurridos;
    private Label lbReembolso;
    private Button btnProcesar;
    private TableLayoutPanel pnlRaiz;

    private readonly IRepositorioPedido _repo;
    private readonly DevolucionService _servicio;
    private readonly IFormularioMediator _mediator;
    private Pedido? _pedidoSeleccionado;

    public FrmDevoluciones(IRepositorioPedido repo, DevolucionService servicio, IFormularioMediator mediator)
    {
        _repo = repo;
        _servicio = servicio;
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
            RowCount = 2,
            Padding = new Padding(15)
        };
        pnlRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        pnlRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // --- Fila 0: Selector ---
        var pnlSelector = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 10, 0, 5)
        };
        var lbPedido = UIHelper.CrearLabel("Pedido ENTREGADO:", negrita: true);
        lbPedido.Margin = new Padding(0, 8, 5, 0);
        cmbPedidosEntregados = UIHelper.CrearComboBox(430);
        cmbPedidosEntregados.Margin = new Padding(0, 5, 10, 0);
        btnCargar = UIHelper.CrearBotonPrimario("Cargar", 90);
        btnCargar.Click += async (_, _) => await CargarPedidoAsync();
        cmbPedidosEntregados.SelectionChangeCommitted += async (_, _) => await CargarPedidoAsync();

        pnlSelector.Controls.AddRange(new Control[] { lbPedido, cmbPedidosEntregados, btnCargar });
        pnlRaiz.Controls.Add(pnlSelector, 0, 0);
        pnlRaiz.SetColumnSpan(pnlSelector, 2);

        // --- Fila 1, col 0: Resumen ---
        grpResumen = UIHelper.CrearGroupBox("Resumen del pedido");
        grpResumen.Dock = DockStyle.Fill;
        var pnlResumen = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
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

        pnlResumen.Controls.Add(UIHelper.CrearLabel("Entregado:", negrita: true), 0, 3);
        lbFechaEntrega = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteNormal, TextAlign = ContentAlignment.MiddleLeft };
        pnlResumen.Controls.Add(lbFechaEntrega, 1, 3);

        grpResumen.Controls.Add(pnlResumen);
        pnlRaiz.Controls.Add(grpResumen, 0, 1);

        // --- Fila 1, col 1: Devolución (Template Method) ---
        grpDevolucion = UIHelper.CrearGroupBox("Devolución (Template Method)");
        grpDevolucion.Dock = DockStyle.Fill;
        var pnlDevolucion = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(15, 10, 15, 10)
        };
        pnlDevolucion.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        pnlDevolucion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        pnlDevolucion.Controls.Add(UIHelper.CrearLabel("Motivo:", negrita: true), 0, 0);
        cmbMotivo = UIHelper.CrearComboBox();
        cmbMotivo.Items.AddRange(new object[] { "DEFECTO (30 días)", "ARREPENTIMIENTO (7 días)" });
        cmbMotivo.SelectedIndex = 0;
        pnlDevolucion.Controls.Add(cmbMotivo, 1, 0);

        pnlDevolucion.Controls.Add(UIHelper.CrearLabel("Días transcurridos:", negrita: true), 0, 1);
        lbDiasTranscurridos = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteNormal, TextAlign = ContentAlignment.MiddleLeft };
        pnlDevolucion.Controls.Add(lbDiasTranscurridos, 1, 1);

        pnlDevolucion.Controls.Add(UIHelper.CrearLabel("Reembolso:", negrita: true), 0, 2);
        lbReembolso = new Label { Text = "—", Dock = DockStyle.Fill, Font = UIHelper.FuenteGrande, ForeColor = UIHelper.ColorSecundario, TextAlign = ContentAlignment.MiddleLeft };
        pnlDevolucion.Controls.Add(lbReembolso, 1, 2);

        btnProcesar = UIHelper.CrearBotonPeligro("Procesar devolución", 180);
        btnProcesar.Click += async (_, _) => await ProcesarDevolucionAsync();
        pnlDevolucion.Controls.Add(btnProcesar, 1, 3);

        grpDevolucion.Controls.Add(pnlDevolucion);
        pnlRaiz.Controls.Add(grpDevolucion, 1, 1);

        Controls.Add(pnlRaiz);
    }

    private async Task CargarPedidosAsync()
    {
        var pedidos = await _repo.ListarAsync();
        var entregados = pedidos.Where(p => p.EstadoActual == "ENTREGADO" && p.Devolucion is null).ToList();
        cmbPedidosEntregados.DataSource = PedidoItem.De(entregados);
        cmbPedidosEntregados.DisplayMember = "Texto";
        cmbPedidosEntregados.ValueMember = "Id";
        if (cmbPedidosEntregados.SelectedValue is Guid) await CargarPedidoAsync();
        else _pedidoSeleccionado = null;
    }

    private async Task CargarPedidoAsync()
    {
        if (cmbPedidosEntregados.SelectedValue is not Guid id) return;
        _pedidoSeleccionado = await _repo.ObtenerPorIdAsync(id);
        if (_pedidoSeleccionado is null) return;

        lbId.Text = _pedidoSeleccionado.Id.ToString("N")[..8];
        lbCliente.Text = _pedidoSeleccionado.Cliente?.Nombre ?? "—";
        lbTotal.Text = $"$ {_pedidoSeleccionado.Total.Amount:0.00}";
        lbFechaEntrega.Text = _pedidoSeleccionado.FechaEntrega?.ToString("yyyy-MM-dd") ?? "—";

        var dias = (int)(DateTime.UtcNow - (_pedidoSeleccionado.FechaEntrega ?? DateTime.UtcNow)).TotalDays;
        lbDiasTranscurridos.Text = $"{dias} día(s)";
    }

    private async Task ProcesarDevolucionAsync()
    {
        if (_pedidoSeleccionado is null)
        {
            MessageBox.Show("Seleccione un pedido.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var motivo = cmbMotivo.SelectedIndex == 0 ? MotivoDevolucion.Defecto : MotivoDevolucion.Arrepentimiento;

            // Template Method (dentro del servicio): persiste, reintegra stock y cambia el estado del pedido
            var pedidoId = _pedidoSeleccionado.Id;
            var devolucion = await _servicio.ProcesarAsync(pedidoId, motivo);

            if (devolucion.Aceptada)
            {
                lbReembolso.Text = $"$ {devolucion.Reembolso.Amount:0.00}";
                MessageBox.Show($"Devolución aceptada. Reembolso: $ {devolucion.Reembolso.Amount:0.00}\nEl pedido pasó a CANCELADO y el stock fue reintegrado.",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _mediator.NotificarStockModificado();
                _mediator.NotificarDevolucionProcesada(pedidoId);
            }
            else
            {
                lbReembolso.Text = "Rechazada";
                _mediator.NotificarDevolucionProcesada(pedidoId);
                MessageBox.Show("Devolución rechazada según la política (fuera de plazo). Queda registrada.",
                    "Rechazo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            await CargarPedidosAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) { }
    public void OnStockModificado() { }
    public void OnCatalogoActualizado() { }
    public void OnDespachoRegistrado(Guid pedidoId) { }
    public void OnDevolucionProcesada(Guid pedidoId) => CargarPedidosAsync().ConfigureAwait(false);
}