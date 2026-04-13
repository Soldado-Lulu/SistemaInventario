using SistemaInventario.Models;
using SistemaInventario.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SistemaInventario.Views
{
    public partial class ConfiguracionEmpresaPage : Page
    {
        private readonly ConfiguracionEmpresaService _configuracionEmpresaService = new();

        public ConfiguracionEmpresaPage()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void CargarDatos()
        {
            try
            {
                var config = _configuracionEmpresaService.Obtener();

                TxtRazonSocial.Text = config.RazonSocial ?? string.Empty;
                TxtNit.Text = config.Nit ?? string.Empty;
                TxtNombreComercial.Text = config.NombreComercial ?? string.Empty;
                TxtDireccion.Text = config.Direccion ?? string.Empty;
                TxtTelefono.Text = config.Telefono ?? string.Empty;
                TxtMunicipio.Text = config.Municipio ?? string.Empty;
                TxtCorreo.Text = config.Correo ?? string.Empty;

                TxtCodigoSistema.Text = config.CodigoSistema ?? string.Empty;
                TxtTokenDelegado.Text = config.TokenDelegado ?? string.Empty;
                TxtCuis.Text = config.Cuis ?? string.Empty;
                TxtCufdActual.Text = config.CufdActual ?? string.Empty;

                TxtCodigoAmbiente.Text = config.CodigoAmbiente.ToString();
                TxtCodigoModalidad.Text = config.CodigoModalidad.ToString();
                TxtCodigoSucursal.Text = config.CodigoSucursal.ToString();
                TxtCodigoPuntoVenta.Text = config.CodigoPuntoVenta?.ToString() ?? string.Empty;

                ChkActivo.IsChecked = config.Activo;

                TxtEstadoConfiguracion.Text =
                    string.IsNullOrWhiteSpace(config.RazonSocial) || string.IsNullOrWhiteSpace(config.Nit)
                    ? "Incompleta"
                    : "Configurada";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al cargar configuración", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRecargar_Click(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(TxtCodigoAmbiente.Text.Trim(), out int codigoAmbiente))
                {
                    MessageBox.Show("El código de ambiente es inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(TxtCodigoModalidad.Text.Trim(), out int codigoModalidad))
                {
                    MessageBox.Show("El código de modalidad es inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(TxtCodigoSucursal.Text.Trim(), out int codigoSucursal))
                {
                    MessageBox.Show("El código de sucursal es inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int? codigoPuntoVenta = null;
                if (!string.IsNullOrWhiteSpace(TxtCodigoPuntoVenta.Text))
                {
                    if (!int.TryParse(TxtCodigoPuntoVenta.Text.Trim(), out int puntoVenta))
                    {
                        MessageBox.Show("El código de punto de venta es inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    codigoPuntoVenta = puntoVenta;
                }

                var modelo = new ConfiguracionEmpresa
                {
                    RazonSocial = TxtRazonSocial.Text.Trim(),
                    Nit = TxtNit.Text.Trim(),
                    NombreComercial = TxtNombreComercial.Text.Trim(),
                    Direccion = TxtDireccion.Text.Trim(),
                    Telefono = TxtTelefono.Text.Trim(),
                    Municipio = TxtMunicipio.Text.Trim(),
                    Correo = TxtCorreo.Text.Trim(),

                    CodigoSistema = TxtCodigoSistema.Text.Trim(),
                    TokenDelegado = TxtTokenDelegado.Text.Trim(),
                    Cuis = TxtCuis.Text.Trim(),
                    CufdActual = TxtCufdActual.Text.Trim(),

                    CodigoAmbiente = codigoAmbiente,
                    CodigoModalidad = codigoModalidad,
                    CodigoSucursal = codigoSucursal,
                    CodigoPuntoVenta = codigoPuntoVenta,

                    Activo = ChkActivo.IsChecked == true
                };

                _configuracionEmpresaService.GuardarOActualizar(modelo);

                TxtEstadoConfiguracion.Text = "Configurada";

                MessageBox.Show("Configuración guardada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}