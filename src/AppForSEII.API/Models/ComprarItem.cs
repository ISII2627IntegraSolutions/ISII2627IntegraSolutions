namespace AppForSEII.API.Models
{
    [PrimaryKey(nameof(LibroId), nameof(CompraId))]
    public class ComprarItem
    {
        public ComprarItem()
        {
            
        }
        public ComprarItem(Libro libro, int cantidad, Compra compra)
        {
            Libro = libro;
            Cantidad = cantidad;
            Compra = compra;
        }
        public Libro Libro { get; set; }
        public int LibroId { get; set; }
        public int Cantidad { get; set; }
        public Compra Compra { get; set; }
        public int CompraId { get; set; }
        
    }
}