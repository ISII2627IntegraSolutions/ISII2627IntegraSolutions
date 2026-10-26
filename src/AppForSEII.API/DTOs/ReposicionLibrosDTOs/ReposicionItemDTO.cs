namespace AppForSEII.API.DTOs.ReponerLibrosDTOs
{
    public class ReposicionItemDTO
    {
        public ReposicionItemDTO()
        {
        }

        public ReposicionItemDTO(int libroId, string titulo, string autor, decimal precioReposicion, int cantidad)
        {
            LibroId = libroId;
            Titulo = titulo;
            Autor = autor;
            PrecioReposicion = precioReposicion;
            Cantidad = cantidad;
        }

        public int LibroId { get; set; }

        public string Titulo { get; set; }

        public string Autor { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        public decimal PrecioReposicion { get; set; }

        [Range(2, int.MaxValue, ErrorMessage = "La cantidad de reposición debe ser mayor que 1.")]
        public int Cantidad { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ReposicionItemDTO dto &&
                   LibroId == dto.LibroId &&
                   Titulo == dto.Titulo &&
                   Autor == dto.Autor &&
                   PrecioReposicion == dto.PrecioReposicion &&
                   Cantidad == dto.Cantidad;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, Titulo, Autor, PrecioReposicion, Cantidad);
        }
    }
}