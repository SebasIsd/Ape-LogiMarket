using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.ValueObjects;
using LogiMarket.WinForms.UI;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public class FrmNuevoProducto : Form
{
    // --- Componentes ---
    private Label lbTitulo;
    private Label lbSku;
    private TextBox txtSku;
    private Label lbNombre;
    private TextBox txtNombre;
    private Label lbPrecio;
    private TextBox txtPrecio;
    private Label lbStock;
    private NumericUpDown nudStock;
    private Label lbPeso;
    private TextBox txtPeso;
    private Button btnGuardar;
    private Button btnCancelar;
    private FlowLayoutPanel pnlBotonera;

    private readonly IRepositorioCatalogo _catalogo;
    private ProductoSimple? _productoEditando;
    public bool ProductoGuardado { get; private set; }

    public FrmNuevoProducto(IRepositorioCatalogo catalogo, ProductoSimple? productoEditando = null)
    {
        _catalogo = catalogo;
        _productoEditando = productoEditando;

        InicializarComponentes();
        CargarDatos();
    }

    private void InicializarComponentes()
    {
        Text = _productoEditando is null ? "Nuevo producto" : "Editar producto";
        ClientSize = new Size(450, 380);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(245, 247, 250);

        lbTitulo = new Label
        {
            Text = _productoEditando is null ? "Crear nuevo producto" : "Editar producto",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 60, 90),
            Location = new Point(20, 15),
            AutoSize = true
        };

        var pnlFormulario = new TableLayoutPanel
        {
            Location = new Point(20, 55),
            Size = new Size(410, 240),
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(0)
        };
        pnlFormulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        pnlFormulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 5; i++)
            pnlFormulario.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

        lbSku = UIHelper.CrearLabel("SKU:", negrita: true);
        txtSku = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };

        lbNombre = UIHelper.CrearLabel("Nombre:", negrita: true);
        txtNombre = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };

        lbPrecio = UIHelper.CrearLabel("Precio (USD):", negrita: true);
        txtPrecio = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };

        lbStock = UIHelper.CrearLabel("Stock:", negrita: true);
        nudStock = new NumericUpDown { Minimum = 0, Maximum = 9999, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };

        lbPeso = UIHelper.CrearLabel("Peso (kg):", negrita: true);
        txtPeso = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };

        pnlFormulario.Controls.Add(lbSku, 0, 0);
        pnlFormulario.Controls.Add(txtSku, 1, 0);
        pnlFormulario.Controls.Add(lbNombre, 0, 1);
        pnlFormulario.Controls.Add(txtNombre, 1, 1);
        pnlFormulario.Controls.Add(lbPrecio, 0, 2);
        pnlFormulario.Controls.Add(txtPrecio, 1, 2);
        pnlFormulario.Controls.Add(lbStock, 0, 3);
        pnlFormulario.Controls.Add(nudStock, 1, 3);
        pnlFormulario.Controls.Add(lbPeso, 0, 4);
        pnlFormulario.Controls.Add(txtPeso, 1, 4);

        pnlBotonera = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 50,
            Padding = new Padding(10, 10, 10, 5)
        };

        btnCancelar = UIHelper.CrearBotonGris("Cancelar", 100);
        btnCancelar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        btnGuardar = UIHelper.CrearBotonPrimario("Guardar", 100);
        btnGuardar.Click += BtnGuardar_Click;

        pnlBotonera.Controls.AddRange(new Control[] { btnCancelar, btnGuardar });

        Controls.Add(lbTitulo);
        Controls.Add(pnlFormulario);
        Controls.Add(pnlBotonera);

        AcceptButton = btnGuardar;
        CancelButton = btnCancelar;
    }

    private void CargarDatos()
    {
        if (_productoEditando is null) return;

        txtSku.Text = _productoEditando.Sku;
        txtSku.ReadOnly = true;
        txtSku.BackColor = Color.FromArgb(230, 230, 230);
        txtNombre.Text = _productoEditando.Nombre;
        txtPrecio.Text = _productoEditando.Precio.Amount.ToString("0.00");
        nudStock.Value = _productoEditando.Stock;
        txtPeso.Text = _productoEditando.PesoKg.ToString("0.00");
    }

    private async void BtnGuardar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSku.Text) ||
            string.IsNullOrWhiteSpace(txtNombre.Text) ||
            !decimal.TryParse(txtPrecio.Text.Replace(",", "."), out var precio) ||
            !decimal.TryParse(txtPeso.Text.Replace(",", "."), out var peso))
        {
            MessageBox.Show("Complete todos los campos correctamente.", "Validación",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            if (_productoEditando is null)
            {
                // Verificar SKU único
                var existente = await _catalogo.ObtenerPorSkuAsync(txtSku.Text.Trim());
                if (existente is not null)
                {
                    MessageBox.Show("Ya existe un producto/kit con ese SKU.", "Duplicado",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var producto = new ProductoSimple(
                    txtSku.Text.Trim(),
                    txtNombre.Text.Trim(),
                    new Money(precio),
                    (int)nudStock.Value,
                    peso);

                await _catalogo.AgregarProductoAsync(producto);
            }
            else
            {
                _productoEditando.ActualizarPrecio(new Money(precio));
                _productoEditando.AjustarStock((int)nudStock.Value - _productoEditando.Stock);
                // Actualizar peso (necesita setter o método)
                await _catalogo.ActualizarProductoAsync(_productoEditando);
            }

            ProductoGuardado = true;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}