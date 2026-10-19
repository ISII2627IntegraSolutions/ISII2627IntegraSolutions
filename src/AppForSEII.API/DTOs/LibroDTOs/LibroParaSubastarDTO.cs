namespace AppForSEII.API.DTOs.LibroDTOs
{
    public class LibroParaSubastarDTO
    {
        public LibroParaSubastarDTO(int id, string titulo, string editorial, string autor, string tipoLibro)
        {
            Id = id;
            Titulo = titulo;
            Editorial = editorial;
            Autor = autor;
            TipoLibro = tipoLibro;
        }

        public int Id { get; set; }

        [StringLength(50, ErrorMessage = "El título no puede tener más de 50 caracteres.")]
        public string Titulo { get; set; }

        [StringLength(50, ErrorMessage = "La editorial no puede tener más de 50 caracteres.")]
        public string Editorial { get; set; }

        public string Autor { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Tipo de libro")]
        public string TipoLibro { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is LibroParaSubastarDTO dto &&
                   Id == dto.Id &&
                   Titulo == dto.Titulo &&
                   Editorial == dto.Editorial &&
                   Autor == dto.Autor &&
                   TipoLibro == dto.TipoLibro;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, Editorial, Autor, TipoLibro);
        }
    }
}