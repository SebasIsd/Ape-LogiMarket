using LogiMarket.Domain.Entities.Catalog;

namespace LogiMarket.Domain.Visitors;

/// <summary>
/// Visitor que recorre un elemento del catálogo y produce un desglose
/// de SKU finales con sus cantidades y el peso logístico total.
/// </summary>
public class DesgloseFisicoVisitor : ICatalogoVisitor
{
    private int _cantidadContexto;
    private readonly Dictionary<string, int> _desglose = new();
    private decimal _pesoTotal;

    public IReadOnlyDictionary<string, int> Desglose => _desglose;
    public decimal PesoTotal => _pesoTotal;

    public void Calcular(IElementoCatalogo elemento, int cantidad)
    {
        _cantidadContexto = cantidad;
        _desglose.Clear();
        _pesoTotal = 0m;
        elemento.Accept(this);
    }

    public void VisitProducto(ProductoSimple producto)
    {
        if (_desglose.TryGetValue(producto.Sku, out var actual))
            _desglose[producto.Sku] = actual + _cantidadContexto;
        else
            _desglose[producto.Sku] = _cantidadContexto;

        _pesoTotal += producto.PesoKg * _cantidadContexto;
    }

    public void VisitKit(KitCompuesto kit)
    {
        foreach (var c in kit.Componentes)
        {
            var visitorAnidado = new DesgloseFisicoVisitor();
            visitorAnidado.Calcular(c.Elemento, c.Cantidad * _cantidadContexto);

            foreach (var (sku, cant) in visitorAnidado.Desglose)
            {
                if (_desglose.TryGetValue(sku, out var actual))
                    _desglose[sku] = actual + cant;
                else
                    _desglose[sku] = cant;
            }
            _pesoTotal += visitorAnidado.PesoTotal;
        }
    }
}