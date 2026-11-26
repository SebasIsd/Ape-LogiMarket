using LogiMarket.Application.Discount;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Application.Shipping;
using LogiMarket.Domain.Builders;
using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Enums;
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
    private Button btnActualizarCant;
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

    // --- Dependencias ---
    private readonly IRepositorioCatalogo _catalogo;
    private readonly IRepositorioCliente _repoClientes;
    private readonly CalculadoraDescuentoContext _descuentos;
    private readonly IFormularioMediator _mediator;
    private readonly IRepositorioPedido _repoPedidos;

    // --- Estado persistente entre cambios de pestaña ---
    private static Pedido? _pedidoActual;
    private static Pedido? _borradorOriginal;
    private static readonly Stack<Pedido?> _pilaDeshacer = new();
    private static readonly Stack<Pedido?> _pilaRehacer = new();
    private const int MAX_HISTORIAL = 5;
    private readonly EnvioFactoryProvider _envioProvider;

    public FrmCarrito(
        IRepositorioCatalogo catalogo,
        IRepositorioCliente repoClientes,
        CalculadoraDescuentoContext descuentos,
        IFormularioMediator mediator,
        IRepositorioPedido repoPedidos,
        EnvioFactoryProvider envioProvider)
    {
        _catalogo = catalogo;
        _repoClientes = repoClientes;
        _descuentos = descuentos;
        _mediator = mediator;
        _mediator.Registrar(this);
        _repoPedidos = repoPedidos;
        _envioProvider = envioProvider;
        InicializarComponentes();
        Load += CargarCombos;
        
    }

    private static string ObtenerMensaje(Exception ex)
    {
        var actual = ex;
        while (actual.InnerException is not null)
            actual = actual.InnerException;
        return actual.Message;
    }

    private void InicializarComponentes()
    {
        BackColor = UIHelper.ColorGrisClaro;

        pnlRaiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(15),
            BackColor = UIHelper.ColorGrisClaro
        };
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // ============================================
        // BARRA SUPERIOR
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
        cmbClientes = UIHelper.CrearComboBox(150);
        cmbClientes.Margin = new Padding(0, 5, 10, 4);

        lbProducto = UIHelper.CrearLabel("Producto/Kit:", negrita: true);
        lbProducto.Margin = new Padding(0, 8, 5, 4);
        cmbProductos = UIHelper.CrearComboBox(165);
        cmbProductos.Margin = new Padding(0, 5, 10, 4);

        lbCantidad = UIHelper.CrearLabel("Cant.:", negrita: true);
        lbCantidad.Margin = new Padding(0, 8, 5, 4);
        nudCantidad = new NumericUpDown
        {
            Width = 60,
            Minimum = 1,
            Maximum = 999,
            Value = 1,
            Font = UIHelper.FuenteNormal,
            Margin = new Padding(0, 5, 10, 4)
        };

        btnAgregar = UIHelper.CrearBotonSecundario("Agregar", 80);
        btnAgregar.Margin = new Padding(3, 4, 3, 4);
        btnAgregar.Click += (_, _) => AgregarLinea();

        btnActualizarCant = UIHelper.CrearBoton("✎ Cant.", UIHelper.ColorPrimario, ancho: 78);
        btnActualizarCant.Margin = new Padding(3, 4, 3, 4);
        btnActualizarCant.Click += (_, _) => ActualizarCantidadSeleccionada();

        btnQuitar = UIHelper.CrearBotonPeligro("Quitar", 72);
        btnQuitar.Margin = new Padding(3, 4, 3, 4);
        btnQuitar.Click += QuitarLinea;

        btnDeshacer = UIHelper.CrearBotonGris("↶ Deshacer", 95);
        btnDeshacer.Margin = new Padding(3, 4, 3, 4);
        btnDeshacer.Click += (_, _) => Deshacer();

        btnRehacer = UIHelper.CrearBotonGris("↷ Rehacer", 95);
        btnRehacer.Margin = new Padding(3, 4, 3, 4);
        btnRehacer.Click += (_, _) => Rehacer();

        btnClonar = UIHelper.CrearBoton("⎘ Clonar", UIHelper.ColorInfo, ancho: 84);
        btnClonar.Margin = new Padding(3, 4, 3, 4);
        btnClonar.Click += (_, _) => ClonarBorrador();

        btnLimpiar = UIHelper.CrearBotonGris("✕ Limpiar", 84);
        btnLimpiar.Margin = new Padding(3, 4, 3, 4);
        btnLimpiar.Click += (_, _) =>
        {
            _pedidoActual = null;
            _borradorOriginal = null;
            _pilaDeshacer.Clear();
            _pilaRehacer.Clear();
            EstablecerEdicionHabilitada(true);
            RefrescarGrid();
            ResetearResumen();
        };

        pnlSuperior.Controls.AddRange(new Control[]
        {
            lbCliente, cmbClientes,
            lbProducto, cmbProductos,
            lbCantidad, nudCantidad,
            btnAgregar, btnActualizarCant, btnQuitar,
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
        // PANEL INFERIOR
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

        var lblSub = UIHelper.CrearLabel("Subtotal:", negrita: true);
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

        var lblDesc = UIHelper.CrearLabel("Descuento:", negrita: true);
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
        lbEnvio.Anchor = AnchorStyles.Left | AnchorStyles.None;
        pnlTotales.Controls.Add(lbEnvio, 2, 1);

        cmbEnvio = UIHelper.CrearComboBox(130);
        cmbEnvio.Anchor = AnchorStyles.Left | AnchorStyles.None;
        cmbEnvio.Items.AddRange(new object[] { "ESTÁNDAR", "EXPRÉS" });
        cmbEnvio.SelectedIndex = 0;
        cmbEnvio.SelectedIndexChanged += (_, _) => Recalcular();
        cmbClientes.SelectedIndexChanged += (_, _) => Recalcular();
        pnlTotales.Controls.Add(cmbEnvio, 3, 1);

        var lblTar = UIHelper.CrearLabel("Tarifa:", negrita: true);
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

        var lblTot = UIHelper.CrearLabel("TOTAL:", negrita: true);
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

    private void EstablecerEdicionHabilitada(bool habilitada)
    {
        btnAgregar.Enabled = habilitada;
        btnActualizarCant.Enabled = habilitada;
        btnQuitar.Enabled = habilitada;
        btnDeshacer.Enabled = habilitada;
        btnRehacer.Enabled = habilitada;
        btnClonar.Enabled = habilitada;
        btnValidar.Enabled = habilitada;
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
            var clienteSel = cmbClientes.SelectedValue as Guid?;
            var productoSel = cmbProductos.SelectedValue as string;

            var clientes = await _repoClientes.ListarAsync();
            cmbClientes.DataSource = clientes;
            cmbClientes.DisplayMember = "Nombre";
            cmbClientes.ValueMember = "Id";
            if (_pedidoActual is not null)
                cmbClientes.SelectedValue = _pedidoActual.ClienteId;
            else if (clienteSel.HasValue)
                cmbClientes.SelectedValue = clienteSel.Value;

            var elementos = await _catalogo.ListarTodoAsync();
            cmbProductos.DataSource = elementos;
            cmbProductos.DisplayMember = "Nombre";
            cmbProductos.ValueMember = "Sku";
            if (productoSel is not null) cmbProductos.SelectedValue = productoSel;

            // Restaurar vista si ya había un borrador en memoria al volver a esta pestaña
            if (_pedidoActual is not null)
            {
                EstablecerEdicionHabilitada(_pedidoActual.EstadoActual == "BORRADOR");
                Recalcular();
                RefrescarGrid();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ObtenerMensaje(ex)}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AgregarLinea()
    {
        if (cmbProductos.SelectedItem is not IElementoCatalogo elemento)
        {
            MessageBox.Show("Seleccione un producto o kit.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            // Capturamos snapshot ANTES de modificar (incluso si es null)
            var snapshotAnterior = _pedidoActual is not null ? (Pedido)_pedidoActual.Clone() : null;

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

            GuardarEnPilaDeshacer(snapshotAnterior);
            _pilaRehacer.Clear();
            Recalcular();
            RefrescarGrid();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ObtenerMensaje(ex), "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ActualizarCantidadSeleccionada()
    {
        if (_pedidoActual is null || dgvCarrito.CurrentRow is null)
        {
            MessageBox.Show("Seleccione una fila del carrito para cambiar su cantidad.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var sku = dgvCarrito.CurrentRow.Cells["colSku"].Value?.ToString();
            if (string.IsNullOrEmpty(sku)) return;

            var snapshotAnterior = (Pedido)_pedidoActual.Clone();
            _pedidoActual.ActualizarCantidad(sku, (int)nudCantidad.Value);

            GuardarEnPilaDeshacer(snapshotAnterior);
            _pilaRehacer.Clear();
            Recalcular();
            RefrescarGrid();

            // Si existe un original clonado, mostramos la evidencia de copia profunda (P14)
            if (_borradorOriginal is not null)
            {
                var cantClon = _pedidoActual.Detalles.FirstOrDefault(d => d.Sku == sku)?.Cantidad ?? 0;
                var cantOrig = _borradorOriginal.Detalles.FirstOrDefault(d => d.Sku == sku)?.Cantidad ?? 0;
                MessageBox.Show(
                    $"Cantidad actualizada en el clon (copia profunda):\n" +
                    $"• Clon ID {_pedidoActual.Id.ToString("N")[..8]}: {cantClon} ud(s)\n" +
                    $"• Original ID {_borradorOriginal.Id.ToString("N")[..8]}: {cantOrig} ud(s) (intacto)",
                    "Prototype (P14)", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ObtenerMensaje(ex), "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void QuitarLinea(object? sender, EventArgs e)
    {
        if (_pedidoActual is null || dgvCarrito.CurrentRow is null) return;

        try
        {
            var sku = dgvCarrito.CurrentRow.Cells["colSku"].Value?.ToString();
            if (string.IsNullOrEmpty(sku)) return;

            var snapshotAnterior = (Pedido)_pedidoActual.Clone();
            _pedidoActual.QuitarLinea(sku);

            GuardarEnPilaDeshacer(snapshotAnterior);
            _pilaRehacer.Clear();
            Recalcular();
            RefrescarGrid();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ObtenerMensaje(ex), "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void Recalcular()
    {
        if (_pedidoActual is null)
        {
            ResetearResumen();
            return;
        }

        try
        {
            // 1. Calcular unidades físicas finales (necesario para el Strategy de descuento)
            var unidades = _pedidoActual.UnidadesFisicasFinales();

            // 2. Determinar categoría del cliente
            var cliente = (Cliente?)cmbClientes.SelectedItem;
            var categoria = cliente?.Categoria ?? CategoriaCliente.Normal;

            // 3. Aplicar Descuento (Strategy)
            var (estrategia, descuento) = _descuentos.Calcular(_pedidoActual.Subtotal, unidades, categoria);
            _pedidoActual.AplicarDescuento(descuento, estrategia.Nombre);

            // 4. Aplicar Tarifa de envío (Abstract Factory)
            var tipo = cmbEnvio.SelectedIndex == 0 ? TipoEnvio.Estandar : TipoEnvio.Expres;
            IEnvioAbstractFactory factory = _envioProvider.ObtenerFactory(tipo);
            IFamiliaEnvio familia = factory.CrearFamilia();

            _pedidoActual.AplicarTarifa(familia.CalcularTarifa()); // ← Se aplica UNA sola vez

            // 5. Actualizar la interfaz de usuario
            lbSubtotal.Text = $"$ {_pedidoActual.Subtotal.Amount:0.00}";
            lbDescuento.Text = $"$ {_pedidoActual.Descuento.Amount:0.00}";
            lbEstrategia.Text = _pedidoActual.EstrategiaDescuentoAplicada;
            lbTarifa.Text = $"$ {_pedidoActual.TarifaEnvio.Amount:0.00}";
            lbTotal.Text = $"$ {_pedidoActual.Total.Amount:0.00}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ObtenerMensaje(ex), "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
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

    // ==========================================
    // MEMENTO (Deshacer / Rehacer hasta 5 estados)
    // ==========================================

    private static void GuardarEnPilaDeshacer(Pedido? snapshot)
    {
        //se guarda el estado actual (snapshot) en el tope de la pila
        _pilaDeshacer.Push(snapshot);

        //si excedemos el límite de 5, debemos eliminar el estado más antiguo (el fondo de la pila)
        if (_pilaDeshacer.Count > MAX_HISTORIAL)
        {
            // Extraemos todo para poder manipular el fondo
            var lista = new List<Pedido?>();
            while (_pilaDeshacer.Count > 0)
            {
                lista.Add(_pilaDeshacer.Pop());
            }

            // 'lista' ahora tiene: [más reciente, ..., más antiguo]
            // Eliminamos el más antiguo (el último elemento de la lista)
            lista.RemoveAt(lista.Count - 1);

            // Restauramos la pila en orden inverso para que el más reciente quede en el tope
            for (int i = lista.Count - 1; i >= 0; i--)
            {
                _pilaDeshacer.Push(lista[i]);
            }
        }
    }

    private void Deshacer()
    {
        if (_pilaDeshacer.Count == 0)
        {
            MessageBox.Show("No hay estados para deshacer.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            // Guardamos el estado actual en la pila de Rehacer antes de sobrescribirlo
            _pilaRehacer.Push(_pedidoActual is not null ? (Pedido)_pedidoActual.Clone() : null);

            // Recuperamos el estado anterior
            _pedidoActual = _pilaDeshacer.Pop();

            Recalcular();
            RefrescarGrid();

            // Habilitar/deshabilitar botones según si el estado recuperado es BORRADOR
            EstablecerEdicionHabilitada(_pedidoActual?.EstadoActual == "BORRADOR");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ObtenerMensaje(ex), "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void Rehacer()
    {
        if (_pilaRehacer.Count == 0)
        {
            MessageBox.Show("No hay estados para rehacer.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            // Guardamos el estado actual en Deshacer antes de sobrescribirlo
            _pilaDeshacer.Push(_pedidoActual is not null ? (Pedido)_pedidoActual.Clone() : null);

            // Recuperamos el estado futuro
            _pedidoActual = _pilaRehacer.Pop();

            Recalcular();
            RefrescarGrid();

            EstablecerEdicionHabilitada(_pedidoActual?.EstadoActual == "BORRADOR");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ObtenerMensaje(ex), "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // --- PROTOTYPE ---
    private void ClonarBorrador()
    {
        if (_pedidoActual is null)
        {
            MessageBox.Show("No hay borrador para clonar.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var original = (Pedido)_pedidoActual.Clone();
            var clon = (Pedido)_pedidoActual.Clone();

            GuardarEnPilaDeshacer((Pedido)_pedidoActual.Clone());
            _borradorOriginal = original;
            _pedidoActual = clon;
            _pilaRehacer.Clear();

            MessageBox.Show(
                $"Borrador clonado (copia profunda).\n" +
                $"• Original ID: {original.Id.ToString("N")[..8]}\n" +
                $"• Clon ID:     {clon.Id.ToString("N")[..8]}\n\n" +
                $"Ahora selecciona la fila, cambia Cantidad y pulsa '✎ Cant.' para editar solo el clon.",
                "Prototype", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Recalcular();
            RefrescarGrid();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ObtenerMensaje(ex), "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
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
            EstablecerEdicionHabilitada(false);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error de validación: {ObtenerMensaje(ex)}", "Rechazo",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) { }
    public void OnStockModificado() { }
    public void OnCatalogoActualizado() => CargarCombos(this, EventArgs.Empty);
    public void OnDespachoRegistrado(Guid pedidoId) { }
    public void OnDevolucionProcesada(Guid pedidoId) { }
}