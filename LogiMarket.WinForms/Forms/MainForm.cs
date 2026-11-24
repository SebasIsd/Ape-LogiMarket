using LogiMarket.Application.Metrics;
using LogiMarket.Application.Ports.Payments;
using LogiMarket.WinForms.Mediator;
using LogiMarket.WinForms.UI;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

public partial class MainForm : Form, IViewRefrescable
{
    // --- Propiedades para logout/cierre ---
    public bool CerrarAppSolicitado { get; private set; } = false;
    public event Action? SolicitarCerrarApp;
    // --- Items del menú (con clave exacta de PermisosModulos) ---
    private readonly List<(string Modulo, string Texto, string Icono, Func<Control> Creador)> _itemsMenu = new();

    // --- Componentes ---
    private Panel pnlSidebar;
    private Panel pnlHeader;
    private Panel pnlContenido;
    private Label lbTituloApp;
    private Label lbUsuario;
    private Control _controlActivo;

    // --- Dependencias ---
    private readonly IFormularioMediator _mediator;
    private readonly ISesionUsuario _sesion;
    private readonly MetricasSesion _metricas;
    private readonly IServiceProvider _sp;

    public MainForm(
        IFormularioMediator mediator,
        ISesionUsuario sesion,
        MetricasSesion metricas,
        IServiceProvider sp)
    {
        _mediator = mediator;
        _sesion = sesion;
        _metricas = metricas;
        _sp = sp;
        _mediator.Registrar(this);

        InicializarComponentes();
        CargarMenuLateral();
        SeleccionarSeccion(0); // Panel general por defecto
    }

    private void InicializarComponentes()
    {
        Text = $"LogiMarket 360 — {_sesion.Username} ({_sesion.Rol})";
        Size = new Size(1400, 900);
        MinimumSize = new Size(1024, 680);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(245, 247, 250);

        // --- Header superior ---
        pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.White,
            Padding = new Padding(20, 0, 20, 0)
        };

        lbTituloApp = new Label
        {
            Text = "LOGIMARKET 360",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 60, 90),
            Location = new Point(20, 15),
            AutoSize = true
        };

        lbUsuario = new Label
        {
            Text = $"{_sesion.Username.ToUpper()} · Sesión local",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(120, 120, 120),
            AutoSize = true
        };
        lbUsuario.Location = new Point(Math.Max(20, pnlHeader.Width - lbUsuario.Width - 20), 20);
        lbUsuario.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        pnlHeader.Controls.AddRange(new Control[] { lbTituloApp, lbUsuario });

        // --- Sidebar izquierdo ---
        pnlSidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 220,
            BackColor = Color.FromArgb(30, 60, 90),
            Padding = new Padding(0, 10, 0, 10)
        };

        // --- Panel de contenido ---
        pnlContenido = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 247, 250),
            Padding = new Padding(20)
        };

        Controls.Add(pnlContenido);
        Controls.Add(pnlSidebar);
        Controls.Add(pnlHeader);

        pnlHeader.Resize += (_, _) =>
        {
            lbUsuario.Location = new Point(Math.Max(20, pnlHeader.Width - lbUsuario.Width - 20), 20);
        };
    }

    private void CargarMenuLateral()
    {
        pnlSidebar.Controls.Clear();
        _itemsMenu.Clear();

        // Definimos todos los módulos con su clave exacta de PermisosModulos
        var todosLosItems = new List<(string Modulo, string Texto, string Icono, Func<Control> Creador)>
    {
        ("Panel", "Panel general", "📊", () => (Control)new PanelGeneralControl(_metricas, _sesion, _sp)),
        ("Catálogo", "Catálogo y kits", "📦", () => (Control)_sp.GetRequiredService<FrmCatalogo>()),
        ("Clientes", "Clientes", "👥", () => (Control)_sp.GetRequiredService<FrmClientes>()),
        ("Carrito", "Carrito / borrador", "🛒", () => (Control)_sp.GetRequiredService<FrmCarrito>()),
        ("Pedidos", "Pedidos e historial", "📋", () => (Control)_sp.GetRequiredService<FrmPedidos>()),
        ("Cobro", "Cobro", "💳", () => (Control)_sp.GetRequiredService<FrmCobro>()),
        ("Despacho", "Despacho", "🚚", () => (Control)_sp.GetRequiredService<FrmDespacho>()),
        ("Devoluciones", "Devoluciones", "↩️", () => (Control)_sp.GetRequiredService<FrmDevoluciones>()),
        ("Inventario", "Inventario", "📊", () => (Control)_sp.GetRequiredService<FrmInventario>()),
        ("Reportes", "Reportes", "📈", () => (Control)_sp.GetRequiredService<FrmReportes>()),
        ("Auditoría", "Auditoría / pruebas", "🔍", () => (Control)_sp.GetRequiredService<FrmAuditoria>()),
        ("Demo", "LogiMarket · Escritorio", "ℹ️", () => (Control)_sp.GetRequiredService<FrmDemo>())
    };

        // Filtrar según el rol de la sesión actual
        foreach (var item in todosLosItems)
        {
            if (PermisosModulos.PuedeVer(_sesion.Rol, item.Modulo))
            {
                _itemsMenu.Add(item);
            }
        }

        var pnlBotones = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.Transparent
        };

        int y = 0;
        for (int i = 0; i < _itemsMenu.Count; i++)
        {
            var indiceCaptura = i;
            var item = _itemsMenu[i];
            var btn = new Button
            {
                Text = $"  {item.Icono}  {item.Texto}",
                Width = pnlSidebar.Width - 14,
                Height = 40,
                Location = new Point(5, y),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Tag = indiceCaptura,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 90, 120);
            btn.Click += (_, _) => SeleccionarSeccion(indiceCaptura);
            pnlBotones.Controls.Add(btn);
            y += 42;
        }

        // Botón cerrar sesión anclado en la parte inferior del sidebar
        var btnSalir = new Button
        {
            Text = "  🚪  Cerrar sesión",
            Dock = DockStyle.Bottom,
            Height = 40,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(200, 60, 60),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0),
            Margin = new Padding(5, 8, 5, 0)
        };
        btnSalir.FlatAppearance.BorderSize = 0;
        btnSalir.Click += BtnSalir_Click;

        pnlSidebar.Controls.Add(pnlBotones);
        pnlSidebar.Controls.Add(btnSalir);
    }

    private void SeleccionarSeccion(int indice)
    {
        // Resaltar botón seleccionado
        foreach (Control c in pnlSidebar.Controls)
        {
            if (c is Panel pnl)
            {
                foreach (Control hijo in pnl.Controls)
                {
                    if (hijo is Button btn && btn.Tag is int idx)
                    {
                        btn.BackColor = idx == indice
                            ? Color.FromArgb(50, 90, 120)
                            : Color.Transparent;
                    }
                }
            }
        }

        // Cargar el control correspondiente
        if (indice >= 0 && indice < _itemsMenu.Count)
        {
            pnlContenido.Controls.Clear();
            try
            {
                var control = _itemsMenu[indice].Creador();
                control.Dock = DockStyle.Fill;
                pnlContenido.Controls.Add(control);
                _controlActivo = control;
            }
            catch (Exception ex)
            {
                var lbl = new Label
                {
                    Text = $"Error al cargar: {ex.Message}",
                    Dock = DockStyle.Fill,
                    ForeColor = Color.Red,
                    Font = new Font("Segoe UI", 10)
                };
                pnlContenido.Controls.Add(lbl);
            }
        }
    }

    private void BtnSalir_Click(object? sender, EventArgs e)
    {
        if (MessageBox.Show("¿Cerrar sesión?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _sesion.CerrarSesion();
            CerrarAppSolicitado = false;
            Close();
        }
    }

    // IViewRefrescable
    public void OnPedidoConfirmado(Guid pedidoId) { }
    public void OnStockModificado() { }
    public void OnCatalogoActualizado() { }
    public void OnDespachoRegistrado(Guid pedidoId) { }
    public void OnDevolucionProcesada(Guid pedidoId) { }
}