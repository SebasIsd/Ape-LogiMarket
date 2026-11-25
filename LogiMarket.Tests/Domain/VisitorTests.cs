using LogiMarket.Domain.Entities.Catalog;
using LogiMarket.Domain.Visitors;
using LogiMarket.Domain.ValueObjects;
using Xunit;

namespace LogiMarket.Tests.Domain;

public class VisitorTests
{
    [Fact]
    public void Visitor_Calcula_Peso_KIT_TELETRABAJO()
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

        var visitor = new DesgloseFisicoVisitor();
        visitor.Calcular(kt, 1);

        Assert.Equal(1.00m, visitor.PesoTotal);
        Assert.Equal(3, visitor.Desglose.Count);
        Assert.Contains("P01", visitor.Desglose.Keys);
        Assert.Contains("P02", visitor.Desglose.Keys);
        Assert.Contains("P03", visitor.Desglose.Keys);
    }
}