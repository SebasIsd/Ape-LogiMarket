using LogiMarket.Application.Ports.Documents;

namespace LogiMarket.Infrastructure.Documents;

public class ArchivoTxtSalida : ISalidaDocumento
{
    private readonly string _carpeta;
    public ArchivoTxtSalida(string carpeta) { _carpeta = carpeta; }
    public string NombreSalida => "ArchivoTXT";

    public async Task EmitirAsync(Documentosalida contenido, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_carpeta);
        var ruta = Path.Combine(_carpeta, $"{contenido.Titulo}.txt");
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"=== {contenido.Titulo} ===");
        sb.AppendLine($"Tipo: {contenido.Tipo}");
        sb.AppendLine($"Generado: {contenido.GeneradoEn:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine(new string('-', 40));
        foreach (var linea in contenido.Lineas) sb.AppendLine(linea);
        await File.WriteAllTextAsync(ruta, sb.ToString(), ct);
    }
}