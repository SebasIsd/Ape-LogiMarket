using LogiMarket.Domain.Entities.Catalog;

namespace LogiMarket.Domain.Visitors;

public interface ICatalogoVisitor
{
    void VisitProducto(ProductoSimple producto);
    void VisitKit(KitCompuesto kit);
}