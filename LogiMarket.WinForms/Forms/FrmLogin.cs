using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Application.Ports.Payments;
using LogiMarket.Infrastructure.Security;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public class FrmLogin : Form
{
    // --- Componentes con nomenclatura estándar ---
    private Label lbTitulo;
    private Label lbUsuario;
    private Label lbClave;
    private Label lbDemo;
    private TextBox txtUsuario;
    private TextBox txtClave;
    private CheckBox chkMostrar;
    private Button btnIngresar;
    private Button btnCancelar;
    private TableLayoutPanel pnlLayout;
    private FlowLayoutPanel pnlBotonera;

    private readonly IRepositorioUsuario _repoUsuario;
    private readonly ISesionUsuario _sesion;

    public FrmLogin(IRepositorioUsuario repoUsuario, ISesionUsuario sesion)
    {
        _repoUsuario = repoUsuario;
        _sesion = sesion;

        InicializarComponentes();
    }

    private void InicializarComponentes()
    {
        // --- Ventana ---
        Text = "LogiMarket 360 — Acceso";
        ClientSize = new Size(460, 360);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(245, 247, 250);

        // --- Layout principal (responsive) ---
        // Filas 0 a 8 = 9 filas en total
        pnlLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 9,
            Padding = new Padding(30, 20, 30, 20),
            BackColor = Color.White,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        };
        pnlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // 0: Título
        pnlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));  // 1: Espaciador
        pnlLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // 2: Usuario
        pnlLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // 3: Campo usuario
        pnlLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // 4: Clave
        pnlLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // 5: Campo clave
        pnlLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // 6: Mostrar
        pnlLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // 7: Demo
        pnlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));   // 8: Botones

        // --- lbTitulo ---
        lbTitulo = new Label
        {
            Text = "Iniciar Sesión",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 60, 90),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };
        pnlLayout.Controls.Add(lbTitulo, 0, 0);

        // --- lbUsuario ---
        lbUsuario = new Label
        {
            Text = "Usuario",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(80, 80, 80),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 2)
        };
        pnlLayout.Controls.Add(lbUsuario, 0, 2);

        // --- txtUsuario ---
        txtUsuario = new TextBox
        {
            Font = new Font("Segoe UI", 11),
            Dock = DockStyle.Fill,
            Height = 32,
            TabIndex = 0
        };
        pnlLayout.Controls.Add(txtUsuario, 0, 3);

        // --- lbClave ---
        lbClave = new Label
        {
            Text = "Contraseña",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(80, 80, 80),
            AutoSize = true,
            Margin = new Padding(0, 8, 0, 2)
        };
        pnlLayout.Controls.Add(lbClave, 0, 4);

        // --- txtClave ---
        txtClave = new TextBox
        {
            Font = new Font("Segoe UI", 11),
            Dock = DockStyle.Fill,
            Height = 32,
            UseSystemPasswordChar = true,
            TabIndex = 1
        };
        pnlLayout.Controls.Add(txtClave, 0, 5);

        // --- chkMostrar ---
        chkMostrar = new CheckBox
        {
            Text = "Mostrar contraseña",
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Margin = new Padding(0, 6, 0, 0),
            TabIndex = 2
        };
        chkMostrar.CheckedChanged += (_, _) =>
            txtClave.UseSystemPasswordChar = !chkMostrar.Checked;
        pnlLayout.Controls.Add(chkMostrar, 0, 6);

        // --- lbDemo ---
        lbDemo = new Label
        {
            Text = "Cuentas demo:  admin/admin123  •  vendedor/vendedor123\nbodega/bodega123  •  consulta/consulta123",
            Font = new Font("Segoe UI", 8, FontStyle.Italic),
            ForeColor = Color.FromArgb(120, 120, 120),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 10, 0, 10)
        };
        pnlLayout.Controls.Add(lbDemo, 0, 7);

        // --- pnlBotonera (centrado) ---
        pnlBotonera = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Anchor = AnchorStyles.None,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0)
        };

        btnCancelar = new Button
        {
            Text = "Cancelar",
            Width = 110,
            Height = 34,
            Font = new Font("Segoe UI", 9),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(230, 230, 230),
            Margin = new Padding(0, 0, 15, 0),
            TabIndex = 3
        };
        btnCancelar.FlatAppearance.BorderSize = 0;
        btnCancelar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        btnIngresar = new Button
        {
            Text = "Ingresar",
            Width = 110,
            Height = 34,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = Color.FromArgb(30, 120, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(15, 0, 0, 0),
            TabIndex = 4
        };
        btnIngresar.FlatAppearance.BorderSize = 0;
        btnIngresar.Click += BtnIngresar_Click;

        pnlBotonera.Controls.Add(btnCancelar);
        pnlBotonera.Controls.Add(btnIngresar);

        pnlLayout.Controls.Add(pnlBotonera, 0, 8);

        Controls.Add(pnlLayout);

        // --- Accesibilidad: Enter = Ingresar, Esc = Cancelar ---
        AcceptButton = btnIngresar;
        CancelButton = btnCancelar;

        // Foco inicial en txtUsuario
        ActiveControl = txtUsuario;
    }

    private async void BtnIngresar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtUsuario.Text) ||
            string.IsNullOrWhiteSpace(txtClave.Text))
        {
            MessageBox.Show("Ingrese usuario y contraseña.", "Validación",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtUsuario.Focus();
            return;
        }

        btnIngresar.Enabled = false;
        btnIngresar.Text = "Verificando...";
        try
        {
            var usuario = await _repoUsuario.ObtenerPorUsernameAsync(txtUsuario.Text.Trim());
            if (usuario is null || !usuario.Activo ||
                !PasswordHasher.Verify(txtClave.Text, usuario.PasswordHash))
            {
                MessageBox.Show("Credenciales inválidas o usuario inactivo.",
                    "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtClave.SelectAll();
                txtClave.Focus();
                return;
            }

            _sesion.IniciarSesion(usuario.Username, usuario.Rol);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error de sistema",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnIngresar.Enabled = true;
            btnIngresar.Text = "Ingresar";
        }
    }
}
