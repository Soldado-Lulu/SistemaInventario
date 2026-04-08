using SistemaInventario.Data;
using SistemaInventario.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaInventario.Services
{
    public class ClienteService
    {
        public List<Cliente> ObtenerTodos(bool soloActivos = true)
        {
            using var db = new AppDbContext();

            var query = db.Clientes.AsQueryable();

            if (soloActivos)
                query = query.Where(c => c.Activo);

            return query
                .OrderBy(c => c.NombreRazonSocial)
                .ToList();
        }

        public Cliente? ObtenerPorId(int id)
        {
            using var db = new AppDbContext();
            return db.Clientes.FirstOrDefault(c => c.Id == id);
        }

        public List<Cliente> Buscar(string texto)
        {
            using var db = new AppDbContext();

            if (string.IsNullOrWhiteSpace(texto))
                return ObtenerTodos();

            texto = texto.Trim().ToLower();

            return db.Clientes
                .Where(c =>
                    c.Activo &&
                    (
                        c.NombreRazonSocial.ToLower().Contains(texto) ||
                        c.NumeroDocumento.ToLower().Contains(texto)
                    )
                )
                .OrderBy(c => c.NombreRazonSocial)
                .ToList();
        }

        public Cliente RegistrarCliente(Cliente cliente)
        {
            using var db = new AppDbContext();

            ValidarCliente(cliente);

            string documentoNormalizado = cliente.NumeroDocumento.Trim();

            bool existe = db.Clientes.Any(c =>
                c.NumeroDocumento == documentoNormalizado &&
                c.CodigoTipoDocumentoIdentidad == cliente.CodigoTipoDocumentoIdentidad);

            if (existe)
                throw new Exception("Ya existe un cliente con ese número de documento.");

            cliente.NombreRazonSocial = cliente.NombreRazonSocial.Trim();
            cliente.NumeroDocumento = documentoNormalizado;
            cliente.Complemento = string.IsNullOrWhiteSpace(cliente.Complemento) ? null : cliente.Complemento.Trim();
            cliente.Correo = string.IsNullOrWhiteSpace(cliente.Correo) ? null : cliente.Correo.Trim();
            cliente.Telefono = string.IsNullOrWhiteSpace(cliente.Telefono) ? null : cliente.Telefono.Trim();
            cliente.Activo = true;

            db.Clientes.Add(cliente);
            db.SaveChanges();

            return cliente;
        }

        public void ActualizarCliente(Cliente cliente)
        {
            using var db = new AppDbContext();

            ValidarCliente(cliente);

            var actual = db.Clientes.FirstOrDefault(c => c.Id == cliente.Id);
            if (actual == null)
                throw new Exception("Cliente no encontrado.");

            string documentoNormalizado = cliente.NumeroDocumento.Trim();

            bool existe = db.Clientes.Any(c =>
                c.Id != cliente.Id &&
                c.NumeroDocumento == documentoNormalizado &&
                c.CodigoTipoDocumentoIdentidad == cliente.CodigoTipoDocumentoIdentidad);

            if (existe)
                throw new Exception("Ya existe otro cliente con ese número de documento.");

            actual.NombreRazonSocial = cliente.NombreRazonSocial.Trim();
            actual.NumeroDocumento = documentoNormalizado;
            actual.Complemento = string.IsNullOrWhiteSpace(cliente.Complemento) ? null : cliente.Complemento.Trim();
            actual.CodigoTipoDocumentoIdentidad = cliente.CodigoTipoDocumentoIdentidad;
            actual.Correo = string.IsNullOrWhiteSpace(cliente.Correo) ? null : cliente.Correo.Trim();
            actual.Telefono = string.IsNullOrWhiteSpace(cliente.Telefono) ? null : cliente.Telefono.Trim();
            actual.Activo = cliente.Activo;

            db.SaveChanges();
        }

        public void DesactivarCliente(int id)
        {
            using var db = new AppDbContext();

            var cliente = db.Clientes.FirstOrDefault(c => c.Id == id);
            if (cliente == null)
                throw new Exception("Cliente no encontrado.");

            cliente.Activo = false;
            db.SaveChanges();
        }

        private static void ValidarCliente(Cliente cliente)
        {
            if (cliente == null)
                throw new Exception("Los datos del cliente son obligatorios.");

            if (string.IsNullOrWhiteSpace(cliente.NombreRazonSocial))
                throw new Exception("El nombre o razón social del cliente es obligatorio.");

            if (string.IsNullOrWhiteSpace(cliente.NumeroDocumento))
                throw new Exception("El número de documento del cliente es obligatorio.");

            if (cliente.CodigoTipoDocumentoIdentidad <= 0)
                throw new Exception("El tipo de documento del cliente es inválido.");
        }
    }
}