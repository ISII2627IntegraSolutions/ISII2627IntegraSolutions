namespace AppForSEII.API.Models
{

    [PrimaryKey(nameof(LibroId), nameof(ReposicionId))]
    public class ReposicionItem
    {
        public ReposicionItem()
        {
        }

        public ReposicionItem(int cantidadReposicion, Libro libro, Reposicion reposicion, decimal precioDeReposicion)
        {
            CantidadReposicion = cantidadReposicion;
            Libro = libro;
            Reposicion = reposicion;
            PrecioDeReposicion = precioDeReposicion;
        }


        public int LibroId { get; set; }
        public Libro Libro { get; set; }
        public int CantidadReposicion { get; set; }
        public Reposicion Reposicion { get; set; }
        public int ReposicionId { get; set; }
        [Precision(10, 2)]
        public decimal PrecioDeReposicion { get; set; }
    }
}