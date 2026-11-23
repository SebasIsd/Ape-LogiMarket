namespace LogiMarket.Domain.Entities;

public class Auditoria
{
    public Guid Id { get; private set; }
    public string Usuario { get; private set; } = default!;
    public string Accion { get; private set; } = default!;
    public string? Detalle { get; private set; }
    public DateTime Fecha { get; private set; }

    protected Auditoria() { }

    public Auditoria(string usuario, string accion, string? detalle)
    {
        Id = Guid.NewGuid();
        Usuario = usuario;
        Accion = accion;
        Detalle = detalle;
        Fecha = DateTime.UtcNow;
    }
}