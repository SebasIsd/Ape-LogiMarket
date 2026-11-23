using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.States;

public class DespachadoState : IEstadoPedido
{
    public string Nombre => "DESPACHADO";
    public void Validar(PedidoContext ctx) => throw new EstadoInvalidoException("Secuencia inválida.");
    public void Pagar(PedidoContext ctx) => throw new EstadoInvalidoException("Secuencia inválida.");
    public void Despachar(PedidoContext ctx) => throw new EstadoInvalidoException("Ya despachado.");
    public void Entregar(PedidoContext ctx) => ctx.CambiarEstado(new EntregadoState());
    public void Cancelar(PedidoContext ctx) => throw new OperacionNoPermitidaException("No se puede cancelar.");
}