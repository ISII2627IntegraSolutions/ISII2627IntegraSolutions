    namespace AppForSEII.API.Models {
    public class ReposicionItem
{
    public ReposicionItem()
    {
    }
    public ReposicionItem(Libro libro)
    {
       
    }
    public ReposicionItem(Libro libro, string? descripcion) : this(libro)
    {
        Descripcion = descripcion;
    }

    public ReposicionItem(int libroId ,  double precioDeReposicion)
    {
        libroId = libroId;
        PrecioDeReposicion = precioDeReposicion;
    }


    public Libro libro { get; set; }
    public int LibroId { get; set; }
    public int ReposicionId { get; set; }
    public string? Descripcion { get; set; }
    public double PrecioDeReposicion { get; set; }
}
}