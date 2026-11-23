using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Entities;

public class Cliente
{
    public Guid Id { get; private set; }
    public string Identificacion { get; private set; } = default!;
    public string Nombre { get; private set; } = default!;
    public string Correo { get; private set; } = default!;
    public string Telefono { get; private set; } = string.Empty;
    public CategoriaCliente Categoria { get; private set; }
    public bool Activo { get; private set; } = true;
    public DateTime FechaCreacion { get; private set; }

    protected Cliente() { }

    public Cliente(string identificacion, string nombre, string correo, CategoriaCliente categoria)
    {
        if (string.IsNullOrWhiteSpace(identificacion))
            throw new DomainException("La identificación es obligatoria.");
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(correo))
            throw new DomainException("El correo es obligatorio.");

        Id = Guid.NewGuid();
        Identificacion = identificacion;
        Nombre = nombre;
        Correo = correo;
        Categoria = categoria;
        FechaCreacion = DateTime.UtcNow;
    }

    public void ActualizarDatos(string nombre, string correo, string telefono, CategoriaCliente categoria)
    {
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(correo))
            throw new DomainException("Nombre y correo son obligatorios.");
        Nombre = nombre;
        Correo = correo;
        Telefono = telefono ?? string.Empty;
        Categoria = categoria;
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}