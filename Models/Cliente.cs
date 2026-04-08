using System.Collections.Generic;

namespace SistemaInventario.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        // Nombre o razón social del cliente
        public string NombreRazonSocial { get; set; } = string.Empty;

        // CI / NIT / documento extranjero / etc.
        public string NumeroDocumento { get; set; } = string.Empty;

        // Complemento opcional
        public string? Complemento { get; set; }

        // 1 = CI, 5 = NIT, etc. Luego podrás ajustarlo con catálogo SIN.
        public int CodigoTipoDocumentoIdentidad { get; set; } = 1;

        public string? Correo { get; set; }

        public string? Telefono { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}