using LogiMarket.Application.Metrics;

namespace LogiMarket.WinForms.Mediator;

public class FormularioPrincipalMediator : IFormularioMediator
{
    private readonly MetricasSesion _metricas;
    private readonly List<IViewRefrescable> _vistas = new();

    public FormularioPrincipalMediator(MetricasSesion metricas)
    {
        _metricas = metricas;
    }

    public void Registrar(IViewRefrescable vista)
    {
        if (vista is not null && !_vistas.Contains(vista))
            _vistas.Add(vista);
    }

    public void NotificarPedidoConfirmado(Guid pedidoId)
    {
        _metricas.IncrementarPedidos();
        foreach (var v in _vistas) v.OnPedidoConfirmado(pedidoId);
    }

    public void NotificarStockModificado()
    {
        foreach (var v in _vistas) v.OnStockModificado();
    }

    public void NotificarCatalogoActualizado()
    {
        foreach (var v in _vistas) v.OnCatalogoActualizado();
    }

    public void NotificarDespachoRegistrado(Guid pedidoId)
    {
        _metricas.IncrementarDespachos();
        foreach (var v in _vistas) v.OnDespachoRegistrado(pedidoId);
    }

    public void NotificarDevolucionProcesada(Guid pedidoId)
    {
        _metricas.IncrementarDevoluciones();
        foreach (var v in _vistas) v.OnDevolucionProcesada(pedidoId);
    }
}