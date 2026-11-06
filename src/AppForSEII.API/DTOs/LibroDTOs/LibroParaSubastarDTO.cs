// DTO del SELECT del CU3: datos de cada libro que aparece en la lista para subastar
// Lo crea GetLibrosParaSubastar (LibroController) y lo prueba GetLibrosParaSubastar_test
// DTO = objeto solo para enviar datos a la web, sin las relaciones de la entidad Libro (evita ciclos en el JSON)
namespace AppForSEII.API.DTOs.LibroDTOs
{
    public class LibroParaSubastarDTO
    {
        // Constructor con parámetros: lo usa el Select de GetLibrosParaSubastar y las pruebas
        // MODIFICAR: para mostrar otro dato, se añade aquí, como propiedad, en Equals, en GetHashCode y en el Select del controlador
        public LibroParaSubastarDTO(int id, string titulo, string editorial, string autor, string tipoLibro)
        {
            Id = id;
            Titulo = titulo;
            Editorial = editorial;
            Autor = autor;
            TipoLibro = tipoLibro;
        }

        // Id del libro: la web lo necesita para mandarlo después en el POST (SubastaItemDTO.LibroId)
        public int Id { get; set; }

        // StringLength = máximo 50 caracteres, igual que en Libro
        [StringLength(50, ErrorMessage = "El título no puede tener más de 50 caracteres.")]
        public string Titulo { get; set; }

        // Solo el nombre de la editorial (en Libro es un objeto Editorial completo)
        [StringLength(50, ErrorMessage = "La editorial no puede tener más de 50 caracteres.")]
        public string Editorial { get; set; }

        public string Autor { get; set; }

        // Display = nombre de la columna que se ve en la web
        [System.ComponentModel.DataAnnotations.Display(Name = "Tipo de libro")]
        public string TipoLibro { get; set; }

        // Equals: dos DTO son iguales si todos sus datos son iguales; lo usa Assert.Equal en las pruebas
        public override bool Equals(object? obj)
        {
            return obj is LibroParaSubastarDTO dto &&
                   Id == dto.Id &&
                   Titulo == dto.Titulo &&
                   Editorial == dto.Editorial &&
                   Autor == dto.Autor &&
                   TipoLibro == dto.TipoLibro;
        }

        // GetHashCode: va siempre con Equals y usa los mismos campos
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, Editorial, Autor, TipoLibro);
        }
    }
}