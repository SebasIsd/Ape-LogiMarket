using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Application.Ports.Payments;
using LogiMarket.Domain.Enums;
using LogiMarket.Infrastructure.Security;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public partial class FrmLogin : Form
{
    private readonly IRepositorioUsuario _repoUsuario;
    private readonly ISesionUsuario _sesion;
    private readonly TextBox _txtUser;
    private readonly TextBox _txtPass;
    private readonly Button _btnLogin;
    private readonly CheckBox _chkVerPass;

    public FrmLogin(IRepositorioUsuario repoUsuario, ISesionUsuario sesion)
    {
        _repoUsuario = repoUsuario;
        _sesion = sesion;

        Text = "LogiMarket 360 — Acceso";
        Size = new Size(400, 250);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var lblUser = new Label { Text = "Usuario:", Location = new Point(20, 30), AutoSize = true };
        var lblPass = new Label { Text = "Contraseña:", Location = new Point(20, 70), AutoSize = true };

        _txtUser = new TextBox { Location = new Point(120, 27), Width = 240 };
        _txtPass = new TextBox { Location = new Point(120, 67), Width = 240, UseSystemPasswordChar = true };

        _chkVerPass = new CheckBox { Text = "Mostrar", Location = new Point(120, 100), AutoSize = true };
        _chkVerPass.CheckedChanged += (_, _) => _txtPass.UseSystemPasswordChar = !_chkVerPass.Checked;

        _btnLogin = new Button { Text = "Ingresar", Location = new Point(120, 140), Width = 100 };
        _btnLogin.Click += BtnLogin_Click;

        var lblCuentas = new Label
        {
            Text = "Cuentas demo: admin/admin123, vendedor/vendedor123, bodega/bodega123, consulta/consulta123",
            Location = new Point(20, 180),
            AutoSize = true,
            Font = new Font(Font, FontStyle.Italic)
        };

        Controls.AddRange(new Control[] { lblUser, lblPass, _txtUser, _txtPass, _chkVerPass, _btnLogin, lblCuentas });
        AcceptButton = _btnLogin;
    }

    private async void BtnLogin_Click(object? sender, EventArgs e)
    {
        _btnLogin.Enabled = false;
        try
        {
            var user = await _repoUsuario.ObtenerPorUsernameAsync(_txtUser.Text.Trim());
            if (user is null || !user.Activo || !PasswordHasher.Verify(_txtPass.Text, user.PasswordHash))
            {
                MessageBox.Show("Credenciales inválidas o usuario inactivo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _sesion.IniciarSesion(user.Username, user.Rol);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnLogin.Enabled = true;
        }
    }
}