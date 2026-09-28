namespace AppForSEII.API.Models
{
    public class Comprar
    {
        public Comprar()
        {
            
        }
        public Comprar(string CompradorUserName, string CompradorNameSurname, ApplicationUser Comprador, string DireccionEnvio, int telefono, string tituloLibro, int LibroId, int Cantidad, decimal Precio, DateTime FechaCompra)
        {
        
            LibroId= LibroId;
            LibroId= LibroId;
            Cantidad= Cantidad;
            Precio= Precio;
            FechaCompra= FechaCompra;
            DireccionEnvio= DireccionEnvio;
            
        }
        public int Id {get; set;}
        public double PrecioTotal { get; set; } 
        
        [System.ComponentModel.DataAnnotations.Display(Name = "Libro")]
        public int LibroId { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Cantidad")]
        [Range(1, int.MaxValue, ErrorMessage = "Minimum quantity for Purchase is 1")]
        public int Cantidad { get; set; }

         [System.ComponentModel.DataAnnotations.Display(Name = "Fecha Compra")]
         [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
         [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
         public DateTime FechaCompra { get; set; }

         [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Dirección de envío")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please, set your address for delivery")]
        public string DireccionEnvio { get; set; }

         public string CompradorUserName { get; set; }

        public string CompradorNameSurname { get; set; }
        [System.ComponentModel.DataAnnotations.Display(Name = "Metodo de pago")]
        public MetodoDePago MetodoDePago { get; set; }


        public ApplicationUser ApplicationUser { get; set; }
    }

    public enum MetodoDePago
    {
        CreditCard,
        PayPal,
        Cash
    }

    
}