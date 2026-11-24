using LogiMarket.Domain.Enums;

namespace LogiMarket.WinForms.UI;

/// <summary>Matriz de módulos (pestañas) visibles por rol. Ajusta aquí si cambian las reglas.</summary>
public static class PermisosModulos
{
    private static readonly Dictionary<RolUsuario, HashSet<string>> _permisos = new()
    {
        [RolUsuario.Admin] = new() { "Panel", "Catálogo", "Clientes", "Carrito", "Pedidos", "Cobro",
                                     "Despacho", "Devoluciones", "Inventario", "Reportes", "Auditoría", "Demo" },
        [RolUsuario.Ventas] = new() { "Panel", "Catálogo", "Clientes", "Carrito", "Pedidos", "Cobro",
                                      "Devoluciones", "Reportes" },
        [RolUsuario.Bodega] = new() { "Panel", "Pedidos", "Despacho", "Devoluciones", "Inventario" },
        [RolUsuario.Consulta] = new() { "Panel", "Pedidos", "Reportes" }
    };

    public static bool PuedeVer(RolUsuario rol, string modulo)
        => _permisos.TryGetValue(rol, out var modulos) && modulos.Contains(modulo);
}