namespace LogiMarket.Application.Ports.Documents;

public interface IComprobante
{
    string Tipo { get; }
    Documentosalida GenerarContenido();
}