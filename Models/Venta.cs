using System;
using System.Collections.Generic;

namespace SistemaInventario.Models
{
    public class Venta
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public decimal Total { get; set; }

        // =========================
        // CLIENTE OPCIONAL (MAESTRO)
        // =========================
        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        // =========================
        // SNAPSHOT DEL COMPRADOR
        // =========================
        public string NombreCliente { get; set; } = string.Empty;

        public string NumeroDocumentoCliente { get; set; } = string.Empty;

        public string? ComplementoCliente { get; set; }

        public string? CorreoCliente { get; set; }

        public string? CelularCliente { get; set; }

        // 1 = CI, 5 = NIT, etc.
        public int CodigoTipoDocumentoIdentidad { get; set; } = 1;

        // =========================
        // FACTURACIÓN
        // =========================
        public decimal DescuentoAdicional { get; set; } = 0m;

        public int CodigoMetodoPago { get; set; } = 1;

        public decimal MontoTotalSujetoIva { get; set; }

        // Exención / excepción fiscal
        public bool TieneExencion { get; set; } = false;

        public string EstadoFiscal { get; set; } = "Pendiente";

        public string? Observacion { get; set; }

        // =========================
        // RELACIONES
        // =========================
        public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();

        public FacturaElectronica? FacturaElectronica { get; set; }
    }
}