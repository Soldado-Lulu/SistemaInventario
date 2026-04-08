using SistemaInventario.Models;
using SistemaInventario.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SistemaInventario.Views
{
    public partial class VentasPage : Page
    {
        private readonly ProductoService _productoService = new();
        private readonly VentaService _ventaService = new();
        private readonly ClienteService _clienteService = new();

        private List<Producto> _productos = new();
        private List<Cliente> _clientes = new();
        private List<VentaDetalle> _detalleVenta = new();

        public VentasPage()
        {
            InitializeComponent();
            CargarProductos();
            CargarClientes();
            ActualizarGrid();
            LimpiarFormularioProducto();
            LimpiarFormularioClienteNuevo();
            LimpiarInfoClienteSeleccionado();
        }

        private void CargarProductos()
        {
            _productos = _productoService.ObtenerTodos()
                .Where(p => p.Stock > 0)
                .OrderBy(p => p.Nombre)
                .ToList();

            CbProductos.ItemsSource = null;
            CbProductos.ItemsSource = _productos;
        }

        private void CargarClientes()
        {
            _clientes = _clienteService.ObtenerTodos()
                .OrderBy(c => c.NombreRazonSocial)
                .ToList();

            CbClientes.ItemsSource = null;
            CbClientes.ItemsSource = _clientes;
            CbClientes.SelectedIndex = -1;
        }

        private void LimpiarFormularioProducto()
        {
            CbProductos.SelectedItem = null;
            TxtCantidad.Text = string.Empty;
            TxtPrecioUnitario.Text = string.Empty;
            TxtStockDisponible.Text = string.Empty;
            TxtSubtotal.Text = string.Empty;
        }

        private void LimpiarFormularioClienteNuevo()
        {
            TxtNombreClienteNuevo.Text = string.Empty;
            TxtDocumentoClienteNuevo.Text = string.Empty;
        }

        private void LimpiarInfoClienteSeleccionado()
        {
            TxtDocumentoCliente.Text = string.Empty;
            TxtCorreoCliente.Text = string.Empty;
            TxtClienteSeleccionado.Text = "Sin cliente";
            TxtTelefonoCliente.Text = "-";
        }

        private void ActualizarInfoProducto()
        {
            if (CbProductos.SelectedItem is not Producto producto)
            {
                TxtPrecioUnitario.Text = string.Empty;
                TxtStockDisponible.Text = string.Empty;
                TxtSubtotal.Text = string.Empty;
                return;
            }

            int cantidadYaAgregada = _detalleVenta
                .Where(d => d.ProductoId == producto.Id)
                .Sum(d => d.Cantidad);

            int stockDisponibleReal = producto.Stock - cantidadYaAgregada;
            if (stockDisponibleReal < 0)
                stockDisponibleReal = 0;

            TxtPrecioUnitario.Text = producto.PrecioVenta.ToString("N2");
            TxtStockDisponible.Text = stockDisponibleReal.ToString();

            if (int.TryParse(TxtCantidad.Text, out int cantidad) && cantidad > 0)
            {
                decimal subtotal = cantidad * producto.PrecioVenta;
                TxtSubtotal.Text = subtotal.ToString("N2");
            }
            else
            {
                TxtSubtotal.Text = "0.00";
            }
        }

        private void ActualizarInfoCliente()
        {
            if (CbClientes.SelectedItem is not Cliente cliente)
            {
                LimpiarInfoClienteSeleccionado();
                return;
            }

            TxtDocumentoCliente.Text = string.IsNullOrWhiteSpace(cliente.Complemento)
                ? cliente.NumeroDocumento
                : $"{cliente.NumeroDocumento} - {cliente.Complemento}";

            TxtCorreoCliente.Text = cliente.Correo ?? string.Empty;
            TxtClienteSeleccionado.Text = cliente.NombreRazonSocial;
            TxtTelefonoCliente.Text = string.IsNullOrWhiteSpace(cliente.Telefono) ? "-" : cliente.Telefono;
        }

        private void ActualizarGrid()
        {
            DgDetalleVenta.ItemsSource = null;
            DgDetalleVenta.ItemsSource = _detalleVenta;

            decimal total = _detalleVenta.Sum(x => x.Subtotal);
            int items = _detalleVenta.Sum(x => x.Cantidad);

            TxtTotalGeneral.Text = $"Bs {total:N2}";
            TxtTotalResumen.Text = $"Bs {total:N2}";
            TxtCantidadItems.Text = $"{items} ítems";
        }

        private void CbProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarInfoProducto();
        }

        private void TxtCantidad_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarInfoProducto();
        }

        private void CbClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarInfoCliente();
        }

        private void BtnGuardarCliente_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nombre = TxtNombreClienteNuevo.Text.Trim();
                string documento = TxtDocumentoClienteNuevo.Text.Trim();

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    MessageBox.Show("Ingresa el nombre o razón social del cliente.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(documento))
                {
                    MessageBox.Show("Ingresa el número de documento o NIT.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var nuevoCliente = new Cliente
                {
                    NombreRazonSocial = nombre,
                    NumeroDocumento = documento,
                    CodigoTipoDocumentoIdentidad = 1,
                    Correo = null,
                    Telefono = null,
                    Activo = true
                };

                var clienteGuardado = _clienteService.RegistrarCliente(nuevoCliente);

                CargarClientes();

                var clienteSeleccionado = _clientes.FirstOrDefault(c => c.Id == clienteGuardado.Id);
                if (clienteSeleccionado != null)
                    CbClientes.SelectedItem = clienteSeleccionado;

                LimpiarFormularioClienteNuevo();

                MessageBox.Show("Cliente registrado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (CbProductos.SelectedItem is not Producto producto)
            {
                MessageBox.Show("Selecciona un producto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtCantidad.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int cantidad))
            {
                if (!int.TryParse(TxtCantidad.Text, out cantidad))
                {
                    MessageBox.Show("Cantidad inválida.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int cantidadYaAgregada = _detalleVenta
                .Where(d => d.ProductoId == producto.Id)
                .Sum(d => d.Cantidad);

            int stockDisponibleReal = producto.Stock - cantidadYaAgregada;

            if (cantidad > stockDisponibleReal)
            {
                MessageBox.Show("No hay suficiente stock disponible.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var detalleExistente = _detalleVenta.FirstOrDefault(d => d.ProductoId == producto.Id);

            if (detalleExistente != null)
            {
                detalleExistente.Cantidad += cantidad;
                detalleExistente.Subtotal = detalleExistente.Cantidad * detalleExistente.PrecioUnitario;
            }
            else
            {
                var detalle = new VentaDetalle
                {
                    ProductoId = producto.Id,
                    Producto = producto,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.PrecioVenta,
                    Subtotal = cantidad * producto.PrecioVenta
                };

                _detalleVenta.Add(detalle);
            }

            ActualizarGrid();
            LimpiarFormularioProducto();
        }

        private void BtnQuitarSeleccionado_Click(object sender, RoutedEventArgs e)
        {
            if (DgDetalleVenta.SelectedItem is not VentaDetalle detalle)
            {
                MessageBox.Show("Selecciona un ítem del detalle.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _detalleVenta.Remove(detalle);
            ActualizarGrid();
            ActualizarInfoProducto();
        }

        private void BtnLimpiarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (!_detalleVenta.Any())
            {
                LimpiarFormularioProducto();
                return;
            }

            var resultado = MessageBox.Show(
                "¿Deseas limpiar toda la venta actual?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
                return;

            _detalleVenta.Clear();
            ActualizarGrid();
            LimpiarFormularioProducto();
        }

        private void BtnGuardarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (CbClientes.SelectedItem is not Cliente clienteSeleccionado)
            {
                MessageBox.Show("Debes seleccionar un cliente.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_detalleVenta.Any())
            {
                MessageBox.Show("No hay productos en la venta.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var detallesParaGuardar = _detalleVenta.Select(d => new VentaDetalle
                {
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList();

                int ventaId = _ventaService.RegistrarVenta(
                    clienteSeleccionado.Id,
                    detallesParaGuardar,
                    descuentoAdicional: 0m,
                    codigoMetodoPago: 1,
                    observacion: null
                );

                MessageBox.Show($"Venta registrada correctamente. ID: {ventaId}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                _detalleVenta.Clear();
                ActualizarGrid();
                CargarProductos();
                LimpiarFormularioProducto();

                CbClientes.SelectedItem = null;
                LimpiarInfoClienteSeleccionado();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}