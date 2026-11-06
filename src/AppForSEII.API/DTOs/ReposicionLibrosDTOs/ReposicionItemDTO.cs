// DTO del CU2: un libro dentro de la reposición, con su precio y su cantidad
// Va dentro de ReposicionForCreateDTO (lo que recibe el POST) y de ReposicionDetailDTO (lo que devuelven el details y el POST)
namespace AppForSEII.API.DTOs.ReponerLibrosDTOs
{
    public class ReposicionItemDTO
    {
        // Constructor vacío: lo usa ASP.NET para convertir el JSON del POST en objeto
        public ReposicionItemDTO()
        {
        }
         // Constructor con parámetros: lo usan CrearDetalle (en ReposicionesController) y las pruebas
        public ReposicionItemDTO(int libroId, string titulo, string autor, decimal precioReposicion, int cantidad)
        {
            LibroId = libroId;
            Titulo = titulo;
            Autor = autor;
            PrecioReposicion = precioReposicion;
            Cantidad = cantidad;
        }
         // Constructor con parámetros: lo usan CrearDetalle (en ReposicionesController) y las pruebas
        public int LibroId { get; set; }
        // Título y autor: datos del libro para enseñarlos en el resumen
        public string Titulo { get; set; }

        public string Autor { get; set; }

         // Precio por unidad; en el POST el controlador NO usa este valor, usa el PrecioCompra del libro
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        public decimal PrecioReposicion { get; set; }
         // Paso 5: la cantidad tiene que ser mayor que 1 (de 2 en adelante)
        // MODIFICAR: para cambiar la cantidad mínima se cambia el 2 aquí y el "item.Cantidad < 2" de CreateReposicion
        [Range(2, int.MaxValue, ErrorMessage = "La cantidad de reposición debe ser mayor que 1.")]
        public int Cantidad { get; set; }
        // Equals: compara todos los campos; lo usan SequenceEqual (en ReposicionForCreateDTO) y Assert.Equal
        public override bool Equals(object? obj)
        {
            return obj is ReposicionItemDTO dto &&
                   LibroId == dto.LibroId &&
                   Titulo == dto.Titulo &&
                   Autor == dto.Autor &&
                   PrecioReposicion == dto.PrecioReposicion &&
                   Cantidad == dto.Cantidad;
        }
         // GetHashCode: va siempre con Equals y usa los mismos campos
        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, Titulo, Autor, PrecioReposicion, Cantidad);
        }
    }
}