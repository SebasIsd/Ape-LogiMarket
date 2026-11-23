using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Visitors;
using LogiMarket.Infrastructure.Catalog;
using LogiMarket.WinForms.Mediator;
using LogiMarket.WinForms.UI;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public partial class FrmCatalogo : UserControl, IViewRefrescable
{
    // --- Componentes ---
    private DataGridView dgvCatalogo;
    private Button btnExpandir;
    private Button btnPeso;
    private TextBox txtSkuSeleccionado;
    private Label lbSku;
    private TableLayoutPanel pnlRaiz;
    private Panel pnlAcciones;

    private readonly IRepositorioCatalogo _catalogo;
    private readonly IFormularioMediator _mediator;
    private readonly CatalogoVisualFlyweightFactory _flyweight;

    public FrmCatalogo(IRepositorioCatalogo catalogo, IFormularioMediator mediator, CatalogoVisualFlyweightFactory flyweight)
    {
        _catalogo = catalogo;
        _mediator = mediator;
        _flyweight = flyweight;
        _mediator.Registrar(this);

        InicializarComponentes();
        Load += async (_, _) => await CargarCatalogoAsync();
    }

    private void InicializarComponentes()
    {
        BackColor = UIHelper.ColorGrisClaro;

        // --- Layout principal: grid arriba (70%), acciones abajo (30%) ---
        pnlRaiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(15)
        };
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

        // --- Grid ---
        dgvCatalogo = UIHelper.CrearDataGridView();
        dgvCatalogo.Dock = DockStyle.Fill;
        pnlRaiz.Controls.Add(dgvCatalogo, 0, 0);

        // --- Panel de acciones ---
        pnlAcciones = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 10, 0, 0)
        };

        lbSku = UIHelper.CrearLabel("SKU seleccionado:", negrita: true);
        lbSku.Location = new Point(0, 5);

        txtSkuSeleccionado = UIHelper.CrearTextBox(200);
        txtSkuSeleccionado.ReadOnly = true;
        txtSkuSeleccionado.Location = new Point(130, 3);

        btnExpandir = UIHelper.CrearBotonSecundario("🔍 Expandir kit (Iterator)", 200);
        btnExpandir.Location = new Point(340, 0);
        btnExpandir.Click += BtnExpandir_Click;

        btnPeso = UIHelper.CrearBoton("⚖️ Calcular peso (Visitor)", UIHelper.ColorInfo, ancho: 200);
        btnPeso.Location = new Point(550, 0);
        btnPeso.Click += BtnPeso_Click;

        pnlAcciones.Controls.AddRange(new Control[] { lbSku, txtSkuSeleccionado, btnExpandir, btnPeso });
        pnlRaiz.Controls.Add(pnlAcciones, 0, 1);

        Controls.Add(pnlRaiz);
    }

    private async Task CargarCatalogoAsync()
    {
        try
        {
            var elementos = await _catalogo.ListarTodoAsync();
            dgvCatalogo.Rows.Clear();
            dgvCatalogo.Columns.Clear();
            dgvCatalogo.Columns.Add("colSku", "SKU");
            dgvCatalogo.Columns.Add("colNombre", "Nombre");
            dgvCatalogo.Columns.Add("colTipo", "Tipo");
            dgvCatalogo.Columns.Add("colPrecio", "Precio");
            dgvCatalogo.Columns.Add("colStock", "Stock");
            dgvCatalogo.Columns.Add("colPeso", "Peso (kg)");

            foreach (var elem in elementos)
            {
                var metadata = _flyweight.Obtener(
                    elem.Sku, elem.Nombre, elem.EsKit,
                    elem is ProductoSimple p ? p.PesoKg : 0m);

                int stock = elem is ProductoSimple ps ? ps.Stock : 0;
                var precio = elem.CalcularPrecioTotal(1);

                dgvCatalogo.Rows.Add(
                    metadata.Sku, metadata.Nombre, metadata.EsKit ? "KIT" : "Producto",
                    precio.Amount.ToString("0.00"), stock, metadata.PesoKg.ToString("0.00"));
            }

            dgvCatalogo.SelectionChanged += (_, _) =>
            {
                if (dgvCatalogo.CurrentRow is not null)
                    txtSkuSeleccionado.Text = dgvCatalogo.CurrentRow.Cells["colSku"].Value?.ToString() ?? "";
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error cargando catálogo: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnExpandir_Click(object? sender, EventArgs e)
    {
        var sku = txtSkuSeleccionado.Text.Trim();
        if (string.IsNullOrEmpty(sku))
        {
            MessageBox.Show("Selecciona un SKU de la tabla.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var elem = await _catalogo.ObtenerPorSkuAsync(sku);
        if (elem is null)
        {
            MessageBox.Show("SKU no encontrado.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (!elem.EsKit)
        {
            MessageBox.Show("Solo se pueden expandir kits.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var iterador = new Domain.Iterators.KitPhysicalIterator(elem, 1);
        var sb = new System.Text.StringBuilder($"Expansión de {sku}:\n\n");
        foreach (var (producto, cantidad) in iterador.GetEnumerator())
            sb.AppendLine($"  • {producto.Sku} - {producto.Nombre} × {cantidad}");

        MessageBox.Show(sb.ToString(), "Iterator - Expansión de kit",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void BtnPeso_Click(object? sender, EventArgs e)
    {
        var sku = txtSkuSeleccionado.Text.Trim();
        if (string.IsNullOrEmpty(sku))
        {
            MessageBox.Show("Selecciona un SKU de la tabla.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var elem = await _catalogo.ObtenerPorSkuAsync(sku);
        if (elem is null)
        {
            MessageBox.Show("SKU no encontrado.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var visitor = new DesgloseFisicoVisitor();
        visitor.Calcular(elem, 1);

        var sb = new System.Text.StringBuilder($"Desglose físico de {sku}:\n\n");
        foreach (var (s, c) in visitor.Desglose)
            sb.AppendLine($"  • {s}: {c} unidades");
        sb.AppendLine($"\n⚖️ Peso total: {visitor.PesoTotal:0.00} kg");

        MessageBox.Show(sb.ToString(), "Visitor - Desglose físico",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) { }
    public void OnStockModificado() => CargarCatalogoAsync().ConfigureAwait(false);
    public void OnCatalogoActualizado() => CargarCatalogoAsync().ConfigureAwait(false);
    public void OnDespachoRegistrado(Guid pedidoId) { }
    public void OnDevolucionProcesada(Guid pedidoId) { }
}