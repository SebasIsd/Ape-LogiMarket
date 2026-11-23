using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.States;

public class PagadoState : IEstadoPedido
{
    public string Nombre => "PAGADO";
    public void Validar(PedidoContext ctx) => throw new EstadoInvalidoException("Ya pagado.");
    public void Pagar(PedidoContext ctx) => throw new EstadoInvalidoException("Ya pagado.");
    public void Despachar(PedidoContext ctx) => ctx.CambiarEstado(new DespachadoState());
    public void Entregar(PedidoContext ctx) => throw new EstadoInvalidoException("Debe despacharse primero.");
    public void Cancelar(PedidoContext ctx) => throw new OperacionNoPermitidaException("No se puede cancelar un pedido pagado.");
}