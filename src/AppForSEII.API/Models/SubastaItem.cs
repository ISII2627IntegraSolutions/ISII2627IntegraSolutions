// MODELO del CU3: tabla SubastaItems, una fila por cada libro de una subasta con su puja
// Es la tabla intermedia de la relación N:M entre Subasta y Libro
// La crean CreateSubasta y SeedSubastas; la lee GetDetailsForSubasta
namespace AppForSEII.API.Models
{
    // Clave primaria compuesta (LibroId + SubastaId): el mismo libro no puede repetirse en una subasta
    [PrimaryKey(nameof(LibroId), nameof(SubastaId))]
    public class SubastaItem
    {
        // Constructor vacío: lo usa Entity Framework
        public SubastaItem()
        {
        }

        // Constructor con parámetros: recibe los objetos Libro y Subasta; EF rellena LibroId y SubastaId al guardar
        public SubastaItem(Libro libro, Subasta subasta, decimal precioPuja, string? descripcion)
        {
            Libro = libro;
            Subasta = subasta;
            PrecioPuja = precioPuja;
            Descripcion = descripcion;
        }

        // FA2: la puja tiene que ser como mínimo 1. EF no valida al guardar: la que actúa en el POST es la de SubastaItemDTO
        [Range(typeof(decimal), "1", "99999999.99",
        ErrorMessage = "El precio de la puja debe ser como mínimo 1.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Precio Puja")]
        // decimal(10,2) en la base de datos
        [Precision(10, 2)]
        public decimal PrecioPuja { get; set; }

        // Paso 5: descripción opcional (? = puede ser null); si se escribe, entre 20 y 100 caracteres
        // MODIFICAR: si cambian la longitud, se cambia aquí y también en SubastaItemDTO
        [StringLength(100, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 100 caracteres.")]
        public string? Descripcion { get; set; }

        // Clave ajena del libro: es lo que se guarda en la tabla
        public int LibroId { get; set; }

        // Navegación: el objeto Libro entero, para usarlo en C# (si.Libro.Titulo)
        public Libro Libro { get; set; }

        // Clave ajena de la subasta
        public int SubastaId { get; set; }

        // Navegación: la subasta a la que pertenece esta puja
        public Subasta Subasta { get; set; }
    }
}