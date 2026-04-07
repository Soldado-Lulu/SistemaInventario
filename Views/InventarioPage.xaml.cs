using SistemaInventario.Models;
using SistemaInventario.Services;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace SistemaInventario.Views
{
    public partial class InventarioPage : Page
    {
        private readonly ProductoService _productoService = new();

        private List<Categoria> _categorias = new();
        private Producto? _productoSeleccionado;

        private int _paginaActual = 1;
        private const int _tamanioPagina = 20;
        private int _totalRegistros = 0;

        public InventarioPage()
        {
            InitializeComponent();
            CargarCategorias();
            CargarInventario();
        }

        private void CargarCategorias()
        {
            _categorias = _productoService.ObtenerCategorias();

            CbCategoriaFiltro.ItemsSource = null;
            CbCategoriaFiltro.ItemsSource = _categorias;
            CbCategoriaFiltro.SelectedItem = null;
        }

        private void CargarInventario()
        {
            string texto = TxtBuscar.Text?.Trim();
            int? categoriaId = CbCategoriaFiltro.SelectedValue as int?;

            var resultado = _productoService.ObtenerPaginado(
                _paginaActual,
                _tamanioPagina,
                texto,
                categoriaId);

            _totalRegistros = resultado.Total;

            DgInventario.ItemsSource = null;
            DgInventario.ItemsSource = resultado.Items;

            TxtResumenProductos.Text = _totalRegistros.ToString();
            TxtResumenStockBajo.Text = _productoService.ObtenerTodos().Count(p => p.Stock <= 5).ToString();
            TxtResumenCategorias.Text = _productoService.ObtenerCategorias().Count.ToString();

            int totalPaginas = _totalRegistros == 0
                ? 1
                : (int)Math.Ceiling((double)_totalRegistros / _tamanioPagina);

            TxtPagina.Text = $"Página {_paginaActual} de {totalPaginas}";
        }

        private void BtnAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (_paginaActual > 1)
            {
                _paginaActual--;
                CargarInventario();
            }
        }

        private void BtnSiguiente_Click(object sender, RoutedEventArgs e)
        {
            int totalPaginas = _totalRegistros == 0
                ? 1
                : (int)Math.Ceiling((double)_totalRegistros / _tamanioPagina);

            if (_paginaActual < totalPaginas)
            {
                _paginaActual++;
                CargarInventario();
            }
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            _paginaActual = 1;
            CargarInventario();
        }

        private void CbCategoriaFiltro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _paginaActual = 1;
            CargarInventario();
        }

        private void DgInventario_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgInventario.SelectedItem is not Producto producto)
                return;

            _productoSeleccionado = producto;
            TxtProductoSeleccionado.Text = producto.Nombre;
            TxtStockActual.Text = producto.Stock.ToString();
        }

        private void BtnEntrada_Click(object sender, RoutedEventArgs e)
        {
            if (_productoSeleccionado == null)
            {
                MessageBox.Show("Selecciona un producto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtCantidadAjuste.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int cantidad))
            {
                if (!int.TryParse(TxtCantidadAjuste.Text, out cantidad))
                {
                    MessageBox.Show("Cantidad inválida.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a cero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _productoService.AjustarStock(_productoSeleccionado.Id, cantidad);
            CargarInventario();
            RefrescarSeleccion();
            TxtCantidadAjuste.Text = string.Empty;

            MessageBox.Show("Entrada de stock registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnSalida_Click(object sender, RoutedEventArgs e)
        {
            if (_productoSeleccionado == null)
            {
                MessageBox.Show("Selecciona un producto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtCantidadAjuste.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int cantidad))
            {
                if (!int.TryParse(TxtCantidadAjuste.Text, out cantidad))
                {
                    MessageBox.Show("Cantidad inválida.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a cero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _productoService.AjustarStock(_productoSeleccionado.Id, -cantidad);
                CargarInventario();
                RefrescarSeleccion();
                TxtCantidadAjuste.Text = string.Empty;

                MessageBox.Show("Salida de stock registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefrescarSeleccion()
        {
            if (_productoSeleccionado == null)
                return;

            var actualizado = _productoService.ObtenerPorId(_productoSeleccionado.Id);
            if (actualizado == null)
                return;

            _productoSeleccionado = actualizado;
            TxtProductoSeleccionado.Text = actualizado.Nombre;
            TxtStockActual.Text = actualizado.Stock.ToString();
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            _productoSeleccionado = null;
            TxtProductoSeleccionado.Text = string.Empty;
            TxtStockActual.Text = string.Empty;
            TxtCantidadAjuste.Text = string.Empty;
            DgInventario.SelectedItem = null;
        }
    }
}