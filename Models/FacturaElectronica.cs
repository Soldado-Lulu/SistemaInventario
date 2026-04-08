using System;

namespace SistemaInventario.Models
{
    public class FacturaElectronica
    {
        public int Id { get; set; }

        public int VentaId { get; set; }
        public Venta? Venta { get; set; }

        // Número interno de factura
        public string NumeroFactura { get; set; } = string.Empty;

        // Datos fiscales devueltos o usados por SIN
        public string? Cuf { get; set; }
        public string? Cufd { get; set; }

        // Estado interno del proceso fiscal
        // Ejemplo: Pendiente, Generada, Enviada, Validada, Rechazada, Anulada
        public string Estado { get; set; } = "Pendiente";

        public DateTime FechaEmision { get; set; } = DateTime.Now;

        // XML y datos técnicos
        public string? XmlGenerado { get; set; }
        public string? XmlFirmado { get; set; }
        public string? HashArchivo { get; set; }
        public string? CodigoRecepcion { get; set; }

        // Respuesta o mensaje del proceso fiscal
        public string? MensajeRespuesta { get; set; }

        // Control adicional
        public bool EnContingencia { get; set; } = false;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}