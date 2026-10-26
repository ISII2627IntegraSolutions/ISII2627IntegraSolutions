// SubastaItemDTO: un libro dentro de la subasta, con su puja y su descripción.
namespace AppForSEII.API.DTOs.SubastarLibrosDTOs
{
    public class SubastaItemDTO
    {
        public SubastaItemDTO()
        {
        }

        public SubastaItemDTO(int libroId, string titulo, string autor, string editorial, decimal precioPuja, string? descripcion)
        {
            LibroId = libroId;
            Titulo = titulo;
            Autor = autor;
            Editorial = editorial;
            PrecioPuja = precioPuja;
            Descripcion = descripcion;
        }

        public int LibroId { get; set; }

        public string Titulo { get; set; }

        public string Autor { get; set; }

        public string Editorial { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Range(typeof(decimal), "1", "99999999.99", ErrorMessage = "El precio de la puja debe ser como mínimo 1.")]
        public decimal PrecioPuja { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 100 caracteres.")]
        public string? Descripcion { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is SubastaItemDTO dto &&
                   LibroId == dto.LibroId &&
                   Titulo == dto.Titulo &&
                   Autor == dto.Autor &&
                   Editorial == dto.Editorial &&
                   PrecioPuja == dto.PrecioPuja &&
                   Descripcion == dto.Descripcion;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, Titulo, Autor, Editorial, PrecioPuja, Descripcion);
        }
    }
}