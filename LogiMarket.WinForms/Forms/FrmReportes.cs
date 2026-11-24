using LogiMarket.Application.Ports.Documents;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;
using LogiMarket.Infrastructure.Documents;
using LogiMarket.WinForms.Mediator;
using LogiMarket.WinForms.UI;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public class FrmReportes : UserControl, IViewRefrescable
{
    // --- Componentes ---
    private ComboBox cmbPedidos;
    private Button btnCargar;
    private ComboBox cmbTipoComprobante;
    private ComboBox cmbSalida;
    private Button btnGenerar;
    private TextBox txtVistaPrevia;
    private TableLayoutPanel pnlRaiz;

    private readonly IRepositorioPedido _repo;
    private readonly IEnumerable<ISalidaDocumento> _salidas;
    private readonly IFormularioMediator _mediator;
    private Pedido? _pedidoSeleccionado;

    public FrmReportes(IRepositorioPedido repo, IEnumerable<ISalidaDocumento> salidas, IFormularioMediator mediator)
    {
        _repo = repo;
        _salidas = salidas;
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
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(15)
        };
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // --- Fila 0: Selector de pedido ---
        var pnlSelector = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 10, 0, 5)
        };
        var lbPedido = UIHelper.CrearLabel("Pedido:", negrita: true);
        lbPedido.Margin = new Padding(0, 8, 5, 0);
        cmbPedidos = UIHelper.CrearComboBox(300);
        cmbPedidos.Margin = new Padding(0, 5, 10, 0);
        btnCargar = UIHelper.CrearBotonPrimario("Cargar", 90);
        btnCargar.Click += async (_, _) => await CargarPedidoAsync();

        pnlSelector.Controls.AddRange(new Control[] { lbPedido, cmbPedidos, btnCargar });
        pnlRaiz.Controls.Add(pnlSelector, 0, 0);

        // --- Fila 1: Tipo + Salida + Generar ---
        var pnlOpciones = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 5, 0, 5)
        };
        var lbTipo = UIHelper.CrearLabel("Comprobante:", negrita: true);
        lbTipo.Margin = new Padding(0, 8, 5, 0);
        cmbTipoComprobante = UIHelper.CrearComboBox(200);
        cmbTipoComprobante.Items.AddRange(new object[] { "Venta", "Devolución" });
        cmbTipoComprobante.SelectedIndex = 0;
        cmbTipoComprobante.Margin = new Padding(0, 5, 15, 0);

        var lbSalida = UIHelper.CrearLabel("Salida:", negrita: true);
        lbSalida.Margin = new Padding(0, 8, 5, 0);
        cmbSalida = UIHelper.CrearComboBox(200);
        cmbSalida.Items.AddRange(new object[] { "VistaPrevia", "ArchivoTXT" });
        cmbSalida.SelectedIndex = 0;
        cmbSalida.Margin = new Padding(0, 5, 15, 0);

        btnGenerar = UIHelper.CrearBotonSecundario("Generar", 120);
        btnGenerar.Click += async (_, _) => await GenerarComprobanteAsync();

        pnlOpciones.Controls.AddRange(new Control[] { lbTipo, cmbTipoComprobante, lbSalida, cmbSalida, btnGenerar });
        pnlRaiz.Controls.Add(pnlOpciones, 0, 1);

        // --- Fila 2: Vista previa ---
        txtVistaPrevia = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            Font = UIHelper.FuenteMonospace,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        pnlRaiz.Controls.Add(txtVistaPrevia, 0, 2);

        Controls.Add(pnlRaiz);
    }

    private async Task CargarPedidosAsync()
    {
        var pedidos = await _repo.ListarAsync();
        cmbPedidos.DataSource = pedidos;
        cmbPedidos.DisplayMember = "Id";
        cmbPedidos.ValueMember = "Id";
    }

    private async Task CargarPedidoAsync()
    {
        if (cmbPedidos.SelectedItem is not Guid id) return;
        _pedidoSeleccionado = await _repo.ObtenerPorIdAsync(id);
    }

    private async Task GenerarComprobanteAsync()
    {
        if (_pedidoSeleccionado is null)
        {
            MessageBox.Show("Seleccione un pedido.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            // Bridge: abstracción (Comprobante) × implementación (Salida)
            IComprobante comprobante = cmbTipoComprobante.SelectedIndex == 0
                ? new ComprobanteVenta(_pedidoSeleccionado)
                : new ComprobanteDevolucion(_pedidoSeleccionado, _pedidoSeleccionado.Devolucion!);

            var contenido = comprobante.GenerarContenido();

            var salidaNombre = cmbSalida.SelectedItem?.ToString() ?? "VistaPrevia";
            var salida = _salidas.First(s => s.NombreSalida == salidaNombre);
            await salida.EmitirAsync(contenido);

            if (salida is VistaPreviaSalida vista)
                txtVistaPrevia.Text = vista.UltimoContenido ?? "Sin contenido";
            else
                txtVistaPrevia.Text = $"Archivo generado en: {Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\LogiMarket360\\Comprobantes";
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
    public void OnDevolucionProcesada(Guid pedidoId) { }
}