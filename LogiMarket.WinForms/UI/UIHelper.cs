using System.Drawing;
using System.Windows.Forms;

namespace LogiMarket.WinForms.UI;

/// <summary>
/// Helper para estilos visuales consistentes en toda la aplicación.
/// </summary>
public static class UIHelper
{
    // --- Paleta de colores ---
    public static readonly Color ColorPrimario = Color.FromArgb(30, 120, 200);
    public static readonly Color ColorSecundario = Color.FromArgb(40, 160, 80);
    public static readonly Color ColorPeligro = Color.FromArgb(200, 60, 60);
    public static readonly Color ColorAdvertencia = Color.FromArgb(240, 160, 40);
    public static readonly Color ColorInfo = Color.FromArgb(120, 80, 180);
    public static readonly Color ColorGrisClaro = Color.FromArgb(245, 247, 250);
    public static readonly Color ColorGrisMedio = Color.FromArgb(230, 230, 230);
    public static readonly Color ColorTexto = Color.FromArgb(50, 50, 50);
    public static readonly Color ColorTextoClaro = Color.FromArgb(120, 120, 120);

    // --- Fuentes ---
    public static readonly Font FuenteTitulo = new Font("Segoe UI", 14, FontStyle.Bold);
    public static readonly Font FuenteSubtitulo = new Font("Segoe UI", 11, FontStyle.Bold);
    public static readonly Font FuenteNormal = new Font("Segoe UI", 9);
    public static readonly Font FuenteNormalNegrita = new Font("Segoe UI", 9, FontStyle.Bold);
    public static readonly Font FuenteGrande = new Font("Segoe UI", 12, FontStyle.Bold);
    public static readonly Font FuenteMonospace = new Font("Consolas", 9);

    // --- Métodos de estilo ---
    public static Button CrearBoton(string texto, Color fondo, Color? textoColor = null, int ancho = 100, int alto = 32)
    {
        var btn = new Button
        {
            Text = texto,
            Width = ancho,
            Height = alto,
            BackColor = fondo,
            ForeColor = textoColor ?? Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = FuenteNormal,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(
            Math.Min(255, fondo.R + 20),
            Math.Min(255, fondo.G + 20),
            Math.Min(255, fondo.B + 20));
        return btn;
    }

    public static Button CrearBotonPrimario(string texto, int ancho = 100)
        => CrearBoton(texto, ColorPrimario, ancho: ancho);

    public static Button CrearBotonSecundario(string texto, int ancho = 100)
        => CrearBoton(texto, ColorSecundario, ancho: ancho);

    public static Button CrearBotonPeligro(string texto, int ancho = 100)
        => CrearBoton(texto, ColorPeligro, ancho: ancho);

    public static Button CrearBotonGris(string texto, int ancho = 100)
        => CrearBoton(texto, ColorGrisMedio, ColorTexto, ancho: ancho);

    public static Label CrearLabel(string texto, bool negrita = false, Color? color = null)
    {
        return new Label
        {
            Text = texto,
            AutoSize = true,
            Font = negrita ? FuenteNormalNegrita : FuenteNormal,
            ForeColor = color ?? ColorTexto
        };
    }

    public static TextBox CrearTextBox(int ancho = 200)
    {
        return new TextBox
        {
            Width = ancho,
            Font = FuenteNormal,
            Padding = new Padding(4)
        };
    }

    public static ComboBox CrearComboBox(int ancho = 200)
    {
        return new ComboBox
        {
            Width = ancho,
            Font = FuenteNormal,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
    }

    public static DataGridView CrearDataGridView()
    {
        return new DataGridView
        {
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            RowHeadersVisible = false,
            EnableHeadersVisualStyles = false,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ColorPrimario,
                ForeColor = Color.White,
                Font = FuenteNormalNegrita,
                Padding = new Padding(4)
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ColorGrisClaro
            },
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = ColorGrisMedio
        };
    }

    public static GroupBox CrearGroupBox(string titulo)
    {
        return new GroupBox
        {
            Text = titulo,
            Font = FuenteSubtitulo,
            ForeColor = ColorPrimario,
            Padding = new Padding(10)
        };
    }
}