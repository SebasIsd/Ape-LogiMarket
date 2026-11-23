using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Visitors;
using LogiMarket.Infrastructure.Catalog;
using LogiMarket.WinForms.Mediator;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public partial class FrmCatalogo : UserControl, IViewRefrescable
{
    private readonly IRepositorioCatalogo _catalogo;
    private readonly IFormularioMediator _mediator;
    private readonly CatalogoVisualFlyweightFactory _flyweight;
    private readonly DataGridView _grid;
    private readonly Button _btnExpandir;
    private readonly Button _btnPeso;
    private readonly TextBox _txtSkuSeleccionado;

    public FrmCatalogo(IRepositorioCatalogo catalogo, IFormularioMediator mediator, CatalogoVisualFlyweightFactory flyweight)
    {
        _catalogo = catalogo;
        _mediator = mediator;
        _flyweight = flyweight;
        _mediator.Registrar(this);

        _grid = new DataGridView
        {
            Dock = DockStyle.Top,
            Height = 300,
            ReadOnly = true,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false
        };

        _txtSkuSeleccionado = new TextBox { ReadOnly = true, Width = 200, Location = new Point(10, 310) };
        _btnExpandir = new Button { Text = "Expandir kit (Iterator)", Location = new Point(220, 307), Width = 180 };
        _btnPeso = new Button { Text = "Calcular peso (Visitor)", Location = new Point(410, 307), Width = 180 };

        _btnExpandir.Click += BtnExpandir_Click;
        _btnPeso.Click += BtnPeso_Click;

        Controls.Add(_grid);
        Controls.Add(_txtSkuSeleccionado);
        Controls.Add(_btnExpandir);
        Controls.Add(_btnPeso);

        Load += async (_, _) => await CargarCatalogoAsync();
    }

    private async Task CargarCatalogoAsync()
    {
        try
        {
            var elementos = await _catalogo.ListarTodoAsync();
            _grid.Rows.Clear();
            _grid.Columns.Clear();
            _grid.Columns.Add("Sku", "SKU");
            _grid.Columns.Add("Nombre", "Nombre");
            _grid.Columns.Add("Tipo", "Tipo");
            _grid.Columns.Add("Precio", "Precio");
            _grid.Columns.Add("Stock", "Stock");
            _grid.Columns.Add("PesoKg", "Peso (kg)");

            foreach (var elem in elementos)
            {
                // Flyweight: compartir metadatos inmutables
                var metadata = _flyweight.Obtener(
                    elem.Sku,
                    elem.Nombre,
                    elem.EsKit,
                    elem is ProductoSimple p ? p.PesoKg : 0m);

                int stock = elem is ProductoSimple ps ? ps.Stock : 0;
                var precio = elem.CalcularPrecioTotal(1);

                _grid.Rows.Add(metadata.Sku, metadata.Nombre, metadata.EsKit ? "KIT" : "Producto",
                    precio.Amount.ToString("0.00"), stock, metadata.PesoKg.ToString("0.00"));
            }

            _grid.SelectionChanged += (_, _) =>
            {
                if (_grid.CurrentRow is not null)
                    _txtSkuSeleccionado.Text = _grid.CurrentRow.Cells["Sku"].Value?.ToString() ?? "";
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error cargando catálogo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnExpandir_Click(object? sender, EventArgs e)
    {
        var sku = _txtSkuSeleccionado.Text.Trim();
        if (string.IsNullOrEmpty(sku)) { MessageBox.Show("Selecciona un SKU."); return; }

        var elem = await _catalogo.ObtenerPorSkuAsync(sku);
        if (elem is null) { MessageBox.Show("SKU no encontrado."); return; }
        if (!elem.EsKit) { MessageBox.Show("Solo se expanden kits."); return; }

        // Iterator propio
        var iterador = new Domain.Iterators.KitPhysicalIterator(elem, 1);
        var sb = new System.Text.StringBuilder($"Expansión de {sku}:\n");
        foreach (var (producto, cantidad) in iterador.GetEnumerator())
            sb.AppendLine($"  {producto.Sku} - {producto.Nombre} x{cantidad}");

        MessageBox.Show(sb.ToString(), "Iterator - Expansión de kit", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void BtnPeso_Click(object? sender, EventArgs e)
    {
        var sku = _txtSkuSeleccionado.Text.Trim();
        if (string.IsNullOrEmpty(sku)) { MessageBox.Show("Selecciona un SKU."); return; }

        var elem = await _catalogo.ObtenerPorSkuAsync(sku);
        if (elem is null) { MessageBox.Show("SKU no encontrado."); return; }

        // Visitor
        var visitor = new DesgloseFisicoVisitor();
        visitor.Calcular(elem, 1);

        var sb = new System.Text.StringBuilder($"Desglose de {sku}:\n");
        foreach (var (s, c) in visitor.Desglose)
            sb.AppendLine($"  {s}: {c} unidades");
        sb.AppendLine($"\nPeso total: {visitor.PesoTotal:0.00} kg");

        MessageBox.Show(sb.ToString(), "Visitor - Desglose físico", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) { }
    public void OnStockModificado() => CargarCatalogoAsync().ConfigureAwait(false);
    public void OnCatalogoActualizado() => CargarCatalogoAsync().ConfigureAwait(false);
    public void OnDespachoRegistrado(Guid pedidoId) { }
    public void OnDevolucionProcesada(Guid pedidoId) { }
}