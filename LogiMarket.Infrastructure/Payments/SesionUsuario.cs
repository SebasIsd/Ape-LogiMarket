using LogiMarket.Application.Ports.Payments;
using LogiMarket.Domain.Enums;

namespace LogiMarket.Infrastructure.Payments;

public class SesionUsuario : ISesionUsuario
{
    public string Username { get; private set; } = string.Empty;
    public RolUsuario Rol { get; private set; }
    public bool EstaAutenticado { get; private set; }

    public void IniciarSesion(string username, RolUsuario rol)
    {
        Username = username;
        Rol = rol;
        EstaAutenticado = true;
    }

    public void CerrarSesion()
    {
        Username = string.Empty;
        Rol = default;
        EstaAutenticado = false;
    }
}