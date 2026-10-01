namespace AppForSEII.API.Models
{
    public class Libro
    {
        public Libro()
        {
            
        }

        public Libro(string titulo, string autor, DateTime fechaLanzamiento, double precioCompra, int stock, int editorialId, int generoId)
        {
            Titulo = titulo;
            Autor = autor;
            FechaLanzamiento = fechaLanzamiento;
            PrecioCompra = precioCompra;
            Stock = stock;
        }
    
        [Key]
        public int Id {get; set;} 
        
        [Required, StringLength(50, ErrorMessage = "El titulo no puede tener más de 50 caracteres.")]
        public string Titulo { get; set; }

        [Required]
        public string Autor { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Fecha Lanzamiento")]
        public DateTime FechaLanzamiento { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Range(0.5, float.MaxValue, ErrorMessage = "Precio de venta debe ser mayor a 0.5")]
        [System.ComponentModel.DataAnnotations.Display(Name = "PrecioCompra")]
        [Precision(10, 2)]
        public double PrecioCompra { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Stock")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad mínima para la compra es 1")]
        public int Stock { get; set; }
        
        // Relaciones con otras entidades
        public Editorial Editorial { get; set; }
        public Genero Genero { get; set; }

        public IList<ComprarItem> ComprarItems { get; set; }
    
    }

}