
namespace AppForSEII.API.Models
{
    [PrimaryKey(nameof(LibroId), nameof(SubastaId))]
    public class SubastaItem
    {
        public SubastaItem()
        {
        }

        public double PrecioPuja { get; set; }

        public string? Descripcion { get; set; }

        public int LibroId { get; set; }

        public Libro Libro { get; set; }

        public int SubastaId { get; set; }

        public Subasta Subasta { get; set; }

    }
  }