// DTO del SELECT del CU2: datos de cada libro que aparece en la lista para reponer (libros sin stock)
// Lo crea GetLibrosParaReponer (LibroController) y lo prueba GetLibrosParaReponer_test
// DTO = objeto solo para enviar datos a la web, sin las relaciones de la entidad Libro (evita ciclos en el JSON)
namespace AppForSEII.API.DTOs.LibroDTOs
{
    public class LibroParaReponerDTO
    {
        // Constructor vacío
        public LibroParaReponerDTO()
        {
            
        }
        // Constructor con parámetros: lo usan el Select de GetLibrosParaReponer y las pruebas
        // MODIFICAR: para mostrar otro dato, se añade aquí, como propiedad, en Equals, en GetHashCode y en el Select del controlador
        public LibroParaReponerDTO(int id,string titulo, DateTime fechaLanzamiento, 
        string autor, decimal precioReposicion, string genero)
        {
          Id=id;
          Titulo=titulo;
          FechaLanzamiento=fechaLanzamiento;
          Autor=autor;
          PrecioReposicion=precioReposicion;
          Genero=genero;
        }
        // Id del libro: la web lo necesita para mandarlo después en el POST (ReposicionItemDTO.LibroId)
        public int Id { get; set; }
        // StringLength = máximo 50 caracteres, igual que en Libro
        [StringLength(50, ErrorMessage = "El título no puede tener más de 50 caracteres.")]
        public string Titulo { get; set; }
        // Fecha de lanzamiento; DataType, DisplayFormat y Display solo cambian cómo se muestra (dd/MM/yyyy)
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Fecha Lanzamiento")]
        public DateTime FechaLanzamiento { get; set; }
        [StringLength(50, ErrorMessage = "El autor no puede tener más de 50 caracteres.")]
        public string Autor { get; set; }
        // Precio de reposición: Libro no tiene uno propio, el controlador pone su PrecioCompra
        // Range = entre 0,50 y 99999999,99 (solo se comprueba si el DTO llega en una petición; aquí solo se devuelve)
        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Range(typeof(decimal), "0.50", "99999999.99",
        ErrorMessage = "El precio debe estar entre 0,50 y 99999999,99.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "PrecioReposicion")]
        [Precision(10, 2)]
        public decimal PrecioReposicion { get; set; }
        // Solo el nombre del género (en Libro es un objeto Genero completo)
        [StringLength(50, ErrorMessage = "El género no puede tener más de 50 caracteres.")]
        public string Genero { get; set; }
        // Equals: dos DTO son iguales si todos sus datos son iguales; lo usa Assert.Equal en las pruebas
        public override bool Equals(object? obj)
        {
            return obj is LibroParaReponerDTO dto &&
                   Id == dto.Id &&
                   Titulo == dto.Titulo &&
                   FechaLanzamiento == dto.FechaLanzamiento &&
                   Autor == dto.Autor &&
                   PrecioReposicion == dto.PrecioReposicion &&
                   Genero == dto.Genero;
        }
        // GetHashCode: va siempre con Equals y usa los mismos campos
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, FechaLanzamiento, Autor, PrecioReposicion, Genero);
        }
    }

}