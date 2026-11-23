namespace LogiMarket.Application.DTOs;

public record ResultadoOperacion(bool Exito, string? Mensaje, object? Datos = null)
{
    public static ResultadoOperacion Ok(object? datos = null) => new(true, null, datos);
    public static ResultadoOperacion Error(string mensaje) => new(false, mensaje);
}