using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Iterators;
using LogiMarket.Domain.Visitors;
using LogiMarket.WinForms.Mediator;
using LogiMarket.WinForms.UI;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public class FrmInventario : UserControl, IViewRefrescable
{
    // --- Componentes ---
    private DataGridView dgvInventario;
    private Button btnExpandir;
    private Button btnPeso;
    private TextBox txtSkuSeleccionado;
    private TableLayoutPanel pnlRaiz;

    private readonly IRepositorioCatalogo _catalogo;
    private readonly IFormularioMediator _mediator;

    public FrmInventario(IRepositorioCatalogo catalogo, IFormularioMediator mediator)
    {
        _catalogo = catalogo;
        _mediator = mediator;
        _mediator.Registrar(this);
        InicializarComponentes();
        Load += async (_, _) => await CargarInventarioAsync();
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
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 80));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

        dgvInventario = UIHelper.CrearDataGridView();
        dgvInventario.Dock = DockStyle.Fill;
        pnlRaiz.Controls.Add(dgvInventario, 0, 0);

        var pnlAcciones = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 10, 0, 0)
        };

        var lbSku = UIHelper.CrearLabel("SKU:", negrita: true);
        lbSku.Margin = new Padding(0, 8, 5, 0);
        txtSkuSeleccionado = UIHelper.CrearTextBox(200);
        txtSkuSeleccionado.ReadOnly = true;
        txtSkuSeleccionado.Margin = new Padding(0, 5, 15, 0);

        btnExpandir = UIHelper.CrearBotonSecundario("🔍 Expandir (Iterator)", 180);
        btnExpandir.Click += BtnExpandir_Click;
        btnPeso = UIHelper.CrearBoton("⚖️ Peso (Visitor)", UIHelper.ColorInfo, ancho: 180);
        btnPeso.Click += BtnPeso_Click;

        pnlAcciones.Controls.AddRange(new Control[] { lbSku, txtSkuSeleccionado, btnExpandir, btnPeso });
        pnlRaiz.Controls.Add(pnlAcciones, 0, 1);

        Controls.Add(pnlRaiz);
    }

    private async Task CargarInventarioAsync()
    {
        try
        {
            var productos = await _catalogo.ListarProductosAsync();
            dgvInventario.DataSource = null;
            dgvInventario.Columns.Clear();
            dgvInventario.Columns.Add("colSku", "SKU");
            dgvInventario.Columns.Add("colNombre", "Nombre");
            dgvInventario.Columns.Add("colStock", "Stock");
            dgvInventario.Columns.Add("colPeso", "Peso (kg)");
            dgvInventario.Columns.Add("colPrecio", "Precio");

            foreach (var p in productos)
            {
                dgvInventario.Rows.Add(p.Sku, p.Nombre, p.Stock, p.PesoKg.ToString("0.00"), p.Precio.Amount.ToString("0.00"));
            }

            dgvInventario.SelectionChanged += (_, _) =>
            {
                if (dgvInventario.CurrentRow is not null)
                    txtSkuSeleccionado.Text = dgvInventario.CurrentRow.Cells["colSku"].Value?.ToString() ?? "";
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnExpandir_Click(object? sender, EventArgs e)
    {
        var sku = txtSkuSeleccionado.Text.Trim();
        if (string.IsNullOrEmpty(sku))
        {
            MessageBox.Show("Seleccione un SKU.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var elem = await _catalogo.ObtenerPorSkuAsync(sku);
        if (elem is null || !elem.EsKit)
        {
            MessageBox.Show("Solo se expanden kits.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Iterator propio
        var iterador = new KitPhysicalIterator(elem, 1);
        var sb = new System.Text.StringBuilder($"Expansión de {sku}:\n\n");
        foreach (var (producto, cantidad) in iterador.GetEnumerator())
            sb.AppendLine($"  • {producto.Sku} - {producto.Nombre} × {cantidad}");

        MessageBox.Show(sb.ToString(), "Iterator", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void BtnPeso_Click(object? sender, EventArgs e)
    {
        var sku = txtSkuSeleccionado.Text.Trim();
        if (string.IsNullOrEmpty(sku))
        {
            MessageBox.Show("Seleccione un SKU.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var elem = await _catalogo.ObtenerPorSkuAsync(sku);
        if (elem is null)
        {
            MessageBox.Show("SKU no encontrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Visitor
        var visitor = new DesgloseFisicoVisitor();
        visitor.Calcular(elem, 1);

        var sb = new System.Text.StringBuilder($"Desglose de {sku}:\n\n");
        foreach (var (s, c) in visitor.Desglose)
            sb.AppendLine($"  • {s}: {c} unidades");
        sb.AppendLine($"\n⚖️ Peso total: {visitor.PesoTotal:0.00} kg");

        MessageBox.Show(sb.ToString(), "Visitor", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) => CargarInventarioAsync().ConfigureAwait(false);
    public void OnStockModificado() => CargarInventarioAsync().ConfigureAwait(false);
    public void OnCatalogoActualizado() => CargarInventarioAsync().ConfigureAwait(false);
    public void OnDespachoRegistrado(Guid pedidoId) { }
    public void OnDevolucionProcesada(Guid pedidoId) { }
}