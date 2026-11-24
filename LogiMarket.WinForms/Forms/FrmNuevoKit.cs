using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Exceptions;
using LogiMarket.WinForms.UI;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public class FrmNuevoKit : Form
{
    // --- Componentes ---
    private Label lbTitulo;
    private Label lbSku;
    private TextBox txtSku;
    private Label lbNombre;
    private TextBox txtNombre;
    private Label lbComponentes;
    private DataGridView dgvComponentes;
    private Label lbAgregar;
    private ComboBox cmbElemento;
    private NumericUpDown nudCantidad;
    private Button btnAgregar;
    private Button btnQuitar;
    private Button btnGuardar;
    private Button btnCancelar;
    private FlowLayoutPanel pnlBotonera;

    private readonly IRepositorioCatalogo _catalogo;
    private KitCompuesto? _kitEditando;
    private readonly List<(IElementoCatalogo Elemento, int Cantidad)> _componentes = new();
    public bool KitGuardado { get; private set; }

    public FrmNuevoKit(IRepositorioCatalogo catalogo, KitCompuesto? kitEditando = null)
    {
        _catalogo = catalogo;
        _kitEditando = kitEditando;

        InicializarComponentes();
        Load += async (_, _) => await CargarElementosAsync();
        CargarDatos();
    }

    private void InicializarComponentes()
    {
        Text = _kitEditando is null ? "Nuevo kit" : "Editar kit";
        ClientSize = new Size(600, 500);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(245, 247, 250);

        lbTitulo = new Label
        {
            Text = _kitEditando is null ? "Crear nuevo kit" : "Editar kit",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 60, 90),
            Location = new Point(20, 15),
            AutoSize = true
        };

        // --- Datos del kit ---
        var pnlDatos = new TableLayoutPanel
        {
            Location = new Point(20, 55),
            Size = new Size(560, 80),
            ColumnCount = 4,
            RowCount = 2
        };
        pnlDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        pnlDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        pnlDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        pnlDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        pnlDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        pnlDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

        lbSku = UIHelper.CrearLabel("SKU:", negrita: true);
        txtSku = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
        lbNombre = UIHelper.CrearLabel("Nombre:", negrita: true);
        txtNombre = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };

        pnlDatos.Controls.Add(lbSku, 0, 0);
        pnlDatos.Controls.Add(txtSku, 1, 0);
        pnlDatos.Controls.Add(lbNombre, 2, 0);
        pnlDatos.Controls.Add(txtNombre, 3, 0);

        // --- Sección componentes ---
        lbComponentes = UIHelper.CrearLabel("Componentes del kit:", negrita: true);
        lbComponentes.Location = new Point(20, 145);

        dgvComponentes = UIHelper.CrearDataGridView();
        dgvComponentes.Location = new Point(20, 170);
        dgvComponentes.Size = new Size(560, 180);
        dgvComponentes.Columns.Add("colElemento", "Elemento");
        dgvComponentes.Columns.Add("colTipo", "Tipo");
        dgvComponentes.Columns.Add("colCantidad", "Cantidad");

        // --- Agregar componente ---
        lbAgregar = UIHelper.CrearLabel("Agregar componente:", negrita: true);
        lbAgregar.Location = new Point(20, 360);

        cmbElemento = new ComboBox
        {
            Location = new Point(170, 358),
            Width = 250,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10)
        };

        nudCantidad = new NumericUpDown
        {
            Location = new Point(430, 358),
            Width = 60,
            Minimum = 1,
            Maximum = 999,
            Value = 1,
            Font = new Font("Segoe UI", 10)
        };

        btnAgregar = UIHelper.CrearBotonSecundario("+ Agregar", 100);
        btnAgregar.Location = new Point(500, 357);
        btnAgregar.Click += BtnAgregar_Click;

        btnQuitar = UIHelper.CrearBotonPeligro("Quitar", 80);
        btnQuitar.Location = new Point(20, 455);
        btnQuitar.Click += BtnQuitar_Click;

        // --- Botonera ---
        pnlBotonera = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 50,
            Padding = new Padding(10, 10, 10, 5)
        };

        btnCancelar = UIHelper.CrearBotonGris("Cancelar", 100);
        btnCancelar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        btnGuardar = UIHelper.CrearBotonPrimario("Guardar composición", 180);
        btnGuardar.Click += BtnGuardar_Click;

        pnlBotonera.Controls.AddRange(new Control[] { btnCancelar, btnGuardar });

        Controls.Add(lbTitulo);
        Controls.Add(pnlDatos);
        Controls.Add(lbComponentes);
        Controls.Add(dgvComponentes);
        Controls.Add(lbAgregar);
        Controls.Add(cmbElemento);
        Controls.Add(nudCantidad);
        Controls.Add(btnAgregar);
        Controls.Add(btnQuitar);
        Controls.Add(pnlBotonera);

        AcceptButton = btnAgregar;
    }

    private async Task CargarElementosAsync()
    {
        try
        {
            var elementos = await _catalogo.ListarTodoAsync();
            cmbElemento.Items.Clear();
            foreach (var elem in elementos)
            {
                // No permitir agregar el mismo kit que se está editando (anti-ciclo básico)
                if (_kitEditando is not null && elem.Sku == _kitEditando.Sku)
                    continue;
                cmbElemento.Items.Add($"{elem.Sku} - {elem.Nombre} ({(elem.EsKit ? "KIT" : "Producto")})");
            }
            if (cmbElemento.Items.Count > 0) cmbElemento.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error cargando elementos: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CargarDatos()
    {
        if (_kitEditando is null) return;

        txtSku.Text = _kitEditando.Sku;
        txtSku.ReadOnly = true;
        txtSku.BackColor = Color.FromArgb(230, 230, 230);
        txtNombre.Text = _kitEditando.Nombre;

        foreach (var c in _kitEditando.Componentes)
        {
            _componentes.Add((c.Elemento, c.Cantidad));
            dgvComponentes.Rows.Add(
                c.Elemento.Sku,
                c.Elemento.EsKit ? "KIT" : "Producto",
                c.Cantidad);
        }
    }

    private void BtnAgregar_Click(object? sender, EventArgs e)
    {
        if (cmbElemento.SelectedIndex < 0) return;

        var texto = cmbElemento.SelectedItem.ToString()!;
        var sku = texto.Split(" - ")[0];
        var cantidad = (int)nudCantidad.Value;

        // Verificar que no esté ya agregado
        if (_componentes.Any(c => c.Elemento.Sku == sku))
        {
            MessageBox.Show("El elemento ya está en la composición.", "Duplicado",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Buscar el elemento en el catálogo (simulado en memoria para el formulario)
        var elemento = EncontrarElemento(sku);
        if (elemento is null)
        {
            MessageBox.Show("Elemento no encontrado.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Anti-ciclo: verificar que el elemento no contenga al kit actual
        if (_kitEditando is not null && elemento.EsKit && elemento is KitCompuesto kitElem)
        {
            if (ContieneReferenciaCircular(kitElem, _kitEditando.Sku))
            {
                MessageBox.Show("Referencia circular detectada. No se puede agregar.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        _componentes.Add((elemento, cantidad));
        dgvComponentes.Rows.Add(elemento.Sku, elemento.EsKit ? "KIT" : "Producto", cantidad);
    }

    private IElementoCatalogo? EncontrarElemento(string sku)
    {
        // Búsqueda síncrona simplificada (en producción sería async)
        var task = _catalogo.ObtenerPorSkuAsync(sku);
        task.Wait();
        return task.Result;
    }

    private bool ContieneReferenciaCircular(KitCompuesto kit, string skuBuscado)
    {
        foreach (var c in kit.Componentes)
        {
            if (c.Elemento.Sku == skuBuscado) return true;
            if (c.Elemento is KitCompuesto k && ContieneReferenciaCircular(k, skuBuscado))
                return true;
        }
        return false;
    }

    private void BtnQuitar_Click(object? sender, EventArgs e)
    {
        if (dgvComponentes.CurrentRow is null) return;

        var index = dgvComponentes.CurrentRow.Index;
        if (index >= 0 && index < _componentes.Count)
        {
            _componentes.RemoveAt(index);
            dgvComponentes.Rows.RemoveAt(index);
        }
    }

    private async void BtnGuardar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSku.Text) ||
            string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            MessageBox.Show("Complete SKU y nombre.", "Validación",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_componentes.Count == 0)
        {
            MessageBox.Show("Agregue al menos un componente al kit.", "Validación",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            if (_kitEditando is null)
            {
                var kit = new KitCompuesto(txtSku.Text.Trim(), txtNombre.Text.Trim());
                foreach (var (elem, cant) in _componentes)
                    kit.AgregarComponente(elem, cant);

                await _catalogo.AgregarKitAsync(kit);
            }
            else
            {
                // Para editar, se requeriría lógica de actualización (simplificado aquí)
                MessageBox.Show("La edición de kits existentes requiere recrear la composición.",
                    "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            KitGuardado = true;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ReferenciaCircularException ex)
        {
            MessageBox.Show(ex.Message, "Error de composición",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}