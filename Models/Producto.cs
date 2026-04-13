using System;

namespace SistemaInventario.Models
{
    public class Producto
    {
        public int Id { get; set; }

        // Datos comerciales
        public string Nombre { get; set; } = string.Empty;
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        // =========================
        // HOMOLOGACIÓN SIN
        // =========================

        // Código de actividad económica (CAEB) del producto/servicio
        public string? CodigoActividadEconomica { get; set; }

        // Código del producto homologado del SIN
        public string? CodigoProductoSin { get; set; }

        // Código de unidad de medida del SIN
        public int? UnidadMedidaSin { get; set; }

        // Para distinguir productos de servicios
        public bool EsServicio { get; set; } = false;

        // Control interno
        public bool TieneFechaVencimiento { get; set; } = false;
    }
}