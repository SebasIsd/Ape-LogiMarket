using LogiMarket.Application.Discount;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Application.Shipping;
using LogiMarket.Domain.Builders;
using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Enums;
using LogiMarket.Domain.ValueObjects;
using LogiMarket.WinForms.Mediator;
using LogiMarket.WinForms.UI;
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
    private readonly IEnvioAbstractFactory _envioFactory;
    private readonly IFormularioMediator _mediator;
    private readonly IRepositorioPedido _repoPedidos;

    private Pedido? _pedidoActual;
    private readonly Stack<Pedido> _pilaDeshacer = new();
    private readonly Stack<Pedido> _pilaRehacer = new();
    private const int MAX_HISTORIAL = 5;

    public FrmCarrito(
        IRepositorioCatalogo catalogo,
        IRepositorioCliente repoClientes,
        CalculadoraDescuentoContext descuentos,
        IEnvioAbstractFactory envioFactory,
        IFormularioMediator mediator,
        IRepositorioPedido repoPedidos)
    {
        _catalogo = catalogo;
        _repoClientes = repoClientes;
        _descuentos = descuentos;
        _envioFactory = envioFactory;
        _mediator = mediator;
        _mediator.Registrar(this);

        InicializarComponentes();
        Load += CargarCombos;
        _repoPedidos = repoPedidos;
    }

    private void InicializarComponentes()
    {
        BackColor = UIHelper.ColorGrisClaro;

        // --- Layout principal: barra superior (auto) + grid (flexible) + inferior (auto) ---
        pnlRaiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(15),
            BackColor = UIHelper.ColorGrisClaro
        };
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // 0: Barra superior
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // 1: Grid
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // 2: Panel inferior

        // ============================================
        // BARRA SUPERIOR: Cliente | Producto | Cantidad | Botones
        // ============================================
        var pnlSuperior = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(0, 2, 0, 10),
            Margin = new Padding(0)
        };

        lbCliente = UIHelper.CrearLabel("Cliente:", negrita: true);
        lbCliente.Margin = new Padding(0, 8, 5, 4);
        cmbClientes = UIHelper.CrearComboBox(160);
        cmbClientes.Margin = new Padding(0, 5, 12, 4);

        lbProducto = UIHelper.CrearLabel("Producto/Kit:", negrita: true);
        lbProducto.Margin = new Padding(0, 8, 5, 4);
        cmbProductos = UIHelper.CrearComboBox(175);
        cmbProductos.Margin = new Padding(0, 5, 12, 4);

        lbCantidad = UIHelper.CrearLabel("Cantidad:", negrita: true);
        lbCantidad.Margin = new Padding(0, 8, 5, 4);
        nudCantidad = new NumericUpDown
        {
            Width = 65,
            Minimum = 1,
            Maximum = 999,
            Value = 1,
            Font = UIHelper.FuenteNormal,
            Margin = new Padding(0, 5, 12, 4)
        };

        btnAgregar = UIHelper.CrearBotonSecundario("Agregar", 85);
        btnAgregar.Margin = new Padding(4, 4, 4, 4);
        btnAgregar.Click += async (_, _) => await AgregarLineaAsync();

        btnQuitar = UIHelper.CrearBotonPeligro("Quitar", 78);
        btnQuitar.Margin = new Padding(4, 4, 4, 4);
        btnQuitar.Click += QuitarLinea;

        btnDeshacer = UIHelper.CrearBotonGris("↶ Deshacer", 98);
        btnDeshacer.Margin = new Padding(4, 4, 4, 4);
        btnDeshacer.Click += (_, _) => Deshacer();

        btnRehacer = UIHelper.CrearBotonGris("↷ Rehacer", 98);
        btnRehacer.Margin = new Padding(4, 4, 4, 4);
        btnRehacer.Click += (_, _) => Rehacer();

        btnClonar = UIHelper.CrearBoton("⎘ Clonar", UIHelper.ColorInfo, ancho: 88);
        btnClonar.Margin = new Padding(4, 4, 4, 4);
        btnClonar.Click += (_, _) => ClonarBorrador();

        btnLimpiar = UIHelper.CrearBotonGris("✕ Limpiar", 88);
        btnLimpiar.Margin = new Padding(4, 4, 4, 4);
        btnLimpiar.Click += (_, _) =>
        {
            _pedidoActual = null;
            _pilaDeshacer.Clear();
            _pilaRehacer.Clear();
            RefrescarGrid();
            ResetearResumen();
        };

        pnlSuperior.Controls.AddRange(new Control[]
        {
            lbCliente, cmbClientes,
            lbProducto, cmbProductos,
            lbCantidad, nudCantidad,
            btnAgregar, btnQuitar,
            btnDeshacer, btnRehacer,
            btnClonar, btnLimpiar
        });
        pnlRaiz.Controls.Add(pnlSuperior, 0, 0);

        // ============================================
        // GRID DEL CARRITO
        // ============================================
        dgvCarrito = UIHelper.CrearDataGridView();
        dgvCarrito.Dock = DockStyle.Fill;
        dgvCarrito.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvCarrito.Margin = new Padding(0, 0, 0, 12);

        var colSku = dgvCarrito.Columns.Add("colSku", "SKU");
        var colNombre = dgvCarrito.Columns.Add("colNombre", "Nombre");
        var colTipo = dgvCarrito.Columns.Add("colTipo", "Tipo");
        var colCantidad = dgvCarrito.Columns.Add("colCantidad", "Cant.");
        var colPrecio = dgvCarrito.Columns.Add("colPrecioUnitario", "Precio Unit.");
        var colSubtotal = dgvCarrito.Columns.Add("colSubtotal", "Subtotal");

        dgvCarrito.Columns[colSku].FillWeight = 14;
        dgvCarrito.Columns[colNombre].FillWeight = 34;
        dgvCarrito.Columns[colTipo].FillWeight = 13;
        dgvCarrito.Columns[colCantidad].FillWeight = 10;
        dgvCarrito.Columns[colPrecio].FillWeight = 14;
        dgvCarrito.Columns[colSubtotal].FillWeight = 15;

        dgvCarrito.Columns["colPrecioUnitario"]!.DefaultCellStyle.Format = "0.00";
        dgvCarrito.Columns["colSubtotal"]!.DefaultCellStyle.Format = "0.00";
        pnlRaiz.Controls.Add(dgvCarrito, 0, 1);

        // ============================================
        // PANEL INFERIOR: Resumen (izquierda) + Botón Validar (derecha)
        // ============================================
        var pnlInferior = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        pnlInferior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
        pnlInferior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));

        // --- Grupo Resumen ---
        // --- Grupo Resumen ---
        grpTotales = UIHelper.CrearGroupBox("Resumen del pedido");
        grpTotales.Dock = DockStyle.Fill;
        grpTotales.AutoSize = true;
        grpTotales.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        grpTotales.Margin = new Padding(0, 0, 10, 0);

        var pnlTotales = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 4,
            RowCount = 4,
            Padding = new Padding(14, 22, 14, 14)
        };
        pnlTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        pnlTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        pnlTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        pnlTotales.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        pnlTotales.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        pnlTotales.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        pnlTotales.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        pnlTotales.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        // Fila 0: Subtotal | Estrategia
        var lblSub = UIHelper.CrearLabel("Subtotal:", negrita: true);
        lblSub.AutoSize = true;
        lblSub.Anchor = AnchorStyles.Left | AnchorStyles.None;
        pnlTotales.Controls.Add(lblSub, 0, 0);

        lbSubtotal = new Label
        {
            Text = "$ 0.00",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.None,
            Font = UIHelper.FuenteNormal
        };
        pnlTotales.Controls.Add(lbSubtotal, 1, 0);

        var lblEst = UIHelper.CrearLabel("Estrategia:", negrita: true);
        lblEst.AutoSize = true;
        lblEst.Anchor = AnchorStyles.Left | AnchorStyles.None;
        pnlTotales.Controls.Add(lblEst, 2, 0);

        lbEstrategia = new Label
        {
            Text = "NORMAL",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.None,
            Font = UIHelper.FuenteNormal
        };
        pnlTotales.Controls.Add(lbEstrategia, 3, 0);

        // Fila 1: Descuento | Envío
        var lblDesc = UIHelper.CrearLabel("Descuento:", negrita: true);
        lblDesc.AutoSize = true;
        lblDesc.Anchor = AnchorStyles.Left | AnchorStyles.None;
        pnlTotales.Controls.Add(lblDesc, 0, 1);

        lbDescuento = new Label
        {
            Text = "$ 0.00",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.None,
            Font = UIHelper.FuenteNormal
        };
        pnlTotales.Controls.Add(lbDescuento, 1, 1);

        lbEnvio = UIHelper.CrearLabel("Envío:", negrita: true);
        lbEnvio.AutoSize = true;
        lbEnvio.Anchor = AnchorStyles.Left | AnchorStyles.None;
        pnlTotales.Controls.Add(lbEnvio, 2, 1);

        cmbEnvio = UIHelper.CrearComboBox(130);
        cmbEnvio.Anchor = AnchorStyles.Left | AnchorStyles.None;
        cmbEnvio.Items.AddRange(new object[] { "ESTÁNDAR", "EXPRÉS" });
        cmbEnvio.SelectedIndex = 0;
        cmbEnvio.SelectedIndexChanged += (_, _) => Recalcular();
        pnlTotales.Controls.Add(cmbEnvio, 3, 1);

        // Fila 2: Tarifa
        var lblTar = UIHelper.CrearLabel("Tarifa:", negrita: true);
        lblTar.AutoSize = true;
        lblTar.Anchor = AnchorStyles.Left | AnchorStyles.None;
        pnlTotales.Controls.Add(lblTar, 0, 2);

        lbTarifa = new Label
        {
            Text = "$ 0.00",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.None,
            Font = UIHelper.FuenteNormal
        };
        pnlTotales.Controls.Add(lbTarifa, 1, 2);

        // Fila 3: TOTAL
        var lblTot = UIHelper.CrearLabel("TOTAL:", negrita: true);
        lblTot.AutoSize = true;
        lblTot.Anchor = AnchorStyles.Left | AnchorStyles.None;
        pnlTotales.Controls.Add(lblTot, 0, 3);

        lbTotal = new Label
        {
            Text = "$ 0.00",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.None,
            Font = new Font(UIHelper.FuenteGrande.FontFamily, 14, FontStyle.Bold),
            ForeColor = UIHelper.ColorPrimario
        };
        pnlTotales.SetColumnSpan(lbTotal, 3);
        pnlTotales.Controls.Add(lbTotal, 1, 3);

        grpTotales.Controls.Add(pnlTotales);
        pnlInferior.Controls.Add(grpTotales, 0, 0);


        // --- Botón Validar (centrado en su celda) ---
        var pnlBotonValidar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(8)
        };
        pnlBotonValidar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        pnlBotonValidar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        btnValidar = UIHelper.CrearBotonPrimario("✓ Validar Pedido", 210);
        btnValidar.Height = 52;
        btnValidar.Font = UIHelper.FuenteGrande;
        btnValidar.Anchor = AnchorStyles.None;
        btnValidar.Click += async (_, _) => await ValidarPedidoAsync();

        pnlBotonValidar.Controls.Add(btnValidar, 0, 0);
        pnlInferior.Controls.Add(pnlBotonValidar, 1, 0);

        pnlRaiz.Controls.Add(pnlInferior, 0, 2);
        Controls.Add(pnlRaiz);
    }

    private void ResetearResumen()
    {
        lbSubtotal.Text = "$ 0.00";
        lbDescuento.Text = "$ 0.00";
        lbEstrategia.Text = "NORMAL";
        lbTarifa.Text = "$ 0.00";
        lbTotal.Text = "$ 0.00";
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

    private void QuitarLinea(object? sender, EventArgs e)
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
        if (_pedidoActual is null)
        {
            ResetearResumen();
            return;
        }

        var unidades = _pedidoActual.Detalles.Sum(d => d.Cantidad);
        var cliente = (Cliente?)cmbClientes.SelectedItem;
        var categoria = cliente?.Categoria ?? CategoriaCliente.Normal;

        var (estrategia, descuento) = _descuentos.Calcular(_pedidoActual.Subtotal, unidades, categoria);
        var familia = _envioFactory.Crear(cmbEnvio.SelectedIndex == 0 ? TipoEnvio.Estandar : TipoEnvio.Expres);

        _pedidoActual.AplicarDescuento(descuento, estrategia.Nombre);
        _pedidoActual.AplicarTarifa(familia.CalcularTarifa());

        lbSubtotal.Text = $"$ {_pedidoActual.Subtotal.Amount:0.00}";
        lbDescuento.Text = $"$ {_pedidoActual.Descuento.Amount:0.00}";
        lbEstrategia.Text = _pedidoActual.EstrategiaDescuentoAplicada;
        lbTarifa.Text = $"$ {_pedidoActual.TarifaEnvio.Amount:0.00}";
        lbTotal.Text = $"$ {_pedidoActual.Total.Amount:0.00}";
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
            await _repoPedidos.AgregarAsync(_pedidoActual);

            _pilaDeshacer.Clear();
            _pilaRehacer.Clear();

            MessageBox.Show("Pedido validado y guardado. Ahora está disponible en Cobro.",
                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

            _mediator.NotificarPedidoConfirmado(_pedidoActual.Id);

            // ★ Deshabilitar botones de edición después de validar
            btnAgregar.Enabled = false;
            btnQuitar.Enabled = false;
            btnDeshacer.Enabled = false;
            btnRehacer.Enabled = false;
            btnClonar.Enabled = false;  // ← No se puede clonar un pedido validado
            btnValidar.Enabled = false;
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
