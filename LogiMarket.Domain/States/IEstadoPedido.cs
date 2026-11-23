namespace LogiMarket.Domain.States;

public interface IEstadoPedido
{
    string Nombre { get; }
    void Validar(PedidoContext ctx);
    void Pagar(PedidoContext ctx);
    void Despachar(PedidoContext ctx);
    void Entregar(PedidoContext ctx);
    void Cancelar(PedidoContext ctx);
}

/// <summary>
/// Contexto mínimo que el estado necesita para realizar transiciones.
/// Mantiene el dominio desacoplado de la persistencia.
/// </summary>
public class PedidoContext
{
    public Entities.Pedido Pedido { get; }
    public Action<IEstadoPedido> CambiarEstado { get; }

    public PedidoContext(Entities.Pedido pedido, Action<IEstadoPedido> cambiarEstado)
    {
        Pedido = pedido;
        CambiarEstado = cambiarEstado;
    }
}