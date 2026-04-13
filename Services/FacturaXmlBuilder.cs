using SistemaInventario.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace SistemaInventario.Services
{
    public class FacturaXmlBuilder
    {
        public string GenerarXml(
            ConfiguracionEmpresa empresa,
            Venta venta,
            FacturaElectronica factura)
        {
            if (empresa == null)
                throw new Exception("No existe configuración de empresa.");

            if (venta == null)
                throw new Exception("La venta es obligatoria.");

            if (factura == null)
                throw new Exception("La factura electrónica es obligatoria.");

            if (venta.Detalles == null || !venta.Detalles.Any())
                throw new Exception("La venta no tiene detalle.");

            ValidarEmpresa(empresa);
            ValidarVenta(venta);

            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

            var xml = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement("facturaComputarizadaCompraVenta",
                    new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                    new XAttribute(xsi + "noNamespaceSchemaLocation", "facturaComputarizadaCompraVenta.xsd"),

                    new XElement("cabecera",
                        new XElement("nitEmisor", empresa.Nit),
                        new XElement("razonSocialEmisor", empresa.RazonSocial),
                        new XElement("municipio", empresa.Municipio ?? string.Empty),
                        new XElement("telefono", empresa.Telefono ?? string.Empty),
                        new XElement("numeroFactura", factura.NumeroFactura),
                        new XElement("cuf", factura.Cuf ?? string.Empty),
                        new XElement("cufd", factura.Cufd ?? string.Empty),
                        new XElement("codigoSucursal", empresa.CodigoSucursal),
                        new XElement("direccion", empresa.Direccion ?? string.Empty),
                        empresa.CodigoPuntoVenta.HasValue
                            ? new XElement("codigoPuntoVenta", empresa.CodigoPuntoVenta.Value)
                            : new XElement("codigoPuntoVenta", new XAttribute(xsi + "nil", "true")),
                        new XElement("fechaEmision", venta.Fecha.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture)),
                        new XElement("nombreRazonSocial", venta.NombreCliente),
                        new XElement("codigoTipoDocumentoIdentidad", venta.CodigoTipoDocumentoIdentidad),
                        new XElement("numeroDocumento", venta.NumeroDocumentoCliente),
                        string.IsNullOrWhiteSpace(venta.ComplementoCliente)
                            ? new XElement("complemento", new XAttribute(xsi + "nil", "true"))
                            : new XElement("complemento", venta.ComplementoCliente),
                        new XElement("codigoCliente", venta.NumeroDocumentoCliente),
                        new XElement("codigoMetodoPago", venta.CodigoMetodoPago),
                        new XElement("numeroTarjeta", new XAttribute(xsi + "nil", "true")),
                        new XElement("montoTotal", venta.Total.ToString("0.00", CultureInfo.InvariantCulture)),
                        new XElement("montoTotalSujetoIva", venta.MontoTotalSujetoIva.ToString("0.00", CultureInfo.InvariantCulture)),
                        new XElement("codigoMoneda", 1),
                        new XElement("tipoCambio", "1"),
                        new XElement("montoTotalMoneda", venta.Total.ToString("0.00", CultureInfo.InvariantCulture)),
                        new XElement("montoGiftCard", new XAttribute(xsi + "nil", "true")),
                        new XElement("descuentoAdicional", venta.DescuentoAdicional.ToString("0.00", CultureInfo.InvariantCulture)),
                        venta.TieneExencion
                            ? new XElement("codigoExcepcion", 1)
                            : new XElement("codigoExcepcion", new XAttribute(xsi + "nil", "true")),
                        new XElement("cafc", new XAttribute(xsi + "nil", "true")),
                        new XElement("leyenda", "Ley N° 453: Tienes derecho a recibir información sobre las características y contenidos de los servicios que utilices."),
                        new XElement("usuario", "sistema"),
                        new XElement("codigoDocumentoSector", 1)
                    ),

                    venta.Detalles.Select(d =>
                    {
                        if (d.Producto == null)
                            throw new Exception("Un detalle no tiene producto asociado.");

                        ValidarProductoHomologado(d.Producto);

                        return new XElement("detalle",
                            new XElement("actividadEconomica", d.Producto.CodigoActividadEconomica),
                            new XElement("codigoProductoSin", d.Producto.CodigoProductoSin),
                            new XElement("codigoProducto", d.Producto.Id),
                            new XElement("descripcion", d.Producto.Nombre),
                            new XElement("cantidad", d.Cantidad),
                            new XElement("unidadMedida", d.Producto.UnidadMedidaSin ?? 1),
                            new XElement("precioUnitario", d.PrecioUnitario.ToString("0.00", CultureInfo.InvariantCulture)),
                            new XElement("montoDescuento", "0.00"),
                            new XElement("subTotal", d.Subtotal.ToString("0.00", CultureInfo.InvariantCulture)),
                            new XElement("numeroSerie", new XAttribute(xsi + "nil", "true")),
                            new XElement("numeroImei", new XAttribute(xsi + "nil", "true"))
                        );
                    })
                )
            );

            return xml.ToString();
        }

        private void ValidarEmpresa(ConfiguracionEmpresa empresa)
        {
            if (string.IsNullOrWhiteSpace(empresa.Nit))
                throw new Exception("La empresa no tiene NIT configurado.");

            if (string.IsNullOrWhiteSpace(empresa.RazonSocial))
                throw new Exception("La empresa no tiene razón social configurada.");

            if (string.IsNullOrWhiteSpace(empresa.CufdActual))
                throw new Exception("La empresa no tiene CUFD configurado.");

            if (empresa.CodigoSucursal < 0)
                throw new Exception("El código de sucursal es inválido.");
        }

        private void ValidarVenta(Venta venta)
        {
            if (string.IsNullOrWhiteSpace(venta.NombreCliente))
                throw new Exception("La venta no tiene nombre del comprador.");

            if (string.IsNullOrWhiteSpace(venta.NumeroDocumentoCliente))
                throw new Exception("La venta no tiene documento del comprador.");
        }

        private void ValidarProductoHomologado(Producto producto)
        {
            if (string.IsNullOrWhiteSpace(producto.CodigoActividadEconomica))
                throw new Exception($"El producto '{producto.Nombre}' no tiene código de actividad económica.");

            if (string.IsNullOrWhiteSpace(producto.CodigoProductoSin))
                throw new Exception($"El producto '{producto.Nombre}' no tiene código producto SIN.");

            if (!producto.UnidadMedidaSin.HasValue)
                throw new Exception($"El producto '{producto.Nombre}' no tiene unidad de medida SIN.");
        }
    }
}