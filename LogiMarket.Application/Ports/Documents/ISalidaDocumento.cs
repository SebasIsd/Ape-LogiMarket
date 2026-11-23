namespace LogiMarket.Application.Ports.Documents;

public interface ISalidaDocumento
{
    string NombreSalida { get; }
    Task EmitirAsync(Documentosalida contenido, CancellationToken ct = default);
}

public record Documentosalida(string Titulo, string Tipo, IReadOnlyList<string> Lineas, DateTime GeneradoEn);