using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SistemaInventario.Services
{
    public static class CufHelper
    {
        public static string GenerarCufEjemplo(int ventaId, string nit, DateTime fecha)
        {
            string nitLimpio = SoloNumeros(nit).PadLeft(13, '0');
            string fechaStr = fecha.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
            string sucursal = "0000";
            string modalidad = "2";
            string emision = "1";
            string tipoFactura = "1";
            string tipoDocumentoSector = "01";
            string numeroFactura = ventaId.ToString().PadLeft(10, '0');
            string puntoVenta = "0000";

            string baseCuf = $"{nitLimpio}{fechaStr}{sucursal}{modalidad}{emision}{tipoFactura}{tipoDocumentoSector}{numeroFactura}{puntoVenta}";

            // Esto es temporal para desarrollo local.
            // Luego aquí se debe implementar el CUF real según algoritmo oficial.
            return baseCuf + "A1";
        }

        private static string SoloNumeros(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            return new string(valor.Where(char.IsDigit).ToArray());
        }
    }
}