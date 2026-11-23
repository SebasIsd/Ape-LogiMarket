using LogiMarket.Domain.Enums;

namespace LogiMarket.Application.Ports.Payments;

public interface ISesionUsuario
{
    string Username { get; }
    RolUsuario Rol { get; }
    bool EstaAutenticado { get; }

    void IniciarSesion(string username, RolUsuario rol);
    void CerrarSesion();
}