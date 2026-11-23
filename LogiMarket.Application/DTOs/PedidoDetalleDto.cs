using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogiMarket.Application.DTOs;
public record PedidoDetalleDto(
    string Sku, 
    string Nombre, 
    bool EsKit, 
    int Cantidad, 
    decimal PrecioUnitario, 
    decimal SubtotalLinea);