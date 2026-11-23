using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.States;

public class ValidadoState : IEstadoPedido
{
    public string Nombre => "VALIDADO";
    public void Validar(PedidoContext ctx) => throw new EstadoInvalidoException("Ya validado.");
    public void Pagar(PedidoContext ctx) => ctx.CambiarEstado(new PagadoState());
    public void Despachar(PedidoContext ctx) => throw new EstadoInvalidoException("Debe pagarse antes de despachar.");
    public void Entregar(PedidoContext ctx) => throw new EstadoInvalidoException("Secuencia inválida.");
    public void Cancelar(PedidoContext ctx) => ctx.CambiarEstado(new CanceladoState());
}