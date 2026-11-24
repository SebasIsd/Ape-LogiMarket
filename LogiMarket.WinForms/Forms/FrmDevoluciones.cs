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
    private readonly IFormularioMediator _mediator;
    private Pedido? _pedidoSeleccionado;

    public FrmDevoluciones(IRepositorioPedido repo, IFormularioMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
        _mediator.Registrar(this);
        InicializarComponentes();
        Load += async (_, _) => await CargarPedidosAsync();
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
        cmbPedidosEntregados = UIHelper.CrearComboBox(300);
        cmbPedidosEntregados.Margin = new Padding(0, 5, 10, 0);
        btnCargar = UIHelper.CrearBotonPrimario("Cargar", 90);
        btnCargar.Click += async (_, _) => await CargarPedidoAsync();

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
        cmbPedidosEntregados.DataSource = entregados;
        cmbPedidosEntregados.DisplayMember = "Id";
        cmbPedidosEntregados.ValueMember = "Id";
    }

    private async Task CargarPedidoAsync()
    {
        if (cmbPedidosEntregados.SelectedItem is not Guid id) return;
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

            // Template Method: secuencia invariante con variantes
            ProcesarDevolucionTemplate handler = motivo switch
            {
                MotivoDevolucion.Defecto => new DevolucionDefectoHandler(
                    p => { /* reintegro simulado */ },
                    async (d, ct) => { /* persistencia simulada */ }),
                MotivoDevolucion.Arrepentimiento => new DevolucionArrepentimientoHandler(
                    p => { /* reintegro simulado */ },
                    async (d, ct) => { /* persistencia simulada */ }),
                _ => throw new ArgumentException("Motivo inválido")
            };

            var devolucion = await handler.EjecutarAsync(_pedidoSeleccionado, motivo, CancellationToken.None);

            if (devolucion.Aceptada)
            {
                lbReembolso.Text = $"$ {devolucion.Reembolso.Amount:0.00}";
                MessageBox.Show($"Devolución aceptada. Reembolso: $ {devolucion.Reembolso.Amount:0.00}",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _mediator.NotificarDevolucionProcesada(_pedidoSeleccionado.Id);
            }
            else
            {
                lbReembolso.Text = "Rechazada";
                MessageBox.Show("Devolución rechazada según la política.",
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