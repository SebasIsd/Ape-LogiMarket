using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.States;

public class CanceladoState : IEstadoPedido
{
    public string Nombre => "CANCELADO";
    public void Validar(PedidoContext ctx) => throw new EstadoInvalidoException("Cancelado.");
    public void Pagar(PedidoContext ctx) => throw new EstadoInvalidoException("Cancelado.");
    public void Despachar(PedidoContext ctx) => throw new EstadoInvalidoException("Cancelado.");
    public void Entregar(PedidoContext ctx) => throw new EstadoInvalidoException("Cancelado.");
    public void Cancelar(PedidoContext ctx) => throw new EstadoInvalidoException("Ya cancelado.");
}