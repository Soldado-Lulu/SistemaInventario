using SistemaInventario.Data;
using SistemaInventario.Models;
using System;
using System.Linq;

namespace SistemaInventario.Services
{
    public class ConfiguracionEmpresaService
    {
        public ConfiguracionEmpresa Obtener()
        {
            using var db = new AppDbContext();

            var config = db.ConfiguracionesEmpresa.FirstOrDefault();

            if (config == null)
            {
                config = new ConfiguracionEmpresa
                {
                    RazonSocial = string.Empty,
                    Nit = string.Empty,
                    NombreComercial = string.Empty,
                    Direccion = string.Empty,
                    Telefono = string.Empty,
                    Municipio = string.Empty,
                    Correo = string.Empty,
                    CodigoSistema = string.Empty,
                    TokenDelegado = string.Empty,
                    Cuis = string.Empty,
                    CufdActual = string.Empty,
                    CodigoAmbiente = 1,
                    CodigoModalidad = 1,
                    CodigoSucursal = 0,
                    CodigoPuntoVenta = null,
                    Activo = true,
                    FechaRegistro = DateTime.Now,
                    FechaUltimaSincronizacion = null
                };
            }

            return config;
        }

        public void GuardarOActualizar(ConfiguracionEmpresa modelo)
        {
            using var db = new AppDbContext();

            Validar(modelo);

            var actual = db.ConfiguracionesEmpresa.FirstOrDefault();

            if (actual == null)
            {
                modelo.RazonSocial = modelo.RazonSocial.Trim();
                modelo.Nit = modelo.Nit.Trim();
                modelo.NombreComercial = Limpiar(modelo.NombreComercial);
                modelo.Direccion = Limpiar(modelo.Direccion);
                modelo.Telefono = Limpiar(modelo.Telefono);
                modelo.Municipio = Limpiar(modelo.Municipio);
                modelo.Correo = Limpiar(modelo.Correo);
                modelo.CodigoSistema = Limpiar(modelo.CodigoSistema);
                modelo.TokenDelegado = Limpiar(modelo.TokenDelegado);
                modelo.Cuis = Limpiar(modelo.Cuis);
                modelo.CufdActual = Limpiar(modelo.CufdActual);
                modelo.Activo = modelo.Activo;

                if (modelo.FechaRegistro == default)
                    modelo.FechaRegistro = DateTime.Now;

                db.ConfiguracionesEmpresa.Add(modelo);
            }
            else
            {
                actual.RazonSocial = modelo.RazonSocial.Trim();
                actual.Nit = modelo.Nit.Trim();
                actual.NombreComercial = Limpiar(modelo.NombreComercial);
                actual.Direccion = Limpiar(modelo.Direccion);
                actual.Telefono = Limpiar(modelo.Telefono);
                actual.Municipio = Limpiar(modelo.Municipio);
                actual.Correo = Limpiar(modelo.Correo);
                actual.CodigoSistema = Limpiar(modelo.CodigoSistema);
                actual.TokenDelegado = Limpiar(modelo.TokenDelegado);
                actual.Cuis = Limpiar(modelo.Cuis);
                actual.CufdActual = Limpiar(modelo.CufdActual);
                actual.CodigoAmbiente = modelo.CodigoAmbiente;
                actual.CodigoModalidad = modelo.CodigoModalidad;
                actual.CodigoSucursal = modelo.CodigoSucursal;
                actual.CodigoPuntoVenta = modelo.CodigoPuntoVenta;
                actual.Activo = modelo.Activo;
                actual.FechaUltimaSincronizacion = modelo.FechaUltimaSincronizacion;
            }

            db.SaveChanges();
        }

        private static void Validar(ConfiguracionEmpresa modelo)
        {
            if (modelo == null)
                throw new Exception("La configuración de empresa es obligatoria.");

            if (string.IsNullOrWhiteSpace(modelo.RazonSocial))
                throw new Exception("La razón social es obligatoria.");

            if (string.IsNullOrWhiteSpace(modelo.Nit))
                throw new Exception("El NIT es obligatorio.");

            if (modelo.CodigoAmbiente <= 0)
                throw new Exception("El código de ambiente es inválido.");

            if (modelo.CodigoModalidad <= 0)
                throw new Exception("El código de modalidad es inválido.");

            if (modelo.CodigoSucursal < 0)
                throw new Exception("El código de sucursal no puede ser negativo.");
        }

        private static string? Limpiar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}