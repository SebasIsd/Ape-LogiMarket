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
    private Label lbTitulo;
    private Label lbSubtitulo;
    private Label lbBuscar;
    private TextBox txtBuscar;
    private Button btnNuevoProducto;
    private Button btnNuevoKit;
    private DataGridView dgvCatalogo;
    private GroupBox grpEstructura;
    private TreeView tvEstructura;
    private Button btnGuardarComposicion;
    private TableLayoutPanel pnlRaiz;

    private readonly IRepositorioCatalogo _catalogo;
    private readonly IFormularioMediator _mediator;
    private readonly CatalogoVisualFlyweightFactory _flyweight;
    private IElementoCatalogo? _elementoSeleccionado;

    public FrmCatalogo(IRepositorioCatalogo catalogo, IFormularioMediator mediator, CatalogoVisualFlyweightFactory flyweight)
    {
        _catalogo = catalogo;
        _mediator = mediator;
        _flyweight = flyweight;
        _mediator.Registrar(this);

        InicializarComponentes();
        Load += async (_, _) => await CargarCatalogoAsync();
        VisibleChanged += async (_, _) => { if (Visible) await CargarCatalogoAsync(); };
    }

    private void InicializarComponentes()
    {
        BackColor = Color.FromArgb(245, 247, 250);

        // --- Título ---
        lbTitulo = new Label
        {
            Text = "Catálogo y kits",
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 60, 90),
            Location = new Point(0, 0),
            AutoSize = true
        };

        lbSubtitulo = new Label
        {
            Text = "Productos individuales y kits compuestos sin ciclos",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(120, 120, 120),
            Location = new Point(0, 35),
            AutoSize = true
        };

        // --- Sección Buscar producto ---
        lbBuscar = new Label
        {
            Text = "Buscar producto",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(50, 50, 50),
            Location = new Point(0, 70),
            AutoSize = true
        };

        txtBuscar = new TextBox
        {
            Location = new Point(0, 95),
            Width = 400,
            Height = 32,
            Font = new Font("Segoe UI", 10),
            PlaceholderText = "SKU, nombre o categoría"
        };
        txtBuscar.TextChanged += async (_, _) => await CargarCatalogoAsync();

        // --- Botones Nuevo producto / Nuevo kit ---
        btnNuevoProducto = new Button
        {
            Text = "Nuevo producto",
            Location = new Point(420, 93),
            Width = 140,
            Height = 34,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(50, 50, 50),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9)
        };
        btnNuevoProducto.FlatAppearance.BorderSize = 1;
        btnNuevoProducto.Click += BtnNuevoProducto_Click;

        btnNuevoKit = new Button
        {
            Text = "Nuevo kit",
            Location = new Point(570, 93),
            Width = 120,
            Height = 34,
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnNuevoKit.FlatAppearance.BorderSize = 0;
        btnNuevoKit.Click += BtnNuevoKit_Click;

        // --- Tabla de catálogo ---
        dgvCatalogo = UIHelper.CrearDataGridView();
        dgvCatalogo.Location = new Point(0, 140);
        dgvCatalogo.Size = new Size(700, 350);
        dgvCatalogo.Columns.Add("colSku", "SKU");
        dgvCatalogo.Columns.Add("colProducto", "Producto");
        dgvCatalogo.Columns.Add("colPrecio", "Precio");
        dgvCatalogo.Columns.Add("colStock", "Stock");
        dgvCatalogo.SelectionChanged += DgvCatalogo_SelectionChanged;

        // --- Panel lateral "Estructura del kit" ---
        grpEstructura = UIHelper.CrearGroupBox("Estructura del kit");
        grpEstructura.Location = new Point(720, 140);
        grpEstructura.Size = new Size(280, 350);

        tvEstructura = new TreeView
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10),
            BackColor = Color.White,
            BorderStyle = BorderStyle.None,
            ShowLines = true,
            ShowPlusMinus = true
        };
        grpEstructura.Controls.Add(tvEstructura);

        // --- Botón Guardar composición ---
        btnGuardarComposicion = new Button
        {
            Text = "Guardar composición",
            Location = new Point(720, 500),
            Width = 280,
            Height = 40,
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnGuardarComposicion.FlatAppearance.BorderSize = 0;
        btnGuardarComposicion.Click += BtnGuardarComposicion_Click;

        // --- Agregar todo ---
        Controls.Add(lbTitulo);
        Controls.Add(lbSubtitulo);
        Controls.Add(lbBuscar);
        Controls.Add(txtBuscar);
        Controls.Add(btnNuevoProducto);
        Controls.Add(btnNuevoKit);
        Controls.Add(dgvCatalogo);
        Controls.Add(grpEstructura);
        Controls.Add(btnGuardarComposicion);

        Resize += (_, _) => AjustarLayout();
    }

    private void AjustarLayout()
    {
        int ancho = ClientSize.Width - 40;
        dgvCatalogo.Size = new Size(ancho - 300, 350);
        grpEstructura.Location = new Point(ancho - 280, 140);
        btnGuardarComposicion.Location = new Point(ancho - 280, 500);
    }

    private async Task CargarCatalogoAsync()
    {
        try
        {
            var elementos = await _catalogo.ListarTodoAsync();
            var filtro = txtBuscar.Text.Trim().ToLower();

            dgvCatalogo.Rows.Clear();

            foreach (var elem in elementos)
            {
                // Aplicar filtro
                if (!string.IsNullOrEmpty(filtro) &&
                    !elem.Sku.ToLower().Contains(filtro) &&
                    !elem.Nombre.ToLower().Contains(filtro))
                    continue;

                var metadata = _flyweight.Obtener(
                    elem.Sku, elem.Nombre, elem.EsKit,
                    elem is ProductoSimple p ? p.PesoKg : 0m);

                int stock = elem is ProductoSimple ps ? ps.Stock : 0;
                var precio = elem.CalcularPrecioTotal(1);
                var stockTexto = elem.EsKit ? "—" : stock.ToString();

                dgvCatalogo.Rows.Add(
                    metadata.Sku,
                    metadata.Nombre,
                    $"USD {precio.Amount:0.00}",
                    stockTexto);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error cargando catálogo: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void DgvCatalogo_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvCatalogo.CurrentRow is null) return;

        var sku = dgvCatalogo.CurrentRow.Cells["colSku"].Value?.ToString();
        if (string.IsNullOrEmpty(sku)) return;

        _elementoSeleccionado = await _catalogo.ObtenerPorSkuAsync(sku);
        if (_elementoSeleccionado is null) return;

        // Mostrar estructura del kit en el TreeView
        tvEstructura.Nodes.Clear();

        if (_elementoSeleccionado.EsKit && _elementoSeleccionado is KitCompuesto kit)
        {
            grpEstructura.Text = $"Estructura del kit\n{kit.Sku}";
            var nodoRaiz = tvEstructura.Nodes.Add(kit.Sku);
            nodoRaiz.NodeFont = new Font("Segoe UI", 10, FontStyle.Bold);
            CargarNodosKit(nodoRaiz, kit);
            tvEstructura.ExpandAll();
        }
        else
        {
            grpEstructura.Text = "Estructura del kit";
            tvEstructura.Nodes.Add("Seleccione un kit para ver su estructura");
        }
    }

    private void CargarNodosKit(TreeNode nodoPadre, KitCompuesto kit)
    {
        foreach (var componente in kit.Componentes)
        {
            if (componente.Elemento is KitCompuesto kitHijo)
            {
                var nodoHijo = nodoPadre.Nodes.Add($"- {kitHijo.Sku}");
                nodoHijo.NodeFont = new Font("Segoe UI", 9, FontStyle.Bold);
                CargarNodosKit(nodoHijo, kitHijo);
            }
            else if (componente.Elemento is ProductoSimple producto)
            {
                nodoPadre.Nodes.Add($"- {producto.Sku} · {producto.Nombre}");
            }
        }
    }

    private async void BtnNuevoProducto_Click(object? sender, EventArgs e)
    {
        using var form = new FrmNuevoProducto(_catalogo);
        if (form.ShowDialog(this) == DialogResult.OK && form.ProductoGuardado)
        {
            await CargarCatalogoAsync();
            _mediator.NotificarCatalogoActualizado();
        }
    }

    private async void BtnNuevoKit_Click(object? sender, EventArgs e)
    {
        using var form = new FrmNuevoKit(_catalogo);
        if (form.ShowDialog(this) == DialogResult.OK && form.KitGuardado)
        {
            await CargarCatalogoAsync();
            _mediator.NotificarCatalogoActualizado();
        }
    }

    private async void BtnGuardarComposicion_Click(object? sender, EventArgs e)
    {
        if (_elementoSeleccionado is null)
        {
            MessageBox.Show("Seleccione un kit de la tabla.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!_elementoSeleccionado.EsKit || _elementoSeleccionado is not KitCompuesto kit)
        {
            MessageBox.Show("Solo se pueden editar kits.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var form = new FrmNuevoKit(_catalogo, kit);
        if (form.ShowDialog(this) == DialogResult.OK && form.KitGuardado)
        {
            await CargarCatalogoAsync();
            _mediator.NotificarCatalogoActualizado();
        }
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) { }
    public void OnStockModificado() => CargarCatalogoAsync().ConfigureAwait(false);
    public void OnCatalogoActualizado() => CargarCatalogoAsync().ConfigureAwait(false);
    public void OnDespachoRegistrado(Guid pedidoId) { }
    public void OnDevolucionProcesada(Guid pedidoId) { }
}