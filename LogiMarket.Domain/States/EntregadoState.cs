using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.States;

public class EntregadoState : IEstadoPedido
{
    public string Nombre => "ENTREGADO";
    public void Validar(PedidoContext ctx) => throw new EstadoInvalidoException("Secuencia inválida.");
    public void Pagar(PedidoContext ctx) => throw new EstadoInvalidoException("Secuencia inválida.");
    public void Despachar(PedidoContext ctx) => throw new EstadoInvalidoException("Secuencia inválida.");
    public void Entregar(PedidoContext ctx) => throw new EstadoInvalidoException("Ya entregado.");
    public void Cancelar(PedidoContext ctx) => throw new OperacionNoPermitidaException("No se puede cancelar.");
}