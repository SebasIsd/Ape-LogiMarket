using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Application.Validation;
using LogiMarket.Domain.Entities;
using LogiMarket.Domain.Enums;
using LogiMarket.WinForms.Mediator;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public class FrmClientes : UserControl, IViewRefrescable
{
    // --- Componentes ---
    private Label lbBusqueda;
    private TextBox txtBusqueda;
    private Button btnBuscar;
    private Button btnNuevo;
    private Button btnEditar;
    private Button btnDesactivar;
    private DataGridView dgvClientes;
    private GroupBox grpFormulario;
    private Label lbIdentificacion;
    private TextBox txtIdentificacion;
    private Label lbNombre;
    private TextBox txtNombre;
    private Label lbCorreo;
    private TextBox txtCorreo;
    private Label lbTelefono;
    private TextBox txtTelefono;
    private Label lbCategoria;
    private ComboBox cmbCategoria;
    private Button btnGuardar;
    private Button btnCancelar;
    private TableLayoutPanel pnlRaiz;
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
        BackColor = Color.FromArgb(245, 247, 250);

        // --- Layout principal: arriba grid, abajo formulario ---
        pnlRaiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(15)
        };
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 55));  // Grid
        pnlRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 45));  // Formulario

        // --- Panel superior: búsqueda + grid ---
        var pnlSuperior = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 0, 10) };

        lbBusqueda = new Label
        {
            Text = "Buscar:",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 8)
        };

        txtBusqueda = new TextBox
        {
            Location = new Point(55, 5),
            Width = 250,
            Font = new Font("Segoe UI", 10)
        };
        txtBusqueda.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) await CargarClientesAsync();
        };

        btnBuscar = new Button
        {
            Text = "Buscar",
            Location = new Point(315, 4),
            Width = 80,
            Height = 28,
            BackColor = Color.FromArgb(30, 120, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnBuscar.FlatAppearance.BorderSize = 0;
        btnBuscar.Click += async (_, _) => await CargarClientesAsync();

        btnNuevo = new Button
        {
            Text = "+ Nuevo",
            Location = new Point(405, 4),
            Width = 80,
            Height = 28,
            BackColor = Color.FromArgb(40, 160, 80),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnNuevo.FlatAppearance.BorderSize = 0;
        btnNuevo.Click += (_, _) => LimpiarFormulario();

        dgvClientes = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 40, 0, 0)
        };
        dgvClientes.SelectionChanged += DgvClientes_SelectionChanged;

        pnlSuperior.Controls.AddRange(new Control[] { lbBusqueda, txtBusqueda, btnBuscar, btnNuevo, dgvClientes });
        pnlRaiz.Controls.Add(pnlSuperior, 0, 0);

        // --- Panel inferior: formulario ---
        grpFormulario = new GroupBox
        {
            Text = "Datos del cliente",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 60, 90)
        };

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
            pnlFormulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

        lbIdentificacion = new Label { Text = "Identificación:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9) };
        txtIdentificacion = new TextBox { Dock = DockStyle.Fill, Height = 28, Font = new Font("Segoe UI", 10) };

        lbNombre = new Label { Text = "Nombre:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9) };
        txtNombre = new TextBox { Dock = DockStyle.Fill, Height = 28, Font = new Font("Segoe UI", 10) };

        lbCorreo = new Label { Text = "Correo:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9) };
        txtCorreo = new TextBox { Dock = DockStyle.Fill, Height = 28, Font = new Font("Segoe UI", 10) };

        lbTelefono = new Label { Text = "Teléfono:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9) };
        txtTelefono = new TextBox { Dock = DockStyle.Fill, Height = 28, Font = new Font("Segoe UI", 10) };

        lbCategoria = new Label { Text = "Categoría:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9) };
        cmbCategoria = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10)
        };
        cmbCategoria.Items.AddRange(new object[] { "Normal", "VIP" });
        cmbCategoria.SelectedIndex = 0;

        pnlFormulario.Controls.Add(lbIdentificacion, 0, 0);
        pnlFormulario.Controls.Add(txtIdentificacion, 1, 0);
        pnlFormulario.Controls.Add(lbNombre, 0, 1);
        pnlFormulario.Controls.Add(txtNombre, 1, 1);
        pnlFormulario.Controls.Add(lbCorreo, 0, 2);
        pnlFormulario.Controls.Add(txtCorreo, 1, 2);
        pnlFormulario.Controls.Add(lbTelefono, 0, 3);
        pnlFormulario.Controls.Add(txtTelefono, 1, 3);
        pnlFormulario.Controls.Add(lbCategoria, 0, 4);
        pnlFormulario.Controls.Add(cmbCategoria, 1, 4);

        grpFormulario.Controls.Add(pnlFormulario);
        pnlRaiz.Controls.Add(grpFormulario, 0, 1);

        // --- Botonera inferior ---
        pnlBotonera = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 45,
            Padding = new Padding(10, 5, 10, 5)
        };

        btnCancelar = new Button
        {
            Text = "Cancelar",
            Width = 100,
            Height = 32,
            BackColor = Color.FromArgb(230, 230, 230),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9)
        };
        btnCancelar.FlatAppearance.BorderSize = 0;
        btnCancelar.Click += (_, _) => LimpiarFormulario();

        btnGuardar = new Button
        {
            Text = "Guardar",
            Width = 100,
            Height = 32,
            BackColor = Color.FromArgb(30, 120, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        btnGuardar.FlatAppearance.BorderSize = 0;
        btnGuardar.Click += async (_, _) => await GuardarClienteAsync();

        btnDesactivar = new Button
        {
            Text = "Desactivar",
            Width = 100,
            Height = 32,
            BackColor = Color.FromArgb(200, 60, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9)
        };
        btnDesactivar.FlatAppearance.BorderSize = 0;
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
                    c.Id,
                    c.Identificacion,
                    c.Nombre,
                    c.Correo,
                    c.Telefono,
                    c.Categoria.ToString().ToUpper(),
                    c.Activo ? "ACTIVO" : "INACTIVO");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        cmbCategoria.SelectedItem = dgvClientes.CurrentRow.Cells["colCategoria"].Value?.ToString() == "VIP" ? "VIP" : "Normal";
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

        // Chain of Responsibility: validar cliente activo antes de guardar
        try
        {
            var categoria = (CategoriaCliente)Enum.Parse(typeof(CategoriaCliente), cmbCategoria.Text);

            if (_clienteEditandoId is null)
            {
                var existente = await _repo.ObtenerPorIdentificacionAsync(txtIdentificacion.Text.Trim());
                if (existente is not null)
                {
                    MessageBox.Show("Ya existe un cliente con esa identificación.",
                        "Duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var nuevo = new Cliente(txtIdentificacion.Text.Trim(), txtNombre.Text.Trim(),
                    txtCorreo.Text.Trim(), categoria);
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
                cliente.ActualizarDatos(txtNombre.Text.Trim(), txtCorreo.Text.Trim(),
                    txtTelefono.Text.Trim(), categoria);
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
            MessageBox.Show($"Error: {ex.Message}", "Error",
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

        var cliente = await _repo.ObtenerPorIdAsync(_clienteEditandoId.Value);
        if (cliente is null) return;

        cliente.Desactivar();
        await _repo.ActualizarAsync(cliente);
        await CargarClientesAsync();
        _mediator.NotificarCatalogoActualizado();
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) => CargarClientesAsync().ConfigureAwait(false);
    public void OnStockModificado() { }
    public void OnCatalogoActualizado() => CargarClientesAsync().ConfigureAwait(false);
    public void OnDespachoRegistrado(Guid pedidoId) { }
    public void OnDevolucionProcesada(Guid pedidoId) { }
}