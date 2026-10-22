namespace AppForSEII.API.Models
{
    public class Compra
    {
        public Compra()
        {

        }
        public Compra(DateTime fechaCompra, string? codigoDescuento, MetodosdePago metodoDePago, IList<ComprarItem> comprarItems)
        {
            PrecioTotal = comprarItems.Sum(ri => ri.PrecioCompra * ri.Cantidad);
            FechaCompra = fechaCompra;
            CodigoDescuento = codigoDescuento;
            MetodoDePago = metodoDePago;
            ComprarItems = comprarItems;
        }
        [Key]
        public int Id { get; set; }
        [Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Fecha Compra")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaCompra { get; set; }

        [Precision(10, 2)]
        public decimal PrecioTotal { get; set; }

        [StringLength(10, MinimumLength = 5, ErrorMessage = "El código de descuento debe tener entre 5 y 10 caracteres.")]
        public string? CodigoDescuento { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public IList<ComprarItem> ComprarItems { get; set; }
        [Required]
        [StringLength(50)]
        public string Nombre {get; set;}
        [Required]
        [StringLength(100)]
        public string Apellidos {get; set;}
        [Required]
        [StringLength(50)]
        public string Direccion {get; set;}
        [Required]
        public string Telefono {get; set;}



        public MetodosdePago MetodoDePago { get; set; }
    }



}