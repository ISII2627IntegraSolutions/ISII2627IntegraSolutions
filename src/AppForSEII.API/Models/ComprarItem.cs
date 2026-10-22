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
            LibroId = libro.Id;
            Cantidad = cantidad;
            Compra = compra;
            PrecioCompra = libro.PrecioCompra;
        }
        public Libro Libro { get; set; }
        public int LibroId { get; set; }
        [Range(2, int.MaxValue, ErrorMessage = "La cantidad de compra debe ser mayor que 1.")]
        [Precision(10, 2)]
        public int Cantidad { get; set; }
        [Precision(10, 2)]
        public decimal PrecioCompra { get; set; }
        public Compra Compra { get; set; }
        public int CompraId { get; set; }
        

    }
}