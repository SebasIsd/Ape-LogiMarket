using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Exceptions;
using LogiMarket.Domain.ValueObjects;
using Xunit;

namespace LogiMarket.Tests.Domain;

public class CompositeTests
{
    [Fact]
    public void KIT_OFICINA_Suma_P01_P02()
    {
        var p1 = new ProductoSimple("P01", "Mouse", new Money(20m), 8, 0.20m);
        var p2 = new ProductoSimple("P02", "Teclado", new Money(30m), 5, 0.70m);
        var kit = new KitCompuesto("KIT_OFICINA", "Kit Oficina");
        kit.AgregarComponente(p1, 1);
        kit.AgregarComponente(p2, 1);

        Assert.Equal(50.00m, kit.CalcularPrecioTotal(1).Amount);
    }

    [Fact]
    public void KIT_TELETRABAJO_Expande_P01_P02_P03()
    {
        var p1 = new ProductoSimple("P01", "Mouse", new Money(20m), 8, 0.20m);
        var p2 = new ProductoSimple("P02", "Teclado", new Money(30m), 5, 0.70m);
        var p3 = new ProductoSimple("P03", "USB", new Money(10m), 20, 0.10m);

        var ko = new KitCompuesto("KIT_OFICINA", "KO");
        ko.AgregarComponente(p1, 1);
        ko.AgregarComponente(p2, 1);

        var kt = new KitCompuesto("KIT_TELETRABAJO", "KT");
        kt.AgregarComponente(ko, 1);
        kt.AgregarComponente(p3, 1);

        var skus = kt.ExpandirSkusFinales(1).Select(x => x.Producto.Sku).ToList();
        Assert.Equal(new[] { "P01", "P02", "P03" }, skus);
        Assert.Equal(60.00m, kt.CalcularPrecioTotal(1).Amount);
    }

    [Fact]
    public void Kit_Circular_Lanza_Excepcion()
    {
        var p1 = new ProductoSimple("P01", "X", new Money(1m), 1, 0.1m);
        var k1 = new KitCompuesto("K1", "K1");
        k1.AgregarComponente(p1, 1);
        var k2 = new KitCompuesto("K2", "K2");
        k2.AgregarComponente(k1, 1);

        Assert.Throws<ReferenciaCircularException>(() => k1.AgregarComponente(k2, 1));
    }
}