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
                    (p.Categoria != null && p.Categoria.Nombre.ToLower().Contains(texto)));
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            }

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

            string categoriaNormalizada = nombreCategoria.Trim();

            var categoriaExistente = db.Categorias
                .FirstOrDefault(c => c.Nombre.ToLower() == categoriaNormalizada.ToLower());

            if (categoriaExistente == null)
            {
                categoriaExistente = new Categoria
                {
                    Nombre = categoriaNormalizada
                };

                db.Categorias.Add(categoriaExistente);
                db.SaveChanges();
            }

            producto.CategoriaId = categoriaExistente.Id;

            db.Productos.Add(producto);
            db.SaveChanges();
        }

        public void Actualizar(Producto producto, string nombreCategoria)
        {
            using var db = new AppDbContext();

            string categoriaNormalizada = nombreCategoria.Trim();

            var categoriaExistente = db.Categorias
                .FirstOrDefault(c => c.Nombre.ToLower() == categoriaNormalizada.ToLower());

            if (categoriaExistente == null)
            {
                categoriaExistente = new Categoria
                {
                    Nombre = categoriaNormalizada
                };

                db.Categorias.Add(categoriaExistente);
                db.SaveChanges();
            }

            var productoDb = db.Productos.FirstOrDefault(p => p.Id == producto.Id);

            if (productoDb == null)
                return;

            productoDb.Nombre = producto.Nombre;
            productoDb.PrecioCompra = producto.PrecioCompra;
            productoDb.PrecioVenta = producto.PrecioVenta;
            productoDb.Stock = producto.Stock;
            productoDb.Descripcion = producto.Descripcion;
            productoDb.CategoriaId = categoriaExistente.Id;

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
    }
}