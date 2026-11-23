namespace LogiMarket.Domain.Entities;

public class EventoOutbox
{
    public Guid Id { get; private set; }
    public string TipoEvento { get; private set; } = default!;
    public string Payload { get; private set; } = default!;
    public DateTime CreadoEn { get; private set; }
    public bool Procesado { get; private set; }
    public DateTime? ProcesadoEn { get; private set; }
    public int Reintentos { get; private set; }

    protected EventoOutbox() { }

    public EventoOutbox(string tipoEvento, string payload)
    {
        if (string.IsNullOrWhiteSpace(tipoEvento)) throw new Exceptions.DomainException("Tipo obligatorio.");
        if (string.IsNullOrWhiteSpace(payload)) throw new Exceptions.DomainException("Payload obligatorio.");

        Id = Guid.NewGuid();
        TipoEvento = tipoEvento;
        Payload = payload;
        CreadoEn = DateTime.UtcNow;
    }

    public void MarcarProcesado()
    {
        Procesado = true;
        ProcesadoEn = DateTime.UtcNow;
    }

    public void IncrementarReintento() => Reintentos++;
}