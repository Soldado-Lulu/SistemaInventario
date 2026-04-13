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

        private class TipoDocumentoItem
        {
            public int Codigo { get; set; }
            public string Nombre { get; set; } = string.Empty;
        }

        public VentasPage()
        {
            InitializeComponent();
            CargarProductos();
            CargarClientes();
            CargarTiposDocumento();
            ActualizarGrid();
            LimpiarFormularioProducto();
            LimpiarFormularioCliente();
            ActualizarResumenCliente();
        }

        private void CargarProductos()
        {
            _productos = _productoService.ObtenerTodos()
                .Where(p => p.Stock > 0)
                .OrderBy(p => p.Nombre)
                .ToList();

            CbProductos.ItemsSource = null;
            CbProductos.ItemsSource = _productos;
            CbProductos.DisplayMemberPath = "Nombre";
            CbProductos.SelectedValuePath = "Id";
        }

        private void CargarClientes()
        {
            _clientes = _clienteService.ObtenerTodos()
                .OrderBy(c => c.NombreRazonSocial)
                .ToList();

            CbClientes.ItemsSource = null;
            CbClientes.ItemsSource = _clientes;
            CbClientes.DisplayMemberPath = "NombreRazonSocial";
            CbClientes.SelectedValuePath = "Id";
            CbClientes.SelectedIndex = -1;
        }

        private void CargarTiposDocumento()
        {
            var tipos = new List<TipoDocumentoItem>
            {
                new TipoDocumentoItem { Codigo = 1, Nombre = "CI" },
                new TipoDocumentoItem { Codigo = 5, Nombre = "NIT" }
            };

            CbTipoDocumentoVenta.ItemsSource = tipos;
            CbTipoDocumentoVenta.DisplayMemberPath = "Nombre";
            CbTipoDocumentoVenta.SelectedValuePath = "Codigo";
            CbTipoDocumentoVenta.SelectedValue = 1;
        }

        private void LimpiarFormularioProducto()
        {
            CbProductos.SelectedItem = null;
            TxtCantidad.Text = string.Empty;
            TxtPrecioUnitario.Text = string.Empty;
            TxtStockDisponible.Text = string.Empty;
            TxtSubtotal.Text = string.Empty;
        }

        private void LimpiarFormularioCliente()
        {
            CbClientes.SelectedIndex = -1;
            TxtNombreClienteVenta.Text = string.Empty;
            TxtDocumentoClienteVenta.Text = string.Empty;
            TxtComplementoClienteVenta.Text = string.Empty;
            TxtCorreoClienteVenta.Text = string.Empty;
            TxtCelularClienteVenta.Text = string.Empty;
            ChkGuardarComoCliente.IsChecked = false;
            ChkTieneExencion.IsChecked = false;
            CbTipoDocumentoVenta.SelectedValue = 1;
        }

        private void ActualizarResumenCliente()
        {
            string nombre = TxtNombreClienteVenta.Text.Trim();
            string telefono = TxtCelularClienteVenta.Text.Trim();

            TxtClienteSeleccionado.Text = string.IsNullOrWhiteSpace(nombre) ? "Sin cliente" : nombre;
            TxtTelefonoCliente.Text = string.IsNullOrWhiteSpace(telefono) ? "-" : telefono;
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

        private void CbClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbClientes.SelectedItem is not Cliente cliente)
                return;

            TxtNombreClienteVenta.Text = cliente.NombreRazonSocial;
            TxtDocumentoClienteVenta.Text = cliente.NumeroDocumento;
            TxtComplementoClienteVenta.Text = cliente.Complemento ?? string.Empty;
            TxtCorreoClienteVenta.Text = cliente.Correo ?? string.Empty;
            TxtCelularClienteVenta.Text = cliente.Telefono ?? string.Empty;
            CbTipoDocumentoVenta.SelectedValue = cliente.CodigoTipoDocumentoIdentidad;

            ActualizarResumenCliente();
        }

        private void TxtDatosCliente_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarResumenCliente();
        }

        private void CbTipoDocumentoVenta_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void CbProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarInfoProducto();
        }

        private void TxtCantidad_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarInfoProducto();
        }

        private int? ObtenerClienteIdSiCoincide()
        {
            if (CbClientes.SelectedItem is Cliente clienteSeleccionado)
                return clienteSeleccionado.Id;

            string documento = TxtDocumentoClienteVenta.Text.Trim();
            int codigoTipo = ObtenerCodigoTipoDocumento();

            if (string.IsNullOrWhiteSpace(documento))
                return null;

            var clienteExistente = _clientes.FirstOrDefault(c =>
                c.Activo &&
                c.NumeroDocumento.Trim().Equals(documento, StringComparison.OrdinalIgnoreCase) &&
                c.CodigoTipoDocumentoIdentidad == codigoTipo);

            return clienteExistente?.Id;
        }

        private int ObtenerCodigoTipoDocumento()
        {
            if (CbTipoDocumentoVenta.SelectedItem is TipoDocumentoItem tipo)
                return tipo.Codigo;

            if (CbTipoDocumentoVenta.SelectedValue is int codigo)
                return codigo;

            return 1;
        }

        private int? GuardarClienteSiCorresponde()
        {
            if (ChkGuardarComoCliente.IsChecked != true)
                return ObtenerClienteIdSiCoincide();

            string nombre = TxtNombreClienteVenta.Text.Trim();
            string documento = TxtDocumentoClienteVenta.Text.Trim();
            string complemento = TxtComplementoClienteVenta.Text.Trim();
            string correo = TxtCorreoClienteVenta.Text.Trim();
            string celular = TxtCelularClienteVenta.Text.Trim();
            int codigoTipo = ObtenerCodigoTipoDocumento();

            var existente = _clientes.FirstOrDefault(c =>
                c.Activo &&
                c.NumeroDocumento.Trim().Equals(documento, StringComparison.OrdinalIgnoreCase) &&
                c.CodigoTipoDocumentoIdentidad == codigoTipo);

            if (existente != null)
                return existente.Id;

            var nuevoCliente = new Cliente
            {
                NombreRazonSocial = nombre,
                NumeroDocumento = documento,
                Complemento = string.IsNullOrWhiteSpace(complemento) ? null : complemento,
                Correo = string.IsNullOrWhiteSpace(correo) ? null : correo,
                Telefono = string.IsNullOrWhiteSpace(celular) ? null : celular,
                CodigoTipoDocumentoIdentidad = codigoTipo,
                Activo = true
            };

            var clienteGuardado = _clienteService.RegistrarCliente(nuevoCliente);
            CargarClientes();

            var recienGuardado = _clientes.FirstOrDefault(c => c.Id == clienteGuardado.Id);
            if (recienGuardado != null)
                CbClientes.SelectedItem = recienGuardado;

            return clienteGuardado.Id;
        }

        private void BtnGuardarCliente_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nombre = TxtNombreClienteVenta.Text.Trim();
                string documento = TxtDocumentoClienteVenta.Text.Trim();
                string complemento = TxtComplementoClienteVenta.Text.Trim();
                string correo = TxtCorreoClienteVenta.Text.Trim();
                string celular = TxtCelularClienteVenta.Text.Trim();
                int codigoTipo = ObtenerCodigoTipoDocumento();

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
                    Complemento = string.IsNullOrWhiteSpace(complemento) ? null : complemento,
                    Correo = string.IsNullOrWhiteSpace(correo) ? null : correo,
                    Telefono = string.IsNullOrWhiteSpace(celular) ? null : celular,
                    CodigoTipoDocumentoIdentidad = codigoTipo,
                    Activo = true
                };

                var clienteGuardado = _clienteService.RegistrarCliente(nuevoCliente);

                CargarClientes();

                var clienteSeleccionado = _clientes.FirstOrDefault(c => c.Id == clienteGuardado.Id);
                if (clienteSeleccionado != null)
                    CbClientes.SelectedItem = clienteSeleccionado;

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
            if (!_detalleVenta.Any() &&
                string.IsNullOrWhiteSpace(TxtNombreClienteVenta.Text) &&
                string.IsNullOrWhiteSpace(TxtDocumentoClienteVenta.Text) &&
                string.IsNullOrWhiteSpace(TxtCorreoClienteVenta.Text) &&
                string.IsNullOrWhiteSpace(TxtCelularClienteVenta.Text))
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
            LimpiarFormularioCliente();
            ActualizarResumenCliente();
        }

        private void BtnGuardarVenta_Click(object sender, RoutedEventArgs e)
        {
            string nombreCliente = TxtNombreClienteVenta.Text.Trim();
            string documentoCliente = TxtDocumentoClienteVenta.Text.Trim();
            string complementoCliente = TxtComplementoClienteVenta.Text.Trim();
            string correoCliente = TxtCorreoClienteVenta.Text.Trim();
            string celularCliente = TxtCelularClienteVenta.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombreCliente))
            {
                MessageBox.Show("Debes ingresar el nombre o razón social del comprador.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(documentoCliente))
            {
                MessageBox.Show("Debes ingresar el documento o NIT del comprador.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_detalleVenta.Any())
            {
                MessageBox.Show("No hay productos en la venta.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int? clienteId = GuardarClienteSiCorresponde();

                var detallesParaGuardar = _detalleVenta.Select(d => new VentaDetalle
                {
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList();

                int ventaId = _ventaService.RegistrarVenta(
                    clienteId,
                    nombreCliente,
                    documentoCliente,
                    string.IsNullOrWhiteSpace(complementoCliente) ? null : complementoCliente,
                    string.IsNullOrWhiteSpace(correoCliente) ? null : correoCliente,
                    string.IsNullOrWhiteSpace(celularCliente) ? null : celularCliente,
                    ObtenerCodigoTipoDocumento(),
                    ChkTieneExencion.IsChecked == true,
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
                LimpiarFormularioCliente();
                ActualizarResumenCliente();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}