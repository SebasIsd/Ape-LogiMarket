using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;
using LogiMarket.Infrastructure.Outbox;
using LogiMarket.WinForms.Mediator;
using LogiMarket.WinForms.UI;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public class FrmAuditoria : UserControl, IViewRefrescable
{
    // --- Componentes ---
    private DataGridView dgvOutbox;
    private DataGridView dgvAuditoria;
    private Button btnProcesar;
    private TableLayoutPanel pnlRaiz;

    private readonly IRepositorioOutbox _outbox;
    private readonly IRepositorioAuditoria _auditoria;
    private readonly IFormularioMediator _mediator;
    private readonly OutboxProcessor _processor;

    public FrmAuditoria(IRepositorioOutbox outbox, IRepositorioAuditoria auditoria, IFormularioMediator mediator, OutboxProcessor processor)
    {
        _processor = processor;
        _outbox = outbox;
        _auditoria = auditoria;
        _mediator = mediator;
        _mediator.Registrar(this);
        InicializarComponentes();
        Load += async (_, _) => await CargarDatosAsync();
        VisibleChanged += async (_, _) => { if (Visible) await CargarDatosAsync(); };
    }

    private void InicializarComponentes()
    {
        BackColor = UIHelper.ColorGrisClaro;

        pnlRaiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(15)
        };
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        // --- Fila 0: Botón procesar ---
        var pnlBoton = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(0, 4, 0, 8),
            Margin = new Padding(0)
        };

        btnProcesar = UIHelper.CrearBotonSecundario("⚙️ Procesar Outbox pendiente", 210);
        btnProcesar.Height = 34;
        btnProcesar.Margin = new Padding(0, 2, 12, 2);
        btnProcesar.Click += async (_, _) => await ProcesarOutboxAsync();

        var lbInfo = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(820, 0),
            Margin = new Padding(4, 8, 0, 2),
            Font = UIHelper.FuenteNormal,
            ForeColor = UIHelper.ColorTextoClaro,
            Text = "Outbox: eventos de negocio (p. ej. PedidoPagado) pendientes de notificar. " +
                   "Auditoría: bitácora de quién hizo qué y cuándo."
        };

        pnlBoton.Controls.Add(btnProcesar);
        pnlBoton.Controls.Add(lbInfo);
        pnlRaiz.Controls.Add(pnlBoton, 0, 0);

        // --- Fila 1: Outbox ---
        var grpOutbox = UIHelper.CrearGroupBox("Eventos Outbox (Observer) — últimos 100");
        grpOutbox.Dock = DockStyle.Fill;
        grpOutbox.Margin = new Padding(0, 0, 0, 10);
        grpOutbox.Padding = new Padding(10, 18, 10, 10);

        dgvOutbox = UIHelper.CrearDataGridView();
        dgvOutbox.Dock = DockStyle.Fill;
        dgvOutbox.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        var cOId = dgvOutbox.Columns.Add("colId", "ID");
        var cOTipo = dgvOutbox.Columns.Add("colTipo", "Tipo");
        var cOCreado = dgvOutbox.Columns.Add("colCreado", "Creado");
        var cOProc = dgvOutbox.Columns.Add("colProcesado", "Procesado");
        var cOReit = dgvOutbox.Columns.Add("colReintentos", "Reintentos");
        dgvOutbox.Columns[cOId].FillWeight = 16;
        dgvOutbox.Columns[cOTipo].FillWeight = 34;
        dgvOutbox.Columns[cOCreado].FillWeight = 22;
        dgvOutbox.Columns[cOProc].FillWeight = 14;
        dgvOutbox.Columns[cOReit].FillWeight = 14;

        grpOutbox.Controls.Add(dgvOutbox);
        pnlRaiz.Controls.Add(grpOutbox, 0, 1);

        // --- Fila 2: Auditoría ---
        var grpAuditoria = UIHelper.CrearGroupBox("Auditoría");
        grpAuditoria.Dock = DockStyle.Fill;
        grpAuditoria.Margin = new Padding(0);
        grpAuditoria.Padding = new Padding(10, 18, 10, 10);

        dgvAuditoria = UIHelper.CrearDataGridView();
        dgvAuditoria.Dock = DockStyle.Fill;
        dgvAuditoria.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        var cAId = dgvAuditoria.Columns.Add("colId", "ID");
        var cAUsu = dgvAuditoria.Columns.Add("colUsuario", "Usuario");
        var cAAcc = dgvAuditoria.Columns.Add("colAccion", "Acción");
        var cADet = dgvAuditoria.Columns.Add("colDetalle", "Detalle");
        var cAFec = dgvAuditoria.Columns.Add("colFecha", "Fecha");
        dgvAuditoria.Columns[cAId].FillWeight = 14;
        dgvAuditoria.Columns[cAUsu].FillWeight = 16;
        dgvAuditoria.Columns[cAAcc].FillWeight = 18;
        dgvAuditoria.Columns[cADet].FillWeight = 34;
        dgvAuditoria.Columns[cAFec].FillWeight = 18;

        grpAuditoria.Controls.Add(dgvAuditoria);
        pnlRaiz.Controls.Add(grpAuditoria, 0, 2);

        Controls.Add(pnlRaiz);
    }

    private async Task CargarDatosAsync()
    {
        try
        {
            var eventos = await _outbox.ListarRecientesAsync(100);
            dgvOutbox.DataSource = null;
            dgvOutbox.Rows.Clear();
            foreach (var e in eventos)
            {
                dgvOutbox.Rows.Add(e.Id.ToString("N")[..8], e.TipoEvento, e.CreadoEn.ToString("yyyy-MM-dd HH:mm"),
                    e.Procesado ? "Sí" : "No", e.Reintentos);
            }

            var auditorias = await _auditoria.ListarUltimosAsync(100);
            dgvAuditoria.DataSource = null;
            dgvAuditoria.Rows.Clear();
            foreach (var a in auditorias)
            {
                dgvAuditoria.Rows.Add(a.Id.ToString("N")[..8], a.Usuario, a.Accion, a.Detalle ?? "—",
                    a.Fecha.ToString("yyyy-MM-dd HH:mm"));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ProcesarOutboxAsync()
    {
        try
        {
            var antes = (await _outbox.ObtenerPendientesAsync(100)).Count;
            if (antes == 0)
            {
                MessageBox.Show("No hay eventos pendientes. Los eventos se generan al cobrar un pedido.",
                    "Outbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // OutboxProcessor: envía cada evento por el notificador y lo marca como procesado
            await _processor.ProcesarPendientesAsync();

            var despues = (await _outbox.ObtenerPendientesAsync(100)).Count;
            MessageBox.Show($"Procesados {antes - despues} de {antes} eventos.", "Outbox",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await CargarDatosAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) => _ = CargarDatosAsync();
    public void OnStockModificado() { }
    public void OnCatalogoActualizado() { }
    public void OnDespachoRegistrado(Guid pedidoId) => _ = CargarDatosAsync();
    public void OnDevolucionProcesada(Guid pedidoId) => _ = CargarDatosAsync();
}