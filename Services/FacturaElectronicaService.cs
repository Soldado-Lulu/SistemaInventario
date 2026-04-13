using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using SistemaInventario.Models;
using System;
using System.Globalization;
using System.Linq;

namespace SistemaInventario.Services
{
    public class FacturaElectronicaService
    {
        private readonly FacturaXmlBuilder _xmlBuilder = new();
        private readonly FacturaCompressionService _compressionService = new();
        private readonly FacturaHashService _hashService = new();

        public FacturaElectronica GenerarDesdeVenta(int ventaId)
        {
            using var db = new AppDbContext();

            var empresa = db.ConfiguracionesEmpresa
                .AsNoTracking()
                .FirstOrDefault(c => c.Activo);

            if (empresa == null)
                throw new Exception("No existe una configuración de empresa activa.");

            var venta = db.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(v => v.FacturaElectronica)
                .FirstOrDefault(v => v.Id == ventaId);

            if (venta == null)
                throw new Exception("La venta no existe.");

            if (venta.FacturaElectronica != null)
                throw new Exception("La venta ya tiene una factura electrónica generada.");

            string numeroFactura = GenerarNumeroFactura(venta.Id);
            string cuf = CufHelper.GenerarCufEjemplo(venta.Id, empresa.Nit, venta.Fecha);

            var factura = new FacturaElectronica
            {
                VentaId = venta.Id,
                NumeroFactura = numeroFactura,
                Cuf = cuf,
                Cufd = empresa.CufdActual,
                Estado = "Generada",
                FechaEmision = DateTime.Now,
                FechaRegistro = DateTime.Now
            };

            // =========================
            // 1) GENERAR XML
            // =========================
            string xml = _xmlBuilder.GenerarXml(empresa, venta, factura);

            // =========================
            // 2) COMPRIMIR GZIP
            // =========================
            byte[] gzipBytes = _compressionService.ComprimirXmlAGzipBytes(xml);
            string gzipBase64 = _compressionService.ConvertirBytesABase64(gzipBytes);

            // =========================
            // 3) GENERAR HASH SHA256
            // =========================
            string hashSha256 = _hashService.GenerarSha256Hex(gzipBytes);

            // =========================
            // GUARDAR RESULTADOS
            // =========================
            factura.XmlGenerado = xml;
            factura.XmlFirmado = gzipBase64; // temporalmente usamos este campo
            factura.HashArchivo = hashSha256;
            factura.MensajeRespuesta = "XML + GZIP + HASH generados localmente correctamente.";

            db.FacturasElectronicas.Add(factura);

            venta.EstadoFiscal = "Generada";

            db.SaveChanges();

            return factura;
        }

        public FacturaElectronica? ObtenerPorVentaId(int ventaId)
        {
            using var db = new AppDbContext();

            return db.FacturasElectronicas
                .AsNoTracking()
                .Include(f => f.Venta)
                .FirstOrDefault(f => f.VentaId == ventaId);
        }

        public string GenerarNumeroFactura(int ventaId)
        {
            return ventaId.ToString("D10", CultureInfo.InvariantCulture);
        }

        public string ObtenerGzipBase64PorVentaId(int ventaId)
        {
            using var db = new AppDbContext();

            var factura = db.FacturasElectronicas
                .AsNoTracking()
                .FirstOrDefault(f => f.VentaId == ventaId);

            if (factura == null)
                throw new Exception("No existe factura para la venta seleccionada.");

            return factura.XmlFirmado ?? string.Empty;
        }
    }
}