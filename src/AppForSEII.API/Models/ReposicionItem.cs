// MODELO del CU2: tabla ReposicionItems, una fila por cada libro de una reposición con su cantidad y su precio
// Es la tabla intermedia de la relación N:M entre Reposicion y Libro
// La crean CreateReposicion y SeedReposiciones; la lee GetDetailsForReposicion
namespace AppForSEII.API.Models
{
    // Clave primaria compuesta (LibroId + ReposicionId): el mismo libro no puede repetirse en una reposición
    [PrimaryKey(nameof(LibroId), nameof(ReposicionId))]
    public class ReposicionItem
    {
        // Constructor vacío: lo usa Entity Framework
        public ReposicionItem()
        {
        }
        // Constructor con parámetros: recibe los objetos Libro y Reposicion; EF rellena LibroId y ReposicionId al guardar
        public ReposicionItem(int cantidadReposicion, Libro libro, Reposicion reposicion, decimal precioDeReposicion)
        {
            CantidadReposicion = cantidadReposicion;
            Libro = libro;
            Reposicion = reposicion;
            PrecioDeReposicion = precioDeReposicion;
        }

        // Clave ajena del libro: es lo que se guarda en la tabla
        public int LibroId { get; set; }
        // Navegación: el objeto Libro entero, para usarlo en C# (ri.Libro.Titulo)
        public Libro Libro { get; set; }
        // Paso 5: unidades que se reponen (la regla "mayor que 1" se comprueba en ReposicionItemDTO y en CreateReposicion)
        public int CantidadReposicion { get; set; }
        // Navegación: la reposición a la que pertenece este libro
        public Reposicion Reposicion { get; set; }
        // Clave ajena de la reposición
        public int ReposicionId { get; set; }
        // Precio por unidad (en CreateReposicion es el PrecioCompra del libro); decimal(10,2) en la base de datos
        [Precision(10, 2)]
        public decimal PrecioDeReposicion { get; set; }
    }
}