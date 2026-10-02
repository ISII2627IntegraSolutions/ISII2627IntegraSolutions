    namespace AppForSEII.API.Models {
    
    [PrimaryKey(nameof(LibroId), nameof(ReposicionId))]
    public class ReposicionItem
{
    public ReposicionItem()
    {
    }

    public ReposicionItem(int cantidadReposicion, Libro libro, Reposicion reposicion,double precioDeReposicion)
    {
        CantidadReposicion=cantidadReposicion;
        Libro=libro;
        Reposicion=reposicion;
        PrecioDeReposicion = precioDeReposicion;
    }


    public int LibroId{get;set;}
    public Libro Libro{get;set;}
    public int CantidadReposicion{get;set;}
    public Reposicion Reposicion{get;set;}
    public int ReposicionId{get;set;}
     public double PrecioDeReposicion { get; set; }
}
}