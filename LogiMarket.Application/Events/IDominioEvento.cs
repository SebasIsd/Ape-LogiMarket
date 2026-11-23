namespace LogiMarket.Application.Events;
//patron aplicado, Observer, para notificar a los subscriptores de eventos de dominio
public interface IDominioEvento
{
    Guid EventoId { get; }
    DateTime OcurridoEn { get; }
    string Tipo { get; }
}

public record PedidoPagadoEvent(Guid EventoId, DateTime OcurridoEn, string Tipo, Guid PedidoId, decimal Total) : IDominioEvento;
public record DespachoRegistradoEvent(Guid EventoId, DateTime OcurridoEn, string Tipo, Guid PedidoId, string Guia) : IDominioEvento;
public record DevolucionAceptadaEvent(Guid EventoId, DateTime OcurridoEn, string Tipo, Guid PedidoId, decimal Reembolso) : IDominioEvento;