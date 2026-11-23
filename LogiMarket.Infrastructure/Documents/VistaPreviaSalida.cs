using LogiMarket.Application.Ports.Documents;

namespace LogiMarket.Infrastructure.Documents;

public class VistaPreviaSalida : ISalidaDocumento
{
    public string NombreSalida => "VistaPrevia";
    public string? UltimoContenido { get; private set; }

    public Task EmitirAsync(Documentosalida contenido, CancellationToken ct = default)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"=== {contenido.Titulo} ===");
        sb.AppendLine($"Tipo: {contenido.Tipo}");
        sb.AppendLine($"Generado: {contenido.GeneradoEn:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine(new string('-', 40));
        foreach (var linea in contenido.Lineas) sb.AppendLine(linea);
        UltimoContenido = sb.ToString();
        return Task.CompletedTask;
    }
}