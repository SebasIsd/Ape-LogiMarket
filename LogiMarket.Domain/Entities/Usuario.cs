using LogiMarket.Domain.Enums;
using LogiMarket.Domain.Exceptions;

namespace LogiMarket.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public RolUsuario Rol { get; private set; }
    public bool Activo { get; private set; } = true;

    protected Usuario() { }

    public Usuario(string username, string passwordHash, RolUsuario rol)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Username obligatorio.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("PasswordHash obligatorio.");

        Id = Guid.NewGuid();
        Username = username;
        PasswordHash = passwordHash;
        Rol = rol;
    }

    public void CambiarPasswordHash(string nuevoHash)
    {
        if (string.IsNullOrWhiteSpace(nuevoHash))
            throw new DomainException("Hash inválido.");
        PasswordHash = nuevoHash;
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}