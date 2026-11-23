using LogiMarket.Domain.Entities.Catalog;

namespace LogiMarket.Domain.Iterators;

/// <summary>
/// Iterador propio que recorre los productos físicos dentro de un kit anidado
/// sin exponer la colección interna. Propaga cantidades de forma determinista.
/// </summary>
public class KitPhysicalIterator
{
    private readonly IElementoCatalogo _raiz;
    private readonly int _cantidad;

    public KitPhysicalIterator(IElementoCatalogo raiz, int cantidad)
    {
        _raiz = raiz ?? throw new ArgumentNullException(nameof(raiz));
        if (cantidad <= 0)
            throw new Exceptions.DomainException("Cantidad inválida.");
        _cantidad = cantidad;
    }

    public IEnumerable<(ProductoSimple Producto, int Cantidad)> GetEnumerator()
    {
        var stack = new Stack<(IElementoCatalogo Elem, int Cant)>();
        stack.Push((_raiz, _cantidad));

        while (stack.Count > 0)
        {
            var (elem, cant) = stack.Pop();
            if (elem is ProductoSimple p)
            {
                yield return (p, cant);
            }
            else if (elem is KitCompuesto k)
            {
                // Empujamos en orden inverso para mantener recorrido determinista
                foreach (var componente in k.Componentes.Reverse())
                    stack.Push((componente.Elemento, componente.Cantidad * cant));
            }
        }
    }
}