using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.States;

public class BorradorState : IEstadoPedido
{
    public string Nombre => "BORRADOR";
    public void Validar(PedidoContext ctx) => ctx.CambiarEstado(new ValidadoState());
    public void Pagar(PedidoContext ctx) => throw new EstadoInvalidoException("Debe validarse antes de pagar.");
    public void Despachar(PedidoContext ctx) => throw new EstadoInvalidoException("Debe pagarse antes de despachar.");
    public void Entregar(PedidoContext ctx) => throw new EstadoInvalidoException("Secuencia inválida.");
    public void Cancelar(PedidoContext ctx) => ctx.CambiarEstado(new CanceladoState());
}