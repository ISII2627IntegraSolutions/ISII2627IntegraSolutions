namespace AppForSEII.API.Models
{
    public class Compra
    {
        public Compra()
        {
            
        }
        public Compra(int CompraId, string CodigoDescuento, double PrecioTotal, DateTime FechaCompra, MetodoDePago metodoDePago, IList<ComprarItem> comprarItems)
        {
            CompraId = CompraId;
            FechaCompra = FechaCompra;
            CodigoDescuento = CodigoDescuento;
            MetodoDePago = metodoDePago;
            ComprarItems = comprarItems;
            PrecioTotal = comprarItems.Sum(item => item.Libro.PrecioCompra * item.Cantidad);
        }
        [Key]
        public int CompraId {get; set;}
        public double PrecioTotal { get; set; } 
        
         [System.ComponentModel.DataAnnotations.Display(Name = "Fecha Compra")]
         [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
         [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
         public DateTime FechaCompra { get; set; }
        public MetodoDePago MetodoDePago { get; set; }


        public ApplicationUser ApplicationUser { get; set; }
        public IList<ComprarItem> ComprarItems { get; set; }
    }

    public enum MetodoDePago
    {
        CreditCard,
        PayPal,
        Cash
    }

    
}