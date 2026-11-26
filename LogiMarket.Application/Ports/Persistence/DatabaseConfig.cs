namespace LogiMarket.Application.Ports.Persistence;

public static class DatabaseConfig
{
    /// <summary>
    /// Ruta absoluta de la base de datos SQLite.
    /// Se usa la misma ruta desde cualquier working directory (VS, dotnet run, etc.)
    /// </summary>
    public static string DatabasePath
    {
        get
        {
            var carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "LogiMarket360");
            Directory.CreateDirectory(carpeta);
            return Path.Combine(carpeta, "logimarket.db");
        }
    }

    public static string ConnectionString => $"Data Source={DatabasePath}";
}