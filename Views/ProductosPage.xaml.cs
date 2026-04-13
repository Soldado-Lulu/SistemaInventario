using SistemaInventario.Models;
using SistemaInventario.Services;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace SistemaInventario.Views
{
    public partial class ProductosPage : Page
    {
        private readonly ProductoService _productoService = new();
        private Producto? _productoSeleccionado;
        private List<Producto> _productos = new();

        public ProductosPage()
        {
            InitializeComponent();
            CargarCategorias();
            CargarProductos();
        }

        private void CargarCategorias()
        {
            var categorias = _productoService.ObtenerCategorias();
            CbCategoria.ItemsSource = null;
            CbCategoria.ItemsSource = categorias;
        }

        private void CargarProductos()
        {
            _productos = _productoService.ObtenerTodos();
            DgProductos.ItemsSource = null;
            DgProductos.ItemsSource = _productos;
            ActualizarResumen();
            CargarCategorias();
        }

        private void ActualizarResumen()
        {
            TxtTotalProductos.Text = $"{_productos.Count} productos";
            TxtStockBajo.Text = $"{_productos.Count(p => p.Stock <= 5)} productos";
            TxtTotalCategorias.Text = $"{_productos.Select(p => p.CategoriaId).Distinct().Count()} categorías";
        }

        private void LimpiarFormulario()
        {
            TxtNombre.Text = string.Empty;
            TxtPrecioCompra.Text = string.Empty;
            TxtPrecioVenta.Text = string.Empty;
            TxtStock.Text = string.Empty;
            TxtDescripcion.Text = string.Empty;
            TxtBuscar.Text = string.Empty;
            TxtCodigoActividad.Text = string.Empty;
            TxtCodigoProductoSin.Text = string.Empty;
            TxtUnidadMedidaSin.Text = string.Empty;

            ChkEsServicio.IsChecked = false;
            ChkTieneVencimiento.IsChecked = false;

            CbCategoria.Text = string.Empty;
            CbCategoria.SelectedItem = null;

            _productoSeleccionado = null;
            DgProductos.SelectedItem = null;
            DgProductos.ItemsSource = null;
            DgProductos.ItemsSource = _productos;
        }

        private bool ValidarFormulario(
            out decimal precioCompra,
            out decimal precioVenta,
            out int stock,
            out string nombreCategoria,
            out int? unidadMedidaSin)
        {
            precioCompra = 0;
            precioVenta = 0;
            stock = 0;
            unidadMedidaSin = null;
            nombreCategoria = CbCategoria.Text.Trim();

            if (string.IsNullOrWhiteSpace(TxtNombre.Text))
            {
                MessageBox.Show("El nombre del producto es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(nombreCategoria))
            {
                MessageBox.Show("La categoría es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(TxtPrecioCompra.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out precioCompra))
            {
                if (!decimal.TryParse(TxtPrecioCompra.Text, out precioCompra))
                {
                    MessageBox.Show("Precio de compra inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            if (!decimal.TryParse(TxtPrecioVenta.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out precioVenta))
            {
                if (!decimal.TryParse(TxtPrecioVenta.Text, out precioVenta))
                {
                    MessageBox.Show("Precio de venta inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            if (!int.TryParse(TxtStock.Text, out stock))
            {
                MessageBox.Show("Stock inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(TxtUnidadMedidaSin.Text))
            {
                if (!int.TryParse(TxtUnidadMedidaSin.Text, out int unidad))
                {
                    MessageBox.Show("La unidad de medida SIN es inválida.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                unidadMedidaSin = unidad;
            }

            if (precioCompra < 0 || precioVenta < 0 || stock < 0)
            {
                MessageBox.Show("No se permiten valores negativos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private Producto ConstruirProducto(decimal precioCompra, decimal precioVenta, int stock, int? unidadMedidaSin)
        {
            return new Producto
            {
                Nombre = TxtNombre.Text.Trim(),
                PrecioCompra = precioCompra,
                PrecioVenta = precioVenta,
                Stock = stock,
                Descripcion = TxtDescripcion.Text.Trim(),

                CodigoActividadEconomica = string.IsNullOrWhiteSpace(TxtCodigoActividad.Text) ? null : TxtCodigoActividad.Text.Trim(),
                CodigoProductoSin = string.IsNullOrWhiteSpace(TxtCodigoProductoSin.Text) ? null : TxtCodigoProductoSin.Text.Trim(),
                UnidadMedidaSin = unidadMedidaSin,
                EsServicio = ChkEsServicio.IsChecked == true,
                TieneFechaVencimiento = ChkTieneVencimiento.IsChecked == true
            };
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulario(out decimal precioCompra, out decimal precioVenta, out int stock, out string nombreCategoria, out int? unidadMedidaSin))
                return;

            var producto = ConstruirProducto(precioCompra, precioVenta, stock, unidadMedidaSin);

            _productoService.Crear(producto, nombreCategoria);
            CargarProductos();
            LimpiarFormulario();

            MessageBox.Show("Producto guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (_productoSeleccionado == null)
            {
                MessageBox.Show("Selecciona un producto para actualizar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidarFormulario(out decimal precioCompra, out decimal precioVenta, out int stock, out string nombreCategoria, out int? unidadMedidaSin))
                return;

            var productoActualizado = ConstruirProducto(precioCompra, precioVenta, stock, unidadMedidaSin);
            productoActualizado.Id = _productoSeleccionado.Id;

            _productoService.Actualizar(productoActualizado, nombreCategoria);
            CargarProductos();
            LimpiarFormulario();

            MessageBox.Show("Producto actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_productoSeleccionado == null)
            {
                MessageBox.Show("Selecciona un producto para eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Deseas eliminar el producto '{_productoSeleccionado.Nombre}'?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes)
                return;

            _productoService.Eliminar(_productoSeleccionado.Id);
            CargarProductos();
            LimpiarFormulario();

            MessageBox.Show("Producto eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void DgProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgProductos.SelectedItem is not Producto producto)
                return;

            _productoSeleccionado = producto;

            TxtNombre.Text = producto.Nombre;
            TxtPrecioCompra.Text = producto.PrecioCompra.ToString();
            TxtPrecioVenta.Text = producto.PrecioVenta.ToString();
            TxtStock.Text = producto.Stock.ToString();
            TxtDescripcion.Text = producto.Descripcion;
            CbCategoria.Text = producto.Categoria?.Nombre ?? string.Empty;

            TxtCodigoActividad.Text = producto.CodigoActividadEconomica ?? string.Empty;
            TxtCodigoProductoSin.Text = producto.CodigoProductoSin ?? string.Empty;
            TxtUnidadMedidaSin.Text = producto.UnidadMedidaSin?.ToString() ?? string.Empty;
            ChkEsServicio.IsChecked = producto.EsServicio;
            ChkTieneVencimiento.IsChecked = producto.TieneFechaVencimiento;
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = TxtBuscar.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(texto))
            {
                DgProductos.ItemsSource = null;
                DgProductos.ItemsSource = _productos;
                return;
            }

            var filtrados = _productos
                .Where(p =>
                    p.Nombre.ToLower().Contains(texto) ||
                    (p.Categoria != null && p.Categoria.Nombre.ToLower().Contains(texto)) ||
                    (p.CodigoProductoSin != null && p.CodigoProductoSin.ToLower().Contains(texto)) ||
                    (p.CodigoActividadEconomica != null && p.CodigoActividadEconomica.ToLower().Contains(texto)))
                .ToList();

            DgProductos.ItemsSource = null;
            DgProductos.ItemsSource = filtrados;
        }
    }
}