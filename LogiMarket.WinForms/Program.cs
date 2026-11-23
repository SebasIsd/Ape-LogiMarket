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
using LogiMarket.Application.Validation;
using LogiMarket.Infrastructure.Catalog;
using LogiMarket.Infrastructure.Documents;
using LogiMarket.Infrastructure.Outbox;
using LogiMarket.Infrastructure.Payments;
using LogiMarket.Infrastructure.Persistence;
using LogiMarket.Infrastructure.Persistence.Repositories;
using LogiMarket.Infrastructure.Persistence.Seed;
using LogiMarket.WinForms.Forms;
using LogiMarket.WinForms.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        // Migraciones y seed al iniciar
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
            await AppSeeder.SeedAsync(db);
        }

        // Iniciar sesión primero
        using (var sessionScope = provider.CreateScope())
        {
            var loginForm = sessionScope.ServiceProvider.GetRequiredService<FrmLogin>();
            if (loginForm.ShowDialog() != DialogResult.OK)
            {
                System.Windows.Forms.Application.Exit();
                return;
            }
        }

        // Abrir MainForm
        using (var mainScope = provider.CreateScope())
        {
            System.Windows.Forms.Application.Run(mainScope.ServiceProvider.GetRequiredService<MainForm>());
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
        services.AddSingleton<INotificadorFactory, NotificadorFactory>();

        // --- Singleton de métricas ---
        services.AddSingleton<MetricasSesion>();

        // --- Flyweight de catálogo ---
        services.AddSingleton<CatalogoVisualFlyweightFactory>();

        // --- Pipeline de validación (Chain of Responsibility) ---
        services.AddScoped<IValidadorPedido, ClienteActivoValidator>();
        services.AddScoped<IValidadorPedido, LineasValidator>();
        services.AddScoped<IValidadorPedido, StockValidator>();
        services.AddScoped<PipelineValidacionPedido>(sp =>
            new PipelineValidacionPedido(sp.GetServices<IValidadorPedido>()));

        // --- Command Invoker ---
        services.AddScoped<ICommandInvoker, CommandInvoker>();

        // --- Observer (Outbox) ---
        services.AddScoped<IPublisher, OutboxPublisher>();
        services.AddScoped<OutboxProcessor>();

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