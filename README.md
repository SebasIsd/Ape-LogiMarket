# LogiMarket 360

**Aplicación Empresarial de Escritorio para Gestión de Ventas y Logística**

Desarrollado en C# con .NET 9, Windows Forms, Clean Architecture y los 23 patrones de diseño GoF.

---

## 📋 Tabla de Contenidos

- [Descripción](#descripción)
- [Requisitos del Sistema](#requisitos-del-sistema)
- [Instalación](#instalación)
- [Ejecución](#ejecución)
- [Cuentas de Demostración](#cuentas-de-demostración)
- [Arquitectura](#arquitectura)
- [Patrones de Diseño Implementados](#patrones-de-diseño-implementados)
- [Pruebas Automatizadas](#pruebas-automatizadas)
- [Restablecer Datos de Prueba](#restablecer-datos-de-prueba)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Autor](#autor)

---

## Descripción

**LogiMarket 360** es una aplicación empresarial de escritorio que gestiona el ciclo completo de ventas de accesorios de computación y kits anidados. El sistema incluye:

- ✅ Catálogo de productos y kits con composición recursiva
- ✅ Gestión de clientes con categorías (Normal/VIP)
- ✅ Creación y validación de pedidos con Builder y Memento
- ✅ Cobro simulado con transacciones atómicas e idempotencia
- ✅ Despacho y entrega con Abstract Factory de envíos
- ✅ Devoluciones con Template Method y políticas por motivo
- ✅ Reportes y comprobantes con Bridge (VistaPrevia/TXT)
- ✅ Auditoría y eventos Outbox con Observer
- ✅ 23 patrones GoF implementados y demostrables
- ✅ Clean Architecture con 5 proyectos independientes

---

## Requisitos del Sistema

| Recurso | Versión Mínima | Verificación |
|---------|----------------|--------------|
| **Sistema Operativo** | Windows 10/11 | - |
| **.NET SDK** | > 8.0.x  | `dotnet --version` |
| **Visual Studio 2022** | 17.8+ (opcional) | Menú `Ayuda → Acerca de` |
| **Git** | 2.40+ | `git --version` |
| **EF Core Tools** | 8.0.11 | `dotnet ef --version` |

### Instalación de dependencias

```bash
# Verificar .NET SDK
dotnet --version

# Si no está instalado, descargar desde:
# https://dotnet.microsoft.com/download/dotnet/9.0

# Instalar EF Core Tools (si no está)
dotnet tool install --global dotnet-ef 
```

---

## Instalación

### 1. Clonar el repositorio

```bash
git clone https://github.com/SebasIsd/Ape-LogiMarket.git
cd LogiMarket360
```

### 2. Restaurar paquetes NuGet

```bash
dotnet restore LogiMarket360.sln
```

### 3. Compilar la solución

```bash
dotnet build LogiMarket360.sln --configuration Release
```

**Salida esperada:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## Ejecución

### Primera ejecución (crea la base de datos automáticamente)

```bash
# Opción 1: Desde Visual Studio
# Presionar F5 o Ctrl+F5

# Opción 2: Desde terminal
dotnet run --project LogiMarket.WinForms
```

La aplicación:
1. Aplica migraciones de EF Core automáticamente
2. Ejecuta el seed idempotente (5 productos, 2 kits, 4 usuarios)
3. Muestra la pantalla de login

### Ejecución manual de migraciones (opcional)

```bash
dotnet ef database update --project LogiMarket.Infrastructure --startup-project LogiMarket.WinForms
```

---

## Cuentas de Demostración

| Usuario | Contraseña | Rol | Permisos |
|---------|------------|-----|----------|
| `admin` | `admin123` | **ADMIN** | Todas las operaciones |
| `vendedor` | `vendedor123` | **VENTAS** | Clientes, pedidos, cobro |
| `bodega` | `bodega123` | **BODEGA** | Preparación, despacho, entrega |
| `consulta` | `consulta123` | **CONSULTA** | Solo lectura |

> ⚠️ **Seguridad:** Las contraseñas se almacenan con hash BCrypt. No existen en texto plano en la base de datos ni en el código fuente.

---

## Arquitectura

### Clean Architecture - 5 Proyectos

```
LogiMarket360.sln
├── LogiMarket.Domain          → Entidades puras, sin dependencias externas
├── LogiMarket.Application     → Casos de uso, puertos, CQRS básico
├── LogiMarket.Infrastructure  → EF Core 8 + SQLite, repositorios, adaptadores
├── LogiMarket.WinForms        → Formularios, Mediator, Composition Root DI
└── LogiMarket.Tests           → xUnit, pruebas unitarias y de integración
```

### Dirección de Dependencias

```
WinForms → Application → Domain
              ↓
         Infrastructure
```

- **Domain**: Cero dependencias externas (sin EF Core, sin UI)
- **Application**: Solo depende de Domain
- **Infrastructure**: Implementa puertos de Application
- **WinForms**: Composition Root con DI (referencia Infrastructure solo en Program.cs)
- **Tests**: Domain, Application, Infrastructure

---

## Patrones de Diseño Implementados

### Creacionales (5/5)
| Patrón | Implementación |
|--------|----------------|
| **Factory Method** | `NotificadorFactory` crea EMAIL/SMS |
| **Abstract Factory** | `EnvioFactory` produce ESTÁNDAR/EXPRÉS |
| **Builder** | `PedidoBuilder` construye pedidos válidos |
| **Prototype** | `Pedido.Clone()` copia profunda de borradores |
| **Singleton** | `MetricasSesion` thread-safe compartido |

### Estructurales (7/7)
| Patrón | Implementación |
|--------|----------------|
| **Adapter** | `PasarelaLegadaAdapter` adapta cobro legacy |
| **Bridge** | `Comprobante*` × `*Salida` (4 combinaciones) |
| **Composite** | `IElementoCatalogo` (Producto/Kit anidado) |
| **Decorator** | `AuditoriaPagoDecorator` registra trazas |
| **Facade** | `ConfirmacionPedidoFacade` orquesta cobro |
| **Flyweight** | `CatalogoVisualFlyweightFactory` comparte metadatos |
| **Proxy** | `PagoAutorizadoProxy` valida roles antes de cobrar |

### Comportamentales (11/11)
| Patrón | Implementación |
|--------|----------------|
| **Chain of Responsibility** | `PipelineValidacionPedido` (cliente→líneas→stock) |
| **Command** | `ConfirmarPedidoCommand` + `CommandInvoker` |
| **Interpreter** | Filtros: `ESTADO = PAGADO AND TOTAL >= 50` |
| **Iterator** | `KitPhysicalIterator` recorre kits anidados |
| **Mediator** | `FormularioPrincipalMediator` coordina vistas |
| **Memento** | Deshacer/Rehacer en borradores (hasta 5 estados) |
| **Observer** | `OutboxPublisher` + eventos de dominio |
| **State** | 6 estados del pedido con transiciones válidas |
| **Strategy** | Descuentos: MAYORISTA 15%, VIP 10%, NORMAL 0% |
| **Template Method** | `ProcesarDevolucionTemplate` (Defecto/Arrepentimiento) |
| **Visitor** | `DesgloseFisicoVisitor` calcula peso logístico |

**Total: 23/23 patrones GoF implementados y demostrables en ejecución real.**

---

## Pruebas Automatizadas

### Ejecutar todas las pruebas

```bash
dotnet test LogiMarket.Tests --verbosity normal
```

**Salida esperada:**
```
Test Run Successful.
Total tests: 26
     Passed: 26
     Failed: 0
    Skipped: 0
 Total time: 3.2847 Seconds
```

### Cobertura de pruebas

| Nivel | Cantidad | Aprobadas |
|-------|----------|-----------|
| Dominio (invariantes, cálculos) | 12 | 12 |
| Aplicación (estrategias, validación) | 8 | 8 |
| Integración (repositorios, transacciones) | 6 | 6 |
| **Total** | **26** | **26 (100%)** |

---

## Restablecer Datos de Prueba

Para volver al estado inicial (seed oficial):

```bash
# 1. Cerrar la aplicación

# 2. Eliminar la base de datos
Remove-Item .\LogiMarket.WinForms\bin\Debug\net9.0-windows\logimarket.db -ErrorAction SilentlyContinue

# 3. Volver a ejecutar (recrea la BD y el seed)
dotnet run --project LogiMarket.WinForms
```

---

## Estructura del Proyecto

```
LogiMarket360/
├── LogiMarket.Domain/
│   ├── Entities/           # Pedido, Cliente, Producto, Kit
│   ├── ValueObjects/       # Money (inmutable)
│   ├── States/             # 6 estados del pedido
│   ├── Builders/           # PedidoBuilder
│   ├── Iterators/          # KitPhysicalIterator
│   └── Visitors/           # DesgloseFisicoVisitor
│
├── LogiMarket.Application/
│   ├── Ports/              # Interfaces (IPasarelaPago, IRepositorio*)
│   ├── UseCases/           # Casos de uso
│   ├── Discount/           # Strategy (descuentos)
│   ├── Shipping/           # Abstract Factory (envíos)
│   ├── Validation/         # Chain of Responsibility
│   └── Events/             # Observer (eventos de dominio)
│
├── LogiMarket.Infrastructure/
│   ├── Persistence/        # EF Core, repositorios, migraciones
│   ├── Payments/           # Adapter, Decorator, Proxy
│   ├── Documents/          # Bridge (comprobantes)
│   ├── Outbox/             # Publisher, Processor
│   └── Security/           # BCrypt
│
├── LogiMarket.WinForms/
│   ├── Forms/              # 12 formularios funcionales
│   ├── Mediator/           # Coordinación de vistas
│   ├── UI/                 # UIHelper (estilos reutilizables)
│   └── Program.cs          # Composition Root DI
│
└── LogiMarket.Tests/
    ├── Domain/             # Pruebas de entidades
    ├── Application/        # Pruebas de casos de uso
    └── Integration/        # Pruebas con BD en memoria
```

---

## Autor

**Sebastian Israel Santana Duran**

Universidad Técnica de Ambato  
Facultad de Ingeniería en Sistemas, Electrónica e Industrial  
Carrera de Software  
Ciclo Académico: Julio – Diciembre 2026  
Asignatura: Patrones de Diseño de Software  
Responsable: Ing. Javier Vargas, Mg.

---

## Licencia

Proyecto académico desarrollado para la Guía Práctica 4 de la asignatura Patrones de Diseño de Software.

---