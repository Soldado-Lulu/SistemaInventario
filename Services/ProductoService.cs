using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data;
using SistemaInventario.Models;

namespace SistemaInventario.Services
{
    public class ProductoService
    {
        public (List<Producto> Items, int Total) ObtenerPaginado(
            int pagina,
            int tamanio,
            string? textoBusqueda = null,
            int? categoriaId = null)
        {
            using var db = new AppDbContext();

            var query = db.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(textoBusqueda))
            {
                string texto = textoBusqueda.Trim().ToLower();

                query = query.Where(p =>
                    p.Nombre.ToLower().Contains(texto) ||
                    (p.Categoria != null && p.Categoria.Nombre.ToLower().Contains(texto)) ||
                    (p.CodigoProductoSin != null && p.CodigoProductoSin.ToLower().Contains(texto)) ||
                    (p.CodigoActividadEconomica != null && p.CodigoActividadEconomica.ToLower().Contains(texto)));
            }

            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId.Value);

            int total = query.Count();

            var items = query
                .OrderBy(p => p.Nombre)
                .Skip((pagina - 1) * tamanio)
                .Take(tamanio)
                .ToList();

            return (items, total);
        }

        public List<Producto> ObtenerTodos()
        {
            using var db = new AppDbContext();

            return db.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .OrderBy(p => p.Nombre)
                .ToList();
        }

        public Producto? ObtenerPorId(int id)
        {
            using var db = new AppDbContext();

            return db.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.Id == id);
        }

        public void Crear(Producto producto, string nombreCategoria)
        {
            using var db = new AppDbContext();

            ValidarProducto(producto, nombreCategoria);

            var categoria = ObtenerOCrearCategoria(db, nombreCategoria);
            producto.CategoriaId = categoria.Id;

            NormalizarProducto(producto);

            db.Productos.Add(producto);
            db.SaveChanges();
        }

        public void Actualizar(Producto producto, string nombreCategoria)
        {
            using var db = new AppDbContext();

            ValidarProducto(producto, nombreCategoria);

            var categoria = ObtenerOCrearCategoria(db, nombreCategoria);

            var productoDb = db.Productos.FirstOrDefault(p => p.Id == producto.Id);

            if (productoDb == null)
                throw new Exception("Producto no encontrado.");

            productoDb.Nombre = producto.Nombre.Trim();
            productoDb.PrecioCompra = producto.PrecioCompra;
            productoDb.PrecioVenta = producto.PrecioVenta;
            productoDb.Stock = producto.Stock;
            productoDb.Descripcion = string.IsNullOrWhiteSpace(producto.Descripcion) ? string.Empty : producto.Descripcion.Trim();
            productoDb.CategoriaId = categoria.Id;

            productoDb.CodigoActividadEconomica = LimpiarTextoOpcional(producto.CodigoActividadEconomica);
            productoDb.CodigoProductoSin = LimpiarTextoOpcional(producto.CodigoProductoSin);
            productoDb.UnidadMedidaSin = producto.UnidadMedidaSin;
            productoDb.EsServicio = producto.EsServicio;
            productoDb.TieneFechaVencimiento = producto.TieneFechaVencimiento;

            db.SaveChanges();
        }

        public void Eliminar(int id)
        {
            using var db = new AppDbContext();

            var producto = db.Productos.FirstOrDefault(p => p.Id == id);

            if (producto != null)
            {
                db.Productos.Remove(producto);
                db.SaveChanges();
            }
        }

        public List<Categoria> ObtenerCategorias()
        {
            using var db = new AppDbContext();

            return db.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .ToList();
        }

        public void AjustarStock(int productoId, int cantidad)
        {
            using var db = new AppDbContext();

            var producto = db.Productos.FirstOrDefault(p => p.Id == productoId);

            if (producto == null)
                throw new Exception("Producto no encontrado.");

            int nuevoStock = producto.Stock + cantidad;

            if (nuevoStock < 0)
                throw new Exception("No se puede dejar el stock en negativo.");

            producto.Stock = nuevoStock;
            db.SaveChanges();
        }

        private static Categoria ObtenerOCrearCategoria(AppDbContext db, string nombreCategoria)
        {
            string categoriaNormalizada = nombreCategoria.Trim();

            var categoriaExistente = db.Categorias
                .FirstOrDefault(c => c.Nombre.ToLower() == categoriaNormalizada.ToLower());

            if (categoriaExistente != null)
                return categoriaExistente;

            categoriaExistente = new Categoria
            {
                Nombre = categoriaNormalizada
            };

            db.Categorias.Add(categoriaExistente);
            db.SaveChanges();

            return categoriaExistente;
        }

        private static void ValidarProducto(Producto producto, string nombreCategoria)
        {
            if (producto == null)
                throw new Exception("Los datos del producto son obligatorios.");

            if (string.IsNullOrWhiteSpace(producto.Nombre))
                throw new Exception("El nombre del producto es obligatorio.");

            if (string.IsNullOrWhiteSpace(nombreCategoria))
                throw new Exception("La categoría es obligatoria.");

            if (producto.PrecioCompra < 0 || producto.PrecioVenta < 0)
                throw new Exception("Los precios no pueden ser negativos.");

            if (producto.Stock < 0)
                throw new Exception("El stock no puede ser negativo.");

            if (producto.UnidadMedidaSin.HasValue && producto.UnidadMedidaSin <= 0)
                throw new Exception("La unidad de medida SIN es inválida.");
        }

        private static void NormalizarProducto(Producto producto)
        {
            producto.Nombre = producto.Nombre.Trim();
            producto.Descripcion = string.IsNullOrWhiteSpace(producto.Descripcion) ? string.Empty : producto.Descripcion.Trim();
            producto.CodigoActividadEconomica = LimpiarTextoOpcional(producto.CodigoActividadEconomica);
            producto.CodigoProductoSin = LimpiarTextoOpcional(producto.CodigoProductoSin);
        }

        private static string? LimpiarTextoOpcional(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}