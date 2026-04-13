using System;
using System.Security.Cryptography;

namespace SistemaInventario.Services
{
    public class FacturaHashService
    {
        public string GenerarSha256Hex(byte[] contenido)
        {
            if (contenido == null || contenido.Length == 0)
                throw new Exception("No existe contenido para generar el hash SHA256.");

            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(contenido);

            return ConvertirAHex(hashBytes);
        }

        private string ConvertirAHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToUpperInvariant();
        }
    }
}