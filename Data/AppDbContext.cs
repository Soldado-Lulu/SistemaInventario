/*using Microsoft.EntityFrameworkCore;
using SistemaInventario.Models;
using System.IO;

namespace SistemaInventario.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<VentaDetalle> VentaDetalles => Set<VentaDetalle>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inventario.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Categoria>()
                .HasIndex(c => c.Nombre)
                .IsUnique();

            modelBuilder.Entity<Categoria>()
                .Property(c => c.Nombre)
                .IsRequired();

            modelBuilder.Entity<Producto>()
                .Property(p => p.Nombre)
                .IsRequired();

            modelBuilder.Entity<Producto>()
                .HasIndex(p => p.Nombre);

            modelBuilder.Entity<Producto>()
                .Property(p => p.Descripcion)
                .IsRequired();

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioCompra)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioVenta)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Venta>()
                .HasIndex(v => v.Fecha);

            modelBuilder.Entity<Venta>()
                .Property(v => v.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<VentaDetalle>()
                .Property(vd => vd.PrecioUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<VentaDetalle>()
                .Property(vd => vd.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<VentaDetalle>()
                .HasOne(vd => vd.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(vd => vd.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VentaDetalle>()
                .HasOne(vd => vd.Producto)
                .WithMany()
                .HasForeignKey(vd => vd.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
*/

using Microsoft.EntityFrameworkCore;
using SistemaInventario.Models;
using System.IO;

namespace SistemaInventario.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<VentaDetalle> VentaDetalles => Set<VentaDetalle>();
        public DbSet<ConfiguracionEmpresa> ConfiguracionesEmpresa => Set<ConfiguracionEmpresa>();
        public DbSet<FacturaElectronica> FacturasElectronicas => Set<FacturaElectronica>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inventario.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // =========================
            // CATEGORIA
            // =========================
            modelBuilder.Entity<Categoria>()
                .HasIndex(c => c.Nombre)
                .IsUnique();

            modelBuilder.Entity<Categoria>()
                .Property(c => c.Nombre)
                .IsRequired();

            // =========================
            // PRODUCTO
            // =========================
            modelBuilder.Entity<Producto>()
                .Property(p => p.Nombre)
                .IsRequired();

            modelBuilder.Entity<Producto>()
                .HasIndex(p => p.Nombre);

            modelBuilder.Entity<Producto>()
                .Property(p => p.Descripcion)
                .IsRequired();

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioCompra)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioVenta)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Producto>()
                .Property(p => p.CodigoActividadEconomica)
                .HasMaxLength(20);

            modelBuilder.Entity<Producto>()
                .Property(p => p.CodigoProductoSin)
                .HasMaxLength(50);

            modelBuilder.Entity<Producto>()
                .Property(p => p.UnidadMedidaSin);

            modelBuilder.Entity<Producto>()
                .Property(p => p.EsServicio)
                .HasDefaultValue(false);

            modelBuilder.Entity<Producto>()
                .Property(p => p.TieneFechaVencimiento)
                .HasDefaultValue(false);
            // =========================
            // CLIENTE
            // =========================
            modelBuilder.Entity<Cliente>()
                .Property(c => c.NombreRazonSocial)
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.NumeroDocumento)
                .IsRequired();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.NumeroDocumento);

            modelBuilder.Entity<Cliente>()
                .Property(c => c.CodigoTipoDocumentoIdentidad)
                .HasDefaultValue(1);

            // =========================
            // VENTA
            // =========================
            modelBuilder.Entity<Venta>()
                .HasIndex(v => v.Fecha);

            modelBuilder.Entity<Venta>()
                .Property(v => v.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Venta>()
                .Property(v => v.DescuentoAdicional)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            modelBuilder.Entity<Venta>()
                .Property(v => v.MontoTotalSujetoIva)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            modelBuilder.Entity<Venta>()
                .Property(v => v.EstadoFiscal)
                .IsRequired();

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);

            // =========================
            // VENTA DETALLE
            // =========================
            modelBuilder.Entity<VentaDetalle>()
                .Property(vd => vd.PrecioUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<VentaDetalle>()
                .Property(vd => vd.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<VentaDetalle>()
                .HasOne(vd => vd.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(vd => vd.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VentaDetalle>()
                .HasOne(vd => vd.Producto)
                .WithMany()
                .HasForeignKey(vd => vd.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // CONFIGURACION EMPRESA
            // =========================
            modelBuilder.Entity<ConfiguracionEmpresa>()
                .Property(c => c.RazonSocial)
                .IsRequired();

            modelBuilder.Entity<ConfiguracionEmpresa>()
                .Property(c => c.Nit)
                .IsRequired();

            modelBuilder.Entity<ConfiguracionEmpresa>()
                .HasIndex(c => c.Nit);

            // =========================
            // FACTURA ELECTRONICA
            // =========================
            modelBuilder.Entity<FacturaElectronica>()
                .Property(f => f.NumeroFactura)
                .IsRequired();

            modelBuilder.Entity<FacturaElectronica>()
                .Property(f => f.Estado)
                .IsRequired();

            modelBuilder.Entity<FacturaElectronica>()
                .HasIndex(f => f.NumeroFactura);

            modelBuilder.Entity<FacturaElectronica>()
                .HasOne(f => f.Venta)
                .WithOne(v => v.FacturaElectronica)
                .HasForeignKey<FacturaElectronica>(f => f.VentaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}