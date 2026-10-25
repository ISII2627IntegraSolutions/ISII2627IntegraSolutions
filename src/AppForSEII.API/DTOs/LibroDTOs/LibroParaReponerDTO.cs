namespace AppForSEII.API.DTOs.LibroDTOs
{
    public class LibroParaReponerDTO
    {
        public LibroParaReponerDTO()
        {
            
        }

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
        public int Id { get; set; }
        [StringLength(50, ErrorMessage = "El título no puede tener más de 50 caracteres.")]
        public string Titulo { get; set; }
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Fecha Lanzamiento")]
        public DateTime FechaLanzamiento { get; set; }
        [StringLength(50, ErrorMessage = "El autor no puede tener más de 50 caracteres.")]
        public string Autor { get; set; }
        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Range(typeof(decimal), "0.50", "99999999.99",
        ErrorMessage = "El precio debe estar entre 0,50 y 99999999,99.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "PrecioReposicion")]
        [Precision(10, 2)]
        public decimal PrecioReposicion { get; set; }
        [StringLength(50, ErrorMessage = "El género no puede tener más de 50 caracteres.")]
        public string Genero { get; set; }

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
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, FechaLanzamiento, Autor, PrecioReposicion, Genero);
        }
    }

}