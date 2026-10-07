namespace AppForSEII.API.Models
{
    public class Compra
    {
        public Compra()
        {
            
        }
        public Compra(DateTime fechaCompra, string? codigoDescuento, MetodosdePago metodoDePago, IList<ComprarItem> comprarItems)
        {
            PrecioTotal = comprarItems.Sum(ri=> ri.Libro.PrecioCompra * ri.Cantidad);
            FechaCompra = fechaCompra;
            CodigoDescuento = codigoDescuento;
            MetodoDePago = metodoDePago;
            ComprarItems = comprarItems;
        }
        [Key]
        public int Id {get; set;}
        [Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Fecha Compra")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaCompra { get; set; }
        
        public double PrecioTotal { get; set; }

        [StringLength(10, MinimumLength = 5, ErrorMessage = "El código de descuento debe tener entre 5 y 10 caracteres.")]
        public string? CodigoDescuento { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public IList<ComprarItem> ComprarItems { get; set; }

        

         public MetodosdePago MetodoDePago { get; set; }
    }


    
}