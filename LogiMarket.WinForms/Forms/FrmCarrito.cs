using LogiMarket.Application.Discount;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Application.Shipping;
using LogiMarket.Domain.Builders;
using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.ValueObjects;
using LogiMarket.WinForms.Mediator;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public class FrmCarrito : UserControl, IViewRefrescable
{
    // --- Componentes ---
    private Label lbCliente;
    private ComboBox cmbClientes;
    private Label lbProducto;
    private ComboBox cmbProductos;
    private Label lbCantidad;
    private NumericUpDown nudCantidad;
    private Button btnAgregar;
    private Button btnQuitar;
    private Button btnDeshacer;
    private Button btnRehacer;
    private Button btnClonar;
    private Button btnValidar;
    private Button btnLimpiar;
    private DataGridView dgvCarrito;
    private GroupBox grpTotales;
    private Label lbSubtotal;
    private Label lbDescuento;
    private Label lbTarifa;
    private Label lbTotal;
    private Label lbEstrategia;
    private Label lbEnvio;
    private ComboBox cmbEnvio;
    private TableLayoutPanel pnlRaiz;

    // --- Estado ---
    private readonly IRepositorioCatalogo _catalogo;
    private readonly IRepositorioCliente _repoClientes;
    private readonly CalculadoraDescuentoContext _descuentos;
    private readonly IEnvioFactory _envioFactory;
    private readonly IFormularioMediator _mediator;

    private Pedido? _pedidoActual;
    private readonly Stack<Pedido> _pilaDeshacer = new();
    private readonly Stack<Pedido> _pilaRehacer = new();
    private const int MAX_HISTORIAL = 5;

    public FrmCarrito(
        IRepositorioCatalogo catalogo,
        IRepositorioCliente repoClientes,
        CalculadoraDescuentoContext descuentos,
        IEnvioFactory envioFactory,
        IFormularioMediator mediator)
    {
        _catalogo = catalogo;
        _repoClientes = repoClientes;
        _descuentos = descuentos;
        _envioFactory = envioFactory;
        _mediator = mediator;
        _mediator.Registrar(this);

        InicializarComponentes();
        Load += CargarCombos;
    }

    private void InicializarComponentes()
    {
        BackColor = Color.FromArgb(245, 247, 250);

        pnlRaiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(15)
        };
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // Barra superior
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Grid
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));  // Totales + botones

        // --- Barra superior: cliente + producto + cantidad + botones ---
        var pnlSuperior = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 5, 0, 5)
        };

        lbCliente = new Label { Text = "Cliente:", AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), Margin = new Padding(0, 8, 5, 0) };
        cmbClientes = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };

        lbProducto = new Label { Text = "  Producto/Kit:", AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), Margin = new Padding(10, 8, 5, 0) };
        cmbProductos = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };

        lbCantidad = new Label { Text = "  Cantidad:", AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), Margin = new Padding(10, 8, 5, 0) };
        nudCantidad = new NumericUpDown { Width = 60, Minimum = 1, Maximum = 999, Value = 1, Font = new Font("Segoe UI", 10) };

        btnAgregar = new Button { Text = "Agregar", Width = 80, Height = 30, BackColor = Color.FromArgb(40, 160, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        btnAgregar.FlatAppearance.BorderSize = 0;
        btnAgregar.Click += async (_, _) => await AgregarLineaAsync();

        btnQuitar = new Button { Text = "Quitar", Width = 70, Height = 30, BackColor = Color.FromArgb(200, 60, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };
        btnQuitar.FlatAppearance.BorderSize = 0;
        btnQuitar.Click += QuitarLinea;

        btnDeshacer = new Button { Text = "↶ Deshacer", Width = 90, Height = 30, BackColor = Color.FromArgb(100, 100, 100), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };
        btnDeshacer.FlatAppearance.BorderSize = 0;
        btnDeshacer.Click += (_, _) => Deshacer();

        btnRehacer = new Button { Text = "↷ Rehacer", Width = 90, Height = 30, BackColor = Color.FromArgb(100, 100, 100), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };
        btnRehacer.FlatAppearance.BorderSize = 0;
        btnRehacer.Click += (_, _) => Rehacer();

        btnClonar = new Button { Text = "⎘ Clonar", Width = 80, Height = 30, BackColor = Color.FromArgb(120, 80, 180), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };
        btnClonar.FlatAppearance.BorderSize = 0;
        btnClonar.Click += (_, _) => ClonarBorrador();

        btnLimpiar = new Button { Text = "✕ Limpiar", Width = 80, Height = 30, BackColor = Color.FromArgb(180, 180, 180), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };
        btnLimpiar.FlatAppearance.BorderSize = 0;
        btnLimpiar.Click += (_, _) => { _pedidoActual = null; _pilaDeshacer.Clear(); _pilaRehacer.Clear(); RefrescarGrid(); };

        pnlSuperior.Controls.AddRange(new Control[] { lbCliente, cmbClientes, lbProducto, cmbProductos, lbCantidad, nudCantidad, btnAgregar, btnQuitar, btnDeshacer, btnRehacer, btnClonar, btnLimpiar });
        pnlRaiz.Controls.Add(pnlSuperior, 0, 0);

        // --- Grid del carrito ---
        dgvCarrito = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        dgvCarrito.Columns.Add("colSku", "SKU");
        dgvCarrito.Columns.Add("colNombre", "Nombre");
        dgvCarrito.Columns.Add("colTipo", "Tipo");
        dgvCarrito.Columns.Add("colCantidad", "Cant.");
        dgvCarrito.Columns.Add("colPrecioUnitario", "Precio Unit.");
        dgvCarrito.Columns.Add("colSubtotal", "Subtotal");
        dgvCarrito.Columns["colPrecioUnitario"]!.DefaultCellStyle.Format = "0.00";
        dgvCarrito.Columns["colSubtotal"]!.DefaultCellStyle.Format = "0.00";
        pnlRaiz.Controls.Add(dgvCarrito, 0, 1);

        // --- Panel inferior: Totales + envío + validar ---
        var pnlInferior = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0, 10, 0, 0)
        };
        pnlInferior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        pnlInferior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        grpTotales = new GroupBox
        {
            Text = "Resumen",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 60, 90)
        };

        var pnlTotales = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(10)
        };
        pnlTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        pnlTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        lbSubtotal = new Label { Text = "Subtotal: $ 0.00", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
        lbDescuento = new Label { Text = "Descuento: $ 0.00", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
        lbEstrategia = new Label { Text = "Estrategia: NORMAL", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
        lbTarifa = new Label { Text = "Tarifa: $ 0.00", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
        lbTotal = new Label { Text = "TOTAL: $ 0.00", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(30, 120, 200) };

        lbEnvio = new Label { Text = "Envío:", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
        cmbEnvio = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
        cmbEnvio.Items.AddRange(new object[] { "ESTÁNDAR", "EXPRÉS" });
        cmbEnvio.SelectedIndex = 0;
        cmbEnvio.SelectedIndexChanged += (_, _) => Recalcular();

        pnlTotales.Controls.Add(lbSubtotal, 0, 0);
        pnlTotales.SetColumnSpan(lbSubtotal, 2);
        pnlTotales.Controls.Add(lbDescuento, 0, 1);
        pnlTotales.SetColumnSpan(lbDescuento, 2);
        pnlTotales.Controls.Add(lbEstrategia, 0, 2);
        pnlTotales.SetColumnSpan(lbEstrategia, 2);
        pnlTotales.Controls.Add(lbTarifa, 0, 3);
        pnlTotales.SetColumnSpan(lbTarifa, 2);
        pnlTotales.Controls.Add(lbEnvio, 0, 4);
        pnlTotales.Controls.Add(cmbEnvio, 1, 4);
        pnlTotales.Controls.Add(lbTotal, 0, 5);
        pnlTotales.SetColumnSpan(lbTotal, 2);

        grpTotales.Controls.Add(pnlTotales);
        pnlInferior.Controls.Add(grpTotales, 0, 0);

        // --- Botones de acción ---
        var pnlAcciones = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.BottomUp,
            Padding = new Padding(10)
        };

        btnValidar = new Button
        {
            Text = "✓ Validar Pedido",
            Width = 200,
            Height = 40,
            BackColor = Color.FromArgb(30, 120, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        btnValidar.FlatAppearance.BorderSize = 0;
        btnValidar.Click += async (_, _) => await ValidarPedidoAsync();

        pnlAcciones.Controls.Add(btnValidar);
        pnlInferior.Controls.Add(pnlAcciones, 1, 0);

        pnlRaiz.Controls.Add(pnlInferior, 0, 2);
        Controls.Add(pnlRaiz);
    }

    private async void CargarCombos(object? sender, EventArgs e)
    {
        try
        {
            var clientes = await _repoClientes.ListarAsync();
            cmbClientes.DataSource = clientes;
            cmbClientes.DisplayMember = "Nombre";
            cmbClientes.ValueMember = "Id";

            var elementos = await _catalogo.ListarTodoAsync();
            cmbProductos.DataSource = elementos;
            cmbProductos.DisplayMember = "Nombre";
            cmbProductos.ValueMember = "Sku";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task AgregarLineaAsync()
    {
        if (cmbProductos.SelectedItem is not IElementoCatalogo elemento)
        {
            MessageBox.Show("Seleccione un producto o kit.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        GuardarMemento();

        if (_pedidoActual is null)
        {
            var cliente = (Cliente?)cmbClientes.SelectedItem;
            if (cliente is null)
            {
                MessageBox.Show("Seleccione un cliente.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Builder: construcción controlada
            var builder = new PedidoBuilder()
                .ConCliente(cliente)
                .ConEnvio(cmbEnvio.SelectedIndex == 0 ? TipoEnvio.Estandar : TipoEnvio.Expres)
                .AgregarLinea(elemento, (int)nudCantidad.Value);
            _pedidoActual = builder.Construir();
        }
        else
        {
            _pedidoActual.AgregarLinea(elemento, (int)nudCantidad.Value);
        }

        Recalcular();
        RefrescarGrid();
    }

    private void QuitarLinea()
    {
        if (_pedidoActual is null || dgvCarrito.CurrentRow is null) return;
        GuardarMemento();
        var sku = dgvCarrito.CurrentRow.Cells["colSku"].Value?.ToString();
        if (sku is not null)
        {
            _pedidoActual.QuitarLinea(sku);
            Recalcular();
            RefrescarGrid();
        }
    }

    private void Recalcular()
    {
        if (_pedidoActual is null) return;

        var unidades = _pedidoActual.Detalles.Sum(d => d.Cantidad);
        var cliente = (Cliente?)cmbClientes.SelectedItem;
        var categoria = cliente?.Categoria ?? CategoriaCliente.Normal;

        var (estrategia, descuento) = _descuentos.Calcular(_pedidoActual.Subtotal, unidades, categoria);
        var familia = _envioFactory.Crear(cmbEnvio.SelectedIndex == 0 ? TipoEnvio.Estandar : TipoEnvio.Expres);

        _pedidoActual.AplicarDescuento(descuento, estrategia.Nombre);
        _pedidoActual.AplicarTarifa(familia.CalcularTarifa());

        lbSubtotal.Text = $"Subtotal: $ {_pedidoActual.Subtotal.Amount:0.00}";
        lbDescuento.Text = $"Descuento: $ {_pedidoActual.Descuento.Amount:0.00}";
        lbEstrategia.Text = $"Estrategia: {_pedidoActual.EstrategiaDescuentoAplicada}";
        lbTarifa.Text = $"Tarifa: $ {_pedidoActual.TarifaEnvio.Amount:0.00}";
        lbTotal.Text = $"TOTAL: $ {_pedidoActual.Total.Amount:0.00}";
    }

    private void RefrescarGrid()
    {
        dgvCarrito.Rows.Clear();
        if (_pedidoActual is null) return;

        foreach (var d in _pedidoActual.Detalles)
        {
            dgvCarrito.Rows.Add(d.Sku, d.Nombre, d.EsKit ? "KIT" : "PRODUCTO",
                d.Cantidad, d.PrecioUnitarioInstantanea.Amount, d.SubtotalLinea.Amount);
        }
    }

    // --- MEMENTO: captura de estado ---
    private void GuardarMemento()
    {
        if (_pedidoActual is null) return;
        _pilaDeshacer.Push((Pedido)_pedidoActual.Clone());
        if (_pilaDeshacer.Count > MAX_HISTORIAL)
        {
            var temp = _pilaDeshacer.ToArray();
            _pilaDeshacer.Clear();
            for (int i = temp.Length - 1; i >= 0; i--) _pilaDeshacer.Push(temp[i]);
        }
        _pilaRehacer.Clear();
    }

    private void Deshacer()
    {
        if (_pilaDeshacer.Count == 0)
        {
            MessageBox.Show("No hay estados para deshacer.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (_pedidoActual is not null)
            _pilaRehacer.Push(_pedidoActual);
        _pedidoActual = _pilaDeshacer.Pop();
        Recalcular();
        RefrescarGrid();
    }

    private void Rehacer()
    {
        if (_pilaRehacer.Count == 0)
        {
            MessageBox.Show("No hay estados para rehacer.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (_pedidoActual is not null)
            _pilaDeshacer.Push(_pedidoActual);
        _pedidoActual = _pilaRehacer.Pop();
        Recalcular();
        RefrescarGrid();
    }

    // --- PROTOTYPE: clonar borrador ---
    private void ClonarBorrador()
    {
        if (_pedidoActual is null)
        {
            MessageBox.Show("No hay borrador para clonar.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var clon = (Pedido)_pedidoActual.Clone();
        _pedidoActual = clon;
        _pilaDeshacer.Clear();
        _pilaRehacer.Clear();
        MessageBox.Show("Borrador clonado (nuevo ID). La copia es profunda.",
            "Prototype", MessageBoxButtons.OK, MessageBoxIcon.Information);
        Recalcular();
        RefrescarGrid();
    }

    private async Task ValidarPedidoAsync()
    {
        if (_pedidoActual is null)
        {
            MessageBox.Show("No hay pedido para validar.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _pedidoActual.Validar();
            _pilaDeshacer.Clear();
            _pilaRehacer.Clear();
            MessageBox.Show("Pedido validado correctamente.", "Éxito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            _mediator.NotificarPedidoConfirmado(_pedidoActual.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error de validación: {ex.Message}", "Rechazo",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) { }
    public void OnStockModificado() { }
    public void OnCatalogoActualizado() { }
    public void OnDespachoRegistrado(Guid pedidoId) { }
    public void OnDevolucionProcesada(Guid pedidoId) { }
}