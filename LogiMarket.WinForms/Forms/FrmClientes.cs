using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Enums;
using LogiMarket.WinForms.Mediator;
using LogiMarket.WinForms.UI;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public partial class FrmClientes : UserControl, IViewRefrescable
{
    // --- Componentes ---
    private TextBox txtBusqueda;
    private Button btnBuscar;
    private Button btnNuevo;
    private Button btnEditar;
    private Button btnDesactivar;
    private DataGridView dgvClientes;
    private GroupBox grpFormulario;
    private TextBox txtIdentificacion;
    private TextBox txtNombre;
    private TextBox txtCorreo;
    private TextBox txtTelefono;
    private ComboBox cmbCategoria;
    private Button btnGuardar;
    private Button btnCancelar;
    private TableLayoutPanel pnlRaiz;
    private Panel pnlSuperior;
    private FlowLayoutPanel pnlBotonera;

    private readonly IRepositorioCliente _repo;
    private readonly IFormularioMediator _mediator;
    private Guid? _clienteEditandoId;

    public FrmClientes(IRepositorioCliente repo, IFormularioMediator mediator)
    {
        _repo = repo;
        _mediator = mediator;
        _mediator.Registrar(this);

        InicializarComponentes();
        Load += async (_, _) => await CargarClientesAsync();
    }

    private void InicializarComponentes()
    {
        BackColor = UIHelper.ColorGrisClaro;

        // --- Layout: grid (60%) + formulario (40%) ---
        pnlRaiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(15)
        };
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

        // --- Panel superior: búsqueda + grid ---
        pnlSuperior = new Panel { Dock = DockStyle.Fill };

        var lbBusqueda = UIHelper.CrearLabel("Buscar:", negrita: true);
        lbBusqueda.Location = new Point(0, 8);

        txtBusqueda = UIHelper.CrearTextBox(250);
        txtBusqueda.Location = new Point(55, 5);
        txtBusqueda.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) await CargarClientesAsync(); };

        btnBuscar = UIHelper.CrearBotonPrimario("Buscar", 80);
        btnBuscar.Location = new Point(315, 4);
        btnBuscar.Click += async (_, _) => await CargarClientesAsync();

        btnNuevo = UIHelper.CrearBotonSecundario("+ Nuevo", 80);
        btnNuevo.Location = new Point(405, 4);
        btnNuevo.Click += (_, _) => LimpiarFormulario();

        dgvClientes = UIHelper.CrearDataGridView();
        dgvClientes.Location = new Point(0, 40);
        dgvClientes.Size = new Size(pnlSuperior.Width - 20, pnlSuperior.Height - 50);
        dgvClientes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        dgvClientes.SelectionChanged += DgvClientes_SelectionChanged;

        pnlSuperior.Controls.AddRange(new Control[] { lbBusqueda, txtBusqueda, btnBuscar, btnNuevo, dgvClientes });
        pnlSuperior.Resize += (_, _) =>
        {
            dgvClientes.Size = new Size(pnlSuperior.Width - 20, pnlSuperior.Height - 50);
        };
        pnlRaiz.Controls.Add(pnlSuperior, 0, 0);

        // --- Panel inferior: formulario ---
        grpFormulario = UIHelper.CrearGroupBox("Datos del cliente");
        grpFormulario.Dock = DockStyle.Fill;

        var pnlFormulario = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(10)
        };
        pnlFormulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        pnlFormulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 5; i++)
            pnlFormulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        pnlFormulario.Controls.Add(UIHelper.CrearLabel("Identificación:", negrita: true), 0, 0);
        txtIdentificacion = UIHelper.CrearTextBox();
        txtIdentificacion.Dock = DockStyle.Fill;
        pnlFormulario.Controls.Add(txtIdentificacion, 1, 0);

        pnlFormulario.Controls.Add(UIHelper.CrearLabel("Nombre:", negrita: true), 0, 1);
        txtNombre = UIHelper.CrearTextBox();
        txtNombre.Dock = DockStyle.Fill;
        pnlFormulario.Controls.Add(txtNombre, 1, 1);

        pnlFormulario.Controls.Add(UIHelper.CrearLabel("Correo:", negrita: true), 0, 2);
        txtCorreo = UIHelper.CrearTextBox();
        txtCorreo.Dock = DockStyle.Fill;
        pnlFormulario.Controls.Add(txtCorreo, 1, 2);

        pnlFormulario.Controls.Add(UIHelper.CrearLabel("Teléfono:", negrita: true), 0, 3);
        txtTelefono = UIHelper.CrearTextBox();
        txtTelefono.Dock = DockStyle.Fill;
        pnlFormulario.Controls.Add(txtTelefono, 1, 3);

        pnlFormulario.Controls.Add(UIHelper.CrearLabel("Categoría:", negrita: true), 0, 4);
        cmbCategoria = UIHelper.CrearComboBox();
        cmbCategoria.Dock = DockStyle.Fill;
        cmbCategoria.Items.AddRange(new object[] { "Normal", "VIP", "Mayorista" });
        cmbCategoria.SelectedIndex = 0;
        pnlFormulario.Controls.Add(cmbCategoria, 1, 4);

        grpFormulario.Controls.Add(pnlFormulario);
        pnlRaiz.Controls.Add(grpFormulario, 0, 1);

        // --- Botonera inferior ---
        pnlBotonera = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 50,
            Padding = new Padding(10, 10, 10, 5)
        };

        btnCancelar = UIHelper.CrearBotonGris("Cancelar", 100);
        btnCancelar.Click += (_, _) => LimpiarFormulario();

        btnGuardar = UIHelper.CrearBotonPrimario("Guardar", 100);
        btnGuardar.Click += async (_, _) => await GuardarClienteAsync();

        btnDesactivar = UIHelper.CrearBotonPeligro("Desactivar", 100);
        btnDesactivar.Click += async (_, _) => await DesactivarClienteAsync();

        pnlBotonera.Controls.AddRange(new Control[] { btnCancelar, btnGuardar, btnDesactivar });

        Controls.Add(pnlRaiz);
        Controls.Add(pnlBotonera);
    }

    private async Task CargarClientesAsync()
    {
        try
        {
            var clientes = await _repo.ListarAsync(txtBusqueda.Text.Trim());
            dgvClientes.DataSource = null;
            dgvClientes.Columns.Clear();

            dgvClientes.Columns.Add("colId", "Id");
            dgvClientes.Columns["colId"]!.Visible = false;
            dgvClientes.Columns.Add("colIdentificacion", "Identificación");
            dgvClientes.Columns.Add("colNombre", "Nombre");
            dgvClientes.Columns.Add("colCorreo", "Correo");
            dgvClientes.Columns.Add("colTelefono", "Teléfono");
            dgvClientes.Columns.Add("colCategoria", "Categoría");
            dgvClientes.Columns.Add("colEstado", "Estado");

            foreach (var c in clientes)
            {
                dgvClientes.Rows.Add(
                    c.Id, c.Identificacion, c.Nombre, c.Correo, c.Telefono,
                    c.Categoria.ToString().ToUpper(),
                    c.Activo ? "ACTIVO" : "INACTIVO");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DgvClientes_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvClientes.CurrentRow is null) return;
        _clienteEditandoId = Guid.Parse(dgvClientes.CurrentRow.Cells["colId"].Value!.ToString()!);
        txtIdentificacion.Text = dgvClientes.CurrentRow.Cells["colIdentificacion"].Value?.ToString();
        txtNombre.Text = dgvClientes.CurrentRow.Cells["colNombre"].Value?.ToString();
        txtCorreo.Text = dgvClientes.CurrentRow.Cells["colCorreo"].Value?.ToString();
        txtTelefono.Text = dgvClientes.CurrentRow.Cells["colTelefono"].Value?.ToString();

        var catTexto = dgvClientes.CurrentRow.Cells["colCategoria"].Value?.ToString()?.ToUpperInvariant();
        cmbCategoria.SelectedItem = catTexto switch
        {
            "VIP" => "VIP",
            "MAYORISTA" => "Mayorista",
            _ => "Normal"
        };
    }

    private void LimpiarFormulario()
    {
        _clienteEditandoId = null;
        txtIdentificacion.Clear();
        txtNombre.Clear();
        txtCorreo.Clear();
        txtTelefono.Clear();
        cmbCategoria.SelectedIndex = 0;
        txtIdentificacion.Focus();
    }

    private async Task GuardarClienteAsync()
    {
        if (string.IsNullOrWhiteSpace(txtIdentificacion.Text) ||
            string.IsNullOrWhiteSpace(txtNombre.Text) ||
            string.IsNullOrWhiteSpace(txtCorreo.Text))
        {
            MessageBox.Show("Identificación, nombre y correo son obligatorios.",
                "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var textoCategoria = cmbCategoria.SelectedItem?.ToString() ?? cmbCategoria.Text;
            if (!Enum.TryParse<CategoriaCliente>(textoCategoria, ignoreCase: true, out var categoria))
            {
                MessageBox.Show($"Categoría inválida: '{textoCategoria}'.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var identificacion = txtIdentificacion.Text.Trim();
            var nombre = txtNombre.Text.Trim();
            var correo = txtCorreo.Text.Trim();
            var telefono = txtTelefono.Text.Trim();

            if (_clienteEditandoId is null)
            {
                var existente = await _repo.ObtenerPorIdentificacionAsync(identificacion);
                if (existente is not null)
                {
                    MessageBox.Show("Ya existe un cliente con esa identificación.",
                        "Duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var nuevo = new Cliente(identificacion, nombre, correo, categoria);
                if (!string.IsNullOrWhiteSpace(telefono))
                {
                    nuevo.ActualizarDatos(nombre, correo, telefono, categoria);
                }

                await _repo.AgregarAsync(nuevo);
            }
            else
            {
                var cliente = await _repo.ObtenerPorIdAsync(_clienteEditandoId.Value);
                if (cliente is null)
                {
                    MessageBox.Show("Cliente no encontrado.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                cliente.ActualizarDatos(nombre, correo, telefono, categoria);
                await _repo.ActualizarAsync(cliente);
            }

            MessageBox.Show("Cliente guardado correctamente.", "Éxito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            await CargarClientesAsync();
            _mediator.NotificarCatalogoActualizado();
        }
        catch (Exception ex)
        {
            var causa = ex;
            while (causa.InnerException is not null) causa = causa.InnerException;
            MessageBox.Show($"Error: {causa.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DesactivarClienteAsync()
    {
        if (_clienteEditandoId is null)
        {
            MessageBox.Show("Seleccione un cliente de la tabla.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show("¿Desactivar este cliente? No podrá generar nuevos pedidos.",
            "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            var cliente = await _repo.ObtenerPorIdAsync(_clienteEditandoId.Value);
            if (cliente is null)
            {
                MessageBox.Show("Cliente no encontrado.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            cliente.Desactivar();
            await _repo.ActualizarAsync(cliente);
            MessageBox.Show("Cliente desactivado correctamente.", "Éxito",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            await CargarClientesAsync();
            _mediator.NotificarCatalogoActualizado();
        }
        catch (Exception ex)
        {
            var causa = ex;
            while (causa.InnerException is not null) causa = causa.InnerException;
            MessageBox.Show($"Error: {causa.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) => CargarClientesAsync().ConfigureAwait(false);
    public void OnStockModificado() { }
    public void OnCatalogoActualizado() => CargarClientesAsync().ConfigureAwait(false);
    public void OnDespachoRegistrado(Guid pedidoId) { }
    public void OnDevolucionProcesada(Guid pedidoId) { }
}