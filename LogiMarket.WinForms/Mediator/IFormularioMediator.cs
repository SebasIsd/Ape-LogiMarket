namespace LogiMarket.WinForms.Mediator;

public interface IFormularioMediator
{
    void Registrar(IViewRefrescable vista);
    void NotificarPedidoConfirmado(Guid pedidoId);
    void NotificarStockModificado();
    void NotificarCatalogoActualizado();
    void NotificarDespachoRegistrado(Guid pedidoId);
    void NotificarDevolucionProcesada(Guid pedidoId);
}