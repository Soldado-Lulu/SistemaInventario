using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using SistemaInventario.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaInventario.Services
{
    public class VentaService
    {
        public int RegistrarVenta(
            int? clienteId,
            string nombreCliente,
            string numeroDocumentoCliente,
            string? complementoCliente,
            string? correoCliente,
            string? celularCliente,
            int codigoTipoDocumentoIdentidad,
            bool tieneExencion,
            List<VentaDetalle> detalles,
            decimal descuentoAdicional = 0m,
            int codigoMetodoPago = 1,
            string? observacion = null)
        {
            using var db = new AppDbContext();
            using var transaction = db.Database.BeginTransaction();

            try
            {
                nombreCliente = (nombreCliente ?? string.Empty).Trim();
                numeroDocumentoCliente = (numeroDocumentoCliente ?? string.Empty).Trim();
                complementoCliente = string.IsNullOrWhiteSpace(complementoCliente) ? null : complementoCliente.Trim();
                correoCliente = string.IsNullOrWhiteSpace(correoCliente) ? null : correoCliente.Trim();
                celularCliente = string.IsNullOrWhiteSpace(celularCliente) ? null : celularCliente.Trim();
                observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();

                if (string.IsNullOrWhiteSpace(nombreCliente))
                    throw new Exception("Debes ingresar el nombre o razón social del comprador.");

                if (string.IsNullOrWhiteSpace(numeroDocumentoCliente))
                    throw new Exception("Debes ingresar el número de documento o NIT del comprador.");

                if (codigoTipoDocumentoIdentidad <= 0)
                    throw new Exception("El tipo de documento es inválido.");

                if (detalles == null || !detalles.Any())
                    throw new Exception("No hay detalles para registrar la venta.");

                if (clienteId.HasValue)
                {
                    var cliente = db.Clientes.FirstOrDefault(c => c.Id == clienteId.Value && c.Activo);
                    if (cliente == null)
                        throw new Exception("El cliente seleccionado no existe o está inactivo.");
                }

                foreach (var detalle in detalles)
                {
                    if (detalle.ProductoId <= 0)
                        throw new Exception("Existe un producto inválido en el detalle.");

                    if (detalle.Cantidad <= 0)
                        throw new Exception("La cantidad debe ser mayor a cero.");

                    if (detalle.PrecioUnitario <= 0)
                        throw new Exception("El precio unitario debe ser mayor a cero.");
                }

                decimal subtotalGeneral = detalles.Sum(d => d.Subtotal);

                if (descuentoAdicional < 0)
                    throw new Exception("El descuento adicional no puede ser negativo.");

                if (descuentoAdicional > subtotalGeneral)
                    throw new Exception("El descuento adicional no puede ser mayor al total de la venta.");

                decimal totalFinal = subtotalGeneral - descuentoAdicional;

                var venta = new Venta
                {
                    Fecha = DateTime.Now,
                    ClienteId = clienteId,
                    NombreCliente = nombreCliente,
                    NumeroDocumentoCliente = numeroDocumentoCliente,
                    ComplementoCliente = complementoCliente,
                    CorreoCliente = correoCliente,
                    CelularCliente = celularCliente,
                    CodigoTipoDocumentoIdentidad = codigoTipoDocumentoIdentidad,
                    TieneExencion = tieneExencion,
                    Total = totalFinal,
                    DescuentoAdicional = descuentoAdicional,
                    CodigoMetodoPago = codigoMetodoPago,
                    MontoTotalSujetoIva = totalFinal,
                    EstadoFiscal = "Pendiente",
                    Observacion = observacion
                };

                db.Ventas.Add(venta);
                db.SaveChanges();

                foreach (var detalle in detalles)
                {
                    var producto = db.Productos.FirstOrDefault(p => p.Id == detalle.ProductoId);

                    if (producto == null)
                        throw new Exception("Producto no encontrado.");

                    if (producto.Stock < detalle.Cantidad)
                        throw new Exception($"Stock insuficiente para {producto.Nombre}.");

                    producto.Stock -= detalle.Cantidad;

                    var nuevoDetalle = new VentaDetalle
                    {
                        VentaId = venta.Id,
                        ProductoId = detalle.ProductoId,
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = detalle.PrecioUnitario,
                        Subtotal = detalle.Subtotal
                    };

                    db.VentaDetalles.Add(nuevoDetalle);
                }

                db.SaveChanges();
                transaction.Commit();

                return venta.Id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Venta> ObtenerVentas()
        {
            using var db = new AppDbContext();

            return db.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                .OrderByDescending(v => v.Fecha)
                .ToList();
        }

        public Venta? ObtenerVentaPorId(int id)
        {
            using var db = new AppDbContext();

            return db.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(v => v.FacturaElectronica)
                .FirstOrDefault(v => v.Id == id);
        }
    }
}