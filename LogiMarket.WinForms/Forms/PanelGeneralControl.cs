using LogiMarket.Application.Metrics;
using LogiMarket.Application.Ports.Payments;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.WinForms.UI;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace LogiMarket.WinForms.Forms;

public class PanelGeneralControl : UserControl
{
    // --- Componentes ---
    private Label lbTitulo;
    private Label lbSubtitulo;

    // Tarjetas de métricas
    private Panel pnlTarjetaPedidos;
    private Panel pnlTarjetaDespacho;
    private Panel pnlTarjetaStock;
    private Panel pnlTarjetaVentas;

    // Tabla de pedidos recientes
    private DataGridView dgvPedidosRecientes;

    // Alertas
    private FlowLayoutPanel flpAlertas;

    private readonly MetricasSesion _metricas;
    private readonly ISesionUsuario _sesion;
    private readonly IServiceProvider _sp;
    private readonly Timer _timer;

    public PanelGeneralControl(MetricasSesion metricas, ISesionUsuario sesion, IServiceProvider sp)
    {
        _metricas = metricas;
        _sesion = sesion;
        _sp = sp;

        InicializarComponentes();
        Load += async (_, _) => await CargarDatosAsync();

        _timer = new Timer { Interval = 2000 };
        _timer.Tick += async (_, _) => await CargarDatosAsync();
        _timer.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InicializarComponentes()
    {
        BackColor = Color.FromArgb(245, 247, 250);
        Padding = new Padding(4);

        var raiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.FromArgb(245, 247, 250),
            Padding = new Padding(0)
        };
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 126));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // --- Fila 0: Título y subtítulo ---
        var pnlEncabezado = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, 12),
            Padding = new Padding(0)
        };

        lbTitulo = new Label
        {
            Text = "Panel general",
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 60, 90),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 2)
        };

        lbSubtitulo = new Label
        {
            Text = "Resumen de operaciones, pedidos y movimientos recientes",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(120, 120, 120),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 4)
        };

        pnlEncabezado.Controls.AddRange(new Control[] { lbTitulo, lbSubtitulo });
        raiz.Controls.Add(pnlEncabezado, 0, 0);

        // --- Fila 1: 4 Tarjetas de métricas ---
        var gridTarjetas = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 16),
            Padding = new Padding(0)
        };
        for (int i = 0; i < 4; i++)
            gridTarjetas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        gridTarjetas.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        pnlTarjetaPedidos = CrearTarjeta("Pedidos activos", "0", "📋", Color.FromArgb(30, 120, 200), new Padding(0, 0, 10, 0));
        pnlTarjetaDespacho = CrearTarjeta("Por despachar", "0", "🚚", Color.FromArgb(240, 160, 40), new Padding(5, 0, 5, 0));
        pnlTarjetaStock = CrearTarjeta("Stock bajo", "0", "⚠️", Color.FromArgb(200, 60, 60), new Padding(5, 0, 5, 0));
        pnlTarjetaVentas = CrearTarjeta("Ventas (USD)", "0.00", "💰", Color.FromArgb(40, 160, 80), new Padding(10, 0, 0, 0));

        gridTarjetas.Controls.Add(pnlTarjetaPedidos, 0, 0);
        gridTarjetas.Controls.Add(pnlTarjetaDespacho, 1, 0);
        gridTarjetas.Controls.Add(pnlTarjetaStock, 2, 0);
        gridTarjetas.Controls.Add(pnlTarjetaVentas, 3, 0);
        raiz.Controls.Add(gridTarjetas, 0, 1);

        // --- Fila 2: Tabla de pedidos recientes + Alertas ---
        var gridInferior = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        gridInferior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
        gridInferior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        gridInferior.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var grpPedidos = UIHelper.CrearGroupBox("Pedidos recientes");
        grpPedidos.Dock = DockStyle.Fill;
        grpPedidos.Margin = new Padding(0, 0, 10, 0);
        grpPedidos.Padding = new Padding(10, 18, 10, 10);

        dgvPedidosRecientes = UIHelper.CrearDataGridView();
        dgvPedidosRecientes.Dock = DockStyle.Fill;
        dgvPedidosRecientes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        var cId = dgvPedidosRecientes.Columns.Add("colId", "Pedido");
        var cCli = dgvPedidosRecientes.Columns.Add("colCliente", "Cliente");
        var cEst = dgvPedidosRecientes.Columns.Add("colEstado", "Estado");
        var cTot = dgvPedidosRecientes.Columns.Add("colTotal", "Total");
        dgvPedidosRecientes.Columns[cId].FillWeight = 20;
        dgvPedidosRecientes.Columns[cCli].FillWeight = 35;
        dgvPedidosRecientes.Columns[cEst].FillWeight = 25;
        dgvPedidosRecientes.Columns[cTot].FillWeight = 20;
        grpPedidos.Controls.Add(dgvPedidosRecientes);

        var grpAlertas = UIHelper.CrearGroupBox("Alertas");
        grpAlertas.Dock = DockStyle.Fill;
        grpAlertas.Margin = new Padding(10, 0, 0, 0);
        grpAlertas.Padding = new Padding(12, 22, 12, 12);

        flpAlertas = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Color.Transparent,
            Margin = new Padding(0),
            Padding = new Padding(4, 4, 4, 4)
        };
        grpAlertas.Controls.Add(flpAlertas);

        gridInferior.Controls.Add(grpPedidos, 0, 0);
        gridInferior.Controls.Add(grpAlertas, 1, 0);
        raiz.Controls.Add(gridInferior, 0, 2);

        Controls.Add(raiz);
    }

    private static Panel CrearTarjeta(string titulo, string valor, string icono, Color color, Padding margenExterno)
    {
        var contenedor = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = margenExterno,
            Padding = new Padding(0),
            BackColor = Color.Transparent
        };

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(12, 10, 12, 10)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var lbIcono = new Label
        {
            Text = icono,
            Font = new Font("Segoe UI", 20),
            AutoSize = true,
            Margin = new Padding(0, 2, 10, 0),
            Anchor = AnchorStyles.Left | AnchorStyles.Top
        };

        var lbTitulo = new Label
        {
            Text = titulo,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.FromArgb(110, 120, 130),
            AutoSize = true,
            Margin = new Padding(0, 2, 0, 2),
            Anchor = AnchorStyles.Left | AnchorStyles.Top
        };

        var lbValor = new Label
        {
            Text = valor,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = color,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 0),
            Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
            Tag = "valor"
        };

        layout.Controls.Add(lbIcono, 0, 0);
        layout.SetRowSpan(lbIcono, 2);
        layout.Controls.Add(lbTitulo, 1, 0);
        layout.Controls.Add(lbValor, 1, 1);

        card.Controls.Add(layout);
        contenedor.Controls.Add(card);
        return contenedor;
    }

    private async Task CargarDatosAsync()
    {
        try
        {
            using var scope = _sp.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IRepositorioPedido>();
            var catalogo = scope.ServiceProvider.GetRequiredService<IRepositorioCatalogo>();

            var pedidos = await repo.ListarAsync();

            // Métricas
            var activos = pedidos.Count(p => p.EstadoActual is "VALIDADO" or "PAGADO");
            var porDespachar = pedidos.Count(p => p.EstadoActual == "PAGADO");
            var ventas = pedidos.Where(p => p.EstadoActual is "PAGADO" or "DESPACHADO" or "ENTREGADO")
                                .Sum(p => p.Total.Amount);

            // Stock bajo
            var productos = await catalogo.ListarProductosAsync();
            var stockBajo = productos.Count(p => p.Stock <= 5);

            // Actualizar tarjetas
            ActualizarValor(pnlTarjetaPedidos, activos.ToString());
            ActualizarValor(pnlTarjetaDespacho, porDespachar.ToString("D2"));
            ActualizarValor(pnlTarjetaStock, stockBajo.ToString("D2"));
            ActualizarValor(pnlTarjetaVentas, ventas.ToString("0.00"));

            // Pedidos recientes (últimos 5)
            dgvPedidosRecientes.Rows.Clear();
            foreach (var p in pedidos.OrderByDescending(x => x.FechaCreacion).Take(5))
            {
                dgvPedidosRecientes.Rows.Add(
                    $"PED-{p.Id.ToString("N")[..3].ToUpper()}",
                    p.Cliente?.Nombre ?? "—",
                    p.EstadoActual,
                    $"USD {p.Total.Amount:0.00}");
            }

            // Alertas
            flpAlertas.Controls.Clear();
            bool hayAlertas = false;

            if (stockBajo > 0)
            {
                hayAlertas = true;
                flpAlertas.Controls.Add(new Label
                {
                    Text = $"• {stockBajo} producto(s) con stock bajo",
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.FromArgb(200, 60, 60),
                    AutoSize = true,
                    Margin = new Padding(0, 0, 0, 8)
                });
            }

            if (porDespachar > 0)
            {
                hayAlertas = true;
                flpAlertas.Controls.Add(new Label
                {
                    Text = $"• {porDespachar} pedido(s) pagado(s) por despachar",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(210, 130, 20),
                    AutoSize = true,
                    Margin = new Padding(0, 0, 0, 8)
                });
            }

            if (!hayAlertas)
            {
                flpAlertas.Controls.Add(new Label
                {
                    Text = "• Sin alertas pendientes",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(40, 160, 80),
                    AutoSize = true,
                    Margin = new Padding(0, 0, 0, 8)
                });
            }
        }
        catch (Exception)
        {
            // Silencioso: el panel se actualiza en segundo plano
        }
    }

    private static void ActualizarValor(Panel tarjeta, string nuevoValor)
    {
        var lbValor = tarjeta.Controls
            .OfType<Panel>().FirstOrDefault()?
            .Controls.OfType<TableLayoutPanel>().FirstOrDefault()?
            .Controls.OfType<Label>().FirstOrDefault(l => l.Tag?.ToString() == "valor");

        if (lbValor != null) lbValor.Text = nuevoValor;
    }
}