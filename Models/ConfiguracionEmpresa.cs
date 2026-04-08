using System;

namespace SistemaInventario.Models
{
    public class ConfiguracionEmpresa
    {
        public int Id { get; set; }

        // Datos del negocio
        public string RazonSocial { get; set; } = string.Empty;
        public string Nit { get; set; } = string.Empty;
        public string? NombreComercial { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Municipio { get; set; }
        public string? Correo { get; set; }

        // Configuración SIAT / SIN
        public string? CodigoSistema { get; set; }
        public string? TokenDelegado { get; set; }
        public string? Cuis { get; set; }
        public string? CufdActual { get; set; }

        // Generalmente 1=piloto, 2=producción o según tu manejo posterior
        public int CodigoAmbiente { get; set; } = 1;

        // Modalidad de facturación
        public int CodigoModalidad { get; set; } = 1;

        // Casa matriz = 0 por defecto
        public int CodigoSucursal { get; set; } = 0;

        // Puede ser null si aún no usas punto de venta
        public int? CodigoPuntoVenta { get; set; }

        // Control interno
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public DateTime? FechaUltimaSincronizacion { get; set; }
    }
}