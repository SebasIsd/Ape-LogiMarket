using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogiMarket.WinForms.Mediator;
public interface IViewRefrescable
{
    void OnPedidoConfirmado(Guid pedidoId);
    void OnStockModificado();
    void OnCatalogoActualizado();
    void OnDespachoRegistrado(Guid pedidoId);
    void OnDevolucionProcesada(Guid pedidoId);
}