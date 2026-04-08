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
            int clienteId,
            List<VentaDetalle> detalles,
            decimal descuentoAdicional = 0m,
            int codigoMetodoPago = 1,
            string? observacion = null)
        {
            using var db = new AppDbContext();
            using var transaction = db.Database.BeginTransaction();

            try
            {
                if (clienteId <= 0)
                    throw new Exception("Debes seleccionar un cliente válido.");

                var cliente = db.Clientes.FirstOrDefault(c => c.Id == clienteId && c.Activo);
                if (cliente == null)
                    throw new Exception("El cliente seleccionado no existe o está inactivo.");

                if (detalles == null || !detalles.Any())
                    throw new Exception("No hay detalles para registrar la venta.");

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
                    Total = totalFinal,
                    DescuentoAdicional = descuentoAdicional,
                    CodigoMetodoPago = codigoMetodoPago,
                    MontoTotalSujetoIva = totalFinal,
                    EstadoFiscal = "Pendiente",
                    Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim()
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