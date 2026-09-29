namespace AppForSEII.API.Models
{
    public class ComprarItem
    {
        public ComprarItem()
        {
            
        }
        public ComprarItem(Libro libro, int cantidad, Comprar compra)
        {
            Libro = libro;
            Cantidad = cantidad;
            Compra = compra;
        }
        public Libro Libro { get; set; }
        public int Cantidad { get; set; }
        public Comprar Compra { get; set; }
        
    }
}