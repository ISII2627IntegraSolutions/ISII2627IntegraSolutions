namespace AppForSEII.API.Models
{
    [PrimaryKey(nameof(LibroId), nameof(SubastaId))]
    public class SubastaItem
    {
        public SubastaItem()
        {
        }

        public SubastaItem(Libro libro, Subasta subasta, double precioPuja, string? descripcion)
        {
            Libro = libro;
            Subasta = subasta;
            PrecioPuja = precioPuja;
            Descripcion = descripcion;
        }

        [Range(1, double.MaxValue, ErrorMessage = "El precio de la puja debe ser como mínimo 1.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Precio Puja")]
        public double PrecioPuja { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 100 caracteres.")]
        public string? Descripcion { get; set; }

        public int LibroId { get; set; }

        public Libro Libro { get; set; }

        public int SubastaId { get; set; }

        public Subasta Subasta { get; set; }
    }
}