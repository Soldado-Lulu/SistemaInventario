using SistemaInventario.Models;
using SistemaInventario.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SistemaInventario.Views
{
    public partial class FacturasPage : Page
    {
        private readonly VentaService _ventaService = new();
        private readonly FacturaElectronicaService _facturaService = new();

        private List<Venta> _ventas = new();
        private Venta? _ventaSeleccionada;

        public FacturasPage()
        {
            InitializeComponent();
            CargarVentas();
            LimpiarPanelFactura();
        }

        private void CargarVentas()
        {
            _ventas = _ventaService.ObtenerVentas()
                .OrderByDescending(v => v.Fecha)
                .ToList();

            DgVentas.ItemsSource = null;
            DgVentas.ItemsSource = _ventas;
        }

        private void LimpiarPanelFactura()
        {
            TxtVentaSeleccionada.Text = "Ninguna";
            TxtClienteVenta.Text = "-";
            TxtTotalVenta.Text = "-";
            TxtEstadoFactura.Text = "Sin generar";
            TxtNumeroFactura.Text = "-";
            TxtCufFactura.Text = "-";
            TxtHashFactura.Text = "-";
            TxtGzipPreview.Text = "-";
            TxtXmlGenerado.Text = string.Empty;
        }

        private void MostrarResumenVenta(Venta venta)
        {
            TxtVentaSeleccionada.Text = $"Venta #{venta.Id}";
            TxtClienteVenta.Text = $"{venta.NombreCliente} - {venta.NumeroDocumentoCliente}";
            TxtTotalVenta.Text = $"Total: Bs {venta.Total:N2}";
        }

        private void CargarFacturaExistente(int ventaId)
        {
            var factura = _facturaService.ObtenerPorVentaId(ventaId);

            if (factura == null)
            {
                TxtEstadoFactura.Text = "Sin generar";
                TxtNumeroFactura.Text = "-";
                TxtCufFactura.Text = "-";
                TxtHashFactura.Text = "-";
                TxtGzipPreview.Text = "-";
                TxtXmlGenerado.Text = string.Empty;
                return;
            }

            TxtEstadoFactura.Text = factura.Estado;
            TxtNumeroFactura.Text = $"N° {factura.NumeroFactura}";
            TxtCufFactura.Text = string.IsNullOrWhiteSpace(factura.Cuf) ? "-" : factura.Cuf;
            TxtHashFactura.Text = string.IsNullOrWhiteSpace(factura.HashArchivo) ? "-" : factura.HashArchivo;

            string gzipPreview = string.IsNullOrWhiteSpace(factura.XmlFirmado)
                ? "-"
                : factura.XmlFirmado.Length <= 120
                    ? factura.XmlFirmado
                    : factura.XmlFirmado.Substring(0, 120) + "...";

            TxtGzipPreview.Text = gzipPreview;
            TxtXmlGenerado.Text = factura.XmlGenerado ?? string.Empty;
        }

        private void DgVentas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgVentas.SelectedItem is not Venta venta)
            {
                _ventaSeleccionada = null;
                LimpiarPanelFactura();
                return;
            }

            _ventaSeleccionada = venta;
            MostrarResumenVenta(venta);
            CargarFacturaExistente(venta.Id);
        }

        private void BtnGenerarFactura_Click(object sender, RoutedEventArgs e)
        {
            if (_ventaSeleccionada == null)
            {
                MessageBox.Show("Selecciona una venta.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var existente = _facturaService.ObtenerPorVentaId(_ventaSeleccionada.Id);

                if (existente != null)
                {
                    MessageBox.Show("Esta venta ya tiene una factura generada.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarFacturaExistente(_ventaSeleccionada.Id);
                    return;
                }

                var factura = _facturaService.GenerarDesdeVenta(_ventaSeleccionada.Id);

                TxtEstadoFactura.Text = factura.Estado;
                TxtNumeroFactura.Text = $"N° {factura.NumeroFactura}";
                TxtCufFactura.Text = factura.Cuf ?? "-";
                TxtHashFactura.Text = factura.HashArchivo ?? "-";

                string gzipPreview = string.IsNullOrWhiteSpace(factura.XmlFirmado)
                    ? "-"
                    : factura.XmlFirmado.Length <= 120
                        ? factura.XmlFirmado
                        : factura.XmlFirmado.Substring(0, 120) + "...";

                TxtGzipPreview.Text = gzipPreview;
                TxtXmlGenerado.Text = factura.XmlGenerado ?? string.Empty;

                MessageBox.Show("Factura generada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                CargarVentas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al generar factura", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRecargarVentas_Click(object sender, RoutedEventArgs e)
        {
            CargarVentas();

            if (_ventaSeleccionada != null)
            {
                var seleccionActual = _ventas.FirstOrDefault(v => v.Id == _ventaSeleccionada.Id);
                if (seleccionActual != null)
                {
                    DgVentas.SelectedItem = seleccionActual;
                }
                else
                {
                    _ventaSeleccionada = null;
                    LimpiarPanelFactura();
                }
            }
        }

        private void TxtBuscarVenta_TextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = TxtBuscarVenta.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(texto))
            {
                DgVentas.ItemsSource = null;
                DgVentas.ItemsSource = _ventas;
                return;
            }

            var filtradas = _ventas
                .Where(v =>
                    v.Id.ToString().Contains(texto) ||
                    v.NombreCliente.ToLower().Contains(texto) ||
                    v.NumeroDocumentoCliente.ToLower().Contains(texto) ||
                    v.EstadoFiscal.ToLower().Contains(texto))
                .ToList();

            DgVentas.ItemsSource = null;
            DgVentas.ItemsSource = filtradas;
        }
    }
}