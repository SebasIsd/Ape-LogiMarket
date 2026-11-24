using LogiMarket.Domain.Entities;
using LogiMarket.WinForms.UI;
using System.Windows.Forms;

namespace LogiMarket.WinForms.Forms;

/// <summary>Ventana modal con el detalle completo de un pedido: cliente, qué compró, totales, pago, despacho y devolución.</summary>
public class FrmDetallePedido : Form
{
    public FrmDetallePedido(Pedido p)
    {
        var corto = p.Id.ToString("N")[..8].ToUpperInvariant();
        Text = $"Detalle del pedido #{corto}";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(860, 680);
        MinimumSize = new Size(760, 560);
        BackColor = UIHelper.ColorGrisClaro;
        Font = UIHelper.FuenteNormal;

        var raiz = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(12) };
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        // --- Datos generales ---
        var grpGeneral = UIHelper.CrearGroupBox("Datos del pedido");
        grpGeneral.Dock = DockStyle.Top;
        grpGeneral.AutoSize = true;
        var tg = CrearTabla(4);
        Fila(tg, "ID:", corto, "Estado:", p.EstadoActual, destacarDerecha: true);
        Fila(tg, "Cliente:", p.Cliente?.Nombre ?? "—", "Identificación:", p.Cliente?.Identificacion ?? "—");
        Fila(tg, "Categoría:", p.Cliente?.Categoria.ToString() ?? "—", "Correo:", p.Cliente?.Correo ?? "—");
        Fila(tg, "Fecha:", p.FechaCreacion.ToLocalTime().ToString("dd/MM/yyyy HH:mm"), "Tipo de envío:", p.TipoEnvio);
        grpGeneral.Controls.Add(tg);
        raiz.Controls.Add(grpGeneral, 0, 0);

        // --- Productos comprados ---
        var grpLineas = UIHelper.CrearGroupBox($"Productos comprados ({p.Detalles.Count} línea(s), {p.UnidadesFisicasFinales()} unidad(es))");
        grpLineas.Dock = DockStyle.Fill;
        var dgv = UIHelper.CrearDataGridView();
        dgv.Dock = DockStyle.Fill;
        dgv.Columns.Add("colSku", "SKU");
        dgv.Columns.Add("colProd", "Producto / Kit");
        dgv.Columns.Add("colTipo", "Tipo");
        dgv.Columns.Add("colCant", "Cant.");
        dgv.Columns.Add("colPu", "P. unitario");
        dgv.Columns.Add("colSub", "Subtotal");
        foreach (var d in p.Detalles)
            dgv.Rows.Add(d.Sku, d.Nombre, d.EsKit ? "Kit" : "Producto", d.Cantidad,
                $"$ {d.PrecioUnitarioInstantanea.Amount:0.00}", $"$ {d.SubtotalLinea.Amount:0.00}");
        if (p.Detalles.Count == 0) dgv.Rows.Add("—", "Sin líneas registradas", "", "", "", "");
        grpLineas.Controls.Add(dgv);
        raiz.Controls.Add(grpLineas, 0, 1);

        // --- Totales, pago, despacho, devolución ---
        var grpResumen = UIHelper.CrearGroupBox("Resumen económico y seguimiento");
        grpResumen.Dock = DockStyle.Top;
        grpResumen.AutoSize = true;
        var tr = CrearTabla(4);
        Fila(tr, "Subtotal:", Dinero(p.Subtotal.Amount), "Descuento:", $"{Dinero(p.Descuento.Amount)} ({p.EstrategiaDescuentoAplicada})");
        Fila(tr, "Tarifa envío:", Dinero(p.TarifaEnvio.Amount), "TOTAL:", Dinero(p.Total.Amount), destacarDerecha: true);

        if (p.Pago is not null)
            Fila(tr, "Pago:", $"{p.Pago.Referencia} ({p.Pago.Proveedor})", "Pagado el:", p.Pago.Fecha.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
        else
            Fila(tr, "Pago:", "Sin pago registrado", "", "");

        if (p.Despacho is not null)
            Fila(tr, "Despacho:", $"Guía {p.Despacho.Guia} · {p.Despacho.TipoEnvio}", "Entrega estimada:", p.Despacho.FechaEstimadaEntrega.ToLocalTime().ToString("dd/MM/yyyy"));
        else
            Fila(tr, "Despacho:", "Sin despachar", "", "");

        if (p.FechaEntrega is not null)
            Fila(tr, "Entregado el:", p.FechaEntrega.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm"), "", "");

        if (p.Devolucion is not null)
            Fila(tr, "Devolución:", $"{p.Devolucion.Motivo} · {(p.Devolucion.Aceptada ? "ACEPTADA" : "RECHAZADA")}",
                "Reembolso:", Dinero(p.Devolucion.Reembolso.Amount));
        grpResumen.Controls.Add(tr);
        raiz.Controls.Add(grpResumen, 0, 2);

        // --- Botón cerrar ---
        var btnCerrar = UIHelper.CrearBotonPrimario("Cerrar", 110);
        btnCerrar.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        btnCerrar.Click += (_, _) => Close();
        var pnlBoton = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 10, 0, 0) };
        pnlBoton.Controls.Add(btnCerrar);
        raiz.Controls.Add(pnlBoton, 0, 3);

        AcceptButton = btnCerrar;
        CancelButton = btnCerrar;
        Controls.Add(raiz);
    }

    private static string Dinero(decimal monto) => $"$ {monto:0.00}";

    private static TableLayoutPanel CrearTabla(int columnas)
    {
        var t = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = columnas, Padding = new Padding(10, 6, 10, 6) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        return t;
    }

    private static void Fila(TableLayoutPanel t, string etIzq, string valIzq, string etDer, string valDer, bool destacarDerecha = false)
    {
        int fila = t.RowCount++;
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.Controls.Add(UIHelper.CrearLabel(etIzq, negrita: true), 0, fila);
        t.Controls.Add(Valor(valIzq, false), 1, fila);
        t.Controls.Add(UIHelper.CrearLabel(etDer, negrita: true), 2, fila);
        t.Controls.Add(Valor(valDer, destacarDerecha), 3, fila);
    }

    private static Label Valor(string texto, bool destacar) => new()
    {
        Text = texto,
        AutoSize = true,
        Margin = new Padding(3, 4, 3, 4),
        MaximumSize = new Size(300, 0),
        Font = destacar ? UIHelper.FuenteSubtitulo : UIHelper.FuenteNormal,
        ForeColor = destacar ? UIHelper.ColorPrimario : UIHelper.ColorTexto
    };
}
