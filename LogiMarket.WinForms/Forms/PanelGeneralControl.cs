using LogiMarket.Application.Metrics;
using LogiMarket.Application.Ports.Payments;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace LogiMarket.WinForms.Forms;

public class PanelGeneralControl : UserControl
{
    private readonly MetricasSesion _metricas;
    private readonly ISesionUsuario _sesion;
    private readonly Label _lblUsuario;
    private readonly Label _lblPedidos;
    private readonly Label _lblDespachos;
    private readonly Label _lblDevoluciones;
    private readonly Timer _timer;

    public PanelGeneralControl(MetricasSesion metricas, ISesionUsuario sesion)
    {
        _metricas = metricas;
        _sesion = sesion;

        _lblUsuario = new Label { Text = $"Usuario: {_sesion.Username} | Rol: {_sesion.Rol}", Location = new Point(10, 10), AutoSize = true };
        _lblPedidos = new Label { Text = "Pedidos confirmados: 0", Location = new Point(10, 50), AutoSize = true, Font = new Font(Font, FontStyle.Bold) };
        _lblDespachos = new Label { Text = "Despachos: 0", Location = new Point(10, 80), AutoSize = true };
        _lblDevoluciones = new Label { Text = "Devoluciones: 0", Location = new Point(10, 110), AutoSize = true };

        Controls.AddRange(new Control[] { _lblUsuario, _lblPedidos, _lblDespachos, _lblDevoluciones });

        _timer = new Timer { Interval = 1000 };
        _timer.Tick += (_, _) => ActualizarMetricas();
        _timer.Start();
    }

    private void ActualizarMetricas()
    {
        _lblPedidos.Text = $"Pedidos confirmados: {_metricas.PedidosConfirmados}";
        _lblDespachos.Text = $"Despachos: {_metricas.Despachos}";
        _lblDevoluciones.Text = $"Devoluciones: {_metricas.Devoluciones}";
    }
}