using LogiMarket.Application.Commands;
using LogiMarket.Application.Discount;
using LogiMarket.Application.Events;
using LogiMarket.Application.Facades;
using LogiMarket.Application.Metrics;
using LogiMarket.Application.Notifications;
using LogiMarket.Application.Ports.Documents;
using LogiMarket.Application.Ports.Notifications;
using LogiMarket.Application.Ports.Payments;
using LogiMarket.Application.Ports.Persistence;
using LogiMarket.Application.Shipping;
using LogiMarket.Application.UseCases.Devoluciones;
using LogiMarket.Application.Validation;
using LogiMarket.Infrastructure.Catalog;
using LogiMarket.Infrastructure.Documents;
using LogiMarket.Infrastructure.Notifications;
using LogiMarket.Infrastructure.Outbox;
using LogiMarket.Infrastructure.Payments;
using LogiMarket.Infrastructure.Persistence;
using LogiMarket.Infrastructure.Persistence.Repositories;
using LogiMarket.Infrastructure.Persistence.Seed;
using LogiMarket.WinForms.Forms;
using LogiMarket.WinForms.Mediator;
using LogiMarket.WinForms.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;

namespace LogiMarket.WinForms;

internal static class Program
{
    [STAThread]
    static async Task Main()
    {
        ApplicationConfiguration.Initialize();

        var services = new ServiceCollection();
        ConfigureServices(services);

        var provider = services.BuildServiceProvider();

        // Migraciones y seed UNA SOLA VEZ al iniciar la app
        using (var scope = provider.CreateScope())
        {
            try
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();
                await AppSeeder.SeedAsync(db);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al inicializar la base de datos:\n{ex.Message}\n\n" +
                    "Ejecute 'dotnet ef database update' o elimine logimarket.db y reinicie.",
                    "LogiMarket 360 - Error de Inicio",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
        }

        // --- Bucle de sesión: permite re-login sin cerrar la app ---
        bool cerrarApp = false;

        while (!cerrarApp)
        {
            // 1. Mostrar Login
            using (var loginScope = provider.CreateScope())
            {
                var loginForm = loginScope.ServiceProvider.GetRequiredService<FrmLogin>();
                var resultado = loginForm.ShowDialog();

                if (resultado != DialogResult.OK)
                {
                    // Usuario canceló el login → cerrar app
                    cerrarApp = true;
                    break;
                }
            }

            // 2. Mostrar MainForm (bloqueante hasta que se cierre)
            using (var mainScope = provider.CreateScope())
            {
                var mainForm = mainScope.ServiceProvider.GetRequiredService<MainForm>();

                // Suscribirse al evento de "cerrar app" vs "logout"
                mainForm.SolicitarCerrarApp += () => cerrarApp = true;

                System.Windows.Forms.Application.Run(mainForm);

                // Si el MainForm se cerró por logout (no por cerrar app),
                // el bucle continúa y muestra el Login de nuevo.
                if (!mainForm.CerrarAppSolicitado)
                {
                    // Logout: continuar el bucle para mostrar Login
                    continue;
                }
                else
                {
                    // Cierre real de la app
                    cerrarApp = true;
                    break;
                }
            }
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // --- EF Core + SQLite ---
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=logimarket.db"));

        // --- Unit of Work ---
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // --- Repositorios ---
        services.AddScoped<IRepositorioPedido, RepositorioPedido>();
        services.AddScoped<IRepositorioCliente, RepositorioCliente>();
        services.AddScoped<IRepositorioCatalogo, RepositorioCatalogo>();
        services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
        services.AddScoped<IRepositorioOutbox, RepositorioOutbox>();
        services.AddScoped<IRepositorioMovimientos, RepositorioMovimientos>();
        services.AddScoped<IRepositorioAuditoria, RepositorioAuditoria>();

        // --- Logging ---
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));

        // --- Cadena de cobro: Proxy → Decorator → Adapter (legado) ---
        services.AddSingleton<PasarelaLegadaSimulada>();
        services.AddSingleton<PasarelaAlternativaSimulada>();
        services.AddScoped<IPasarelaPago>(sp =>
        {
            var legado = sp.GetRequiredService<PasarelaLegadaSimulada>();
            var sesion = sp.GetRequiredService<ISesionUsuario>();
            var log = sp.GetRequiredService<ILogger<AuditoriaPagoDecorator>>();
            var logProxy = sp.GetRequiredService<ILogger<PagoAutorizadoProxy>>();

            IPasarelaPago adapter = new PasarelaLegadaAdapter(legado);
            IPasarelaPago decorated = new AuditoriaPagoDecorator(adapter, log);
            IPasarelaPago proxied = new PagoAutorizadoProxy(decorated, sesion, logProxy);
            return proxied;
        });

        // --- Sesión de usuario (compartida) ---
        services.AddSingleton<ISesionUsuario, SesionUsuario>();

        // --- Estrategias de descuento (Strategy) ---
        services.AddSingleton<IDescuentoStrategy, Mayorista15Strategy>();
        services.AddSingleton<IDescuentoStrategy, Vip10Strategy>();
        services.AddSingleton<IDescuentoStrategy, SinDescuentoStrategy>();
        services.AddScoped<CalculadoraDescuentoContext>(sp =>
            new CalculadoraDescuentoContext(sp.GetServices<IDescuentoStrategy>()));

        // --- Abstract Factory de envíos ---
        services.AddSingleton<IEnvioAbstractFactory, EnvioFactory>();

        // --- Factory Method de notificadores ---
        services.AddSingleton<LogDeNotificaciones>();
        services.AddSingleton<INotificadorFactory, NotificadorFactory>();

        // --- Singleton de métricas ---
        services.AddSingleton(MetricasSesion.Instance);

        // --- Flyweight de catálogo ---
        services.AddSingleton<CatalogoVisualFlyweightFactory>();

        // --- Pipeline de validación (Chain of Responsibility) ---
        // ✅ Cadena de responsabilidad construida explícitamente en el orden correcto
        services.AddScoped<PipelineValidacionPedido>(sp =>
        {
            var catalogo = sp.GetRequiredService<IRepositorioCatalogo>();
            // Orden: Cliente activo → Líneas → Stock
            var stock = new StockValidator(catalogo);
            var lineas = new LineasValidator(stock);
            var cliente = new ClienteActivoValidator(lineas);
            return new PipelineValidacionPedido(cliente);
        });

        // --- Command Invoker ---
        services.AddScoped<ICommandInvoker, CommandInvoker>();

        // --- Observer (Outbox) ---
        services.AddScoped<IPublisher, OutboxPublisher>();
        services.AddScoped<OutboxProcessor>();

        // --- Carrito de compras (sesión) ---
        services.AddScoped<CarritoSesion>();

        // --- Bridge: salidas de documentos ---
        services.AddSingleton<ISalidaDocumento>(sp =>
        {
            var carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "LogiMarket360", "Comprobantes");
            return new ArchivoTxtSalida(carpeta);
        });
        services.AddSingleton<ISalidaDocumento, VistaPreviaSalida>();

        // --- Facade ---
        services.AddScoped<ConfirmacionPedidoFacade>();
        services.AddScoped<DevolucionService>();

        // --- Mediator ---
        services.AddScoped<IFormularioMediator, FormularioPrincipalMediator>();

        // --- Formularios ---
        services.AddTransient<FrmLogin>();
        services.AddTransient<MainForm>();
        services.AddTransient<FrmCatalogo>();
        services.AddTransient<FrmClientes>();
        services.AddTransient<FrmCarrito>();
        services.AddTransient<FrmPedidos>();
        services.AddTransient<FrmCobro>();
        services.AddTransient<FrmDespacho>();
        services.AddTransient<FrmDevoluciones>();
        services.AddTransient<FrmInventario>();
        services.AddTransient<FrmReportes>();
        services.AddTransient<FrmAuditoria>();
        services.AddTransient<FrmDemo>();
    }
}