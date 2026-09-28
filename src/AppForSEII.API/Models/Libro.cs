namespace AppForSEII.API.Models
{
    public class Libro
    {
        public Libro()
        {
            
        }
        public Libro(int Id, string Titulo, String Autor, DateTime FechaLanzamiento, decimal PrecioCompra, int Stock)
        {
            Id= Id;
            Titulo= Titulo;
            Autor= Autor;
            FechaLanzamiento= FechaLanzamiento;
            PrecioCompra= PrecioCompra;
            Stock= Stock;
        }
        public int Id {get; set;} 
        
        [StringLength(50, ErrorMessage = "Title name cannot be longer than 50 characters.")]
        public string Title { get; set; }

        public Genero Genero { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Fecha Lanzamiento")]
        public DateTime FechaLanzamiento { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Range(0.5, float.MaxValue, ErrorMessage = "Minimum price is 0.5 ")]
        [System.ComponentModel.DataAnnotations.Display(Name = "PrecioCompra")]
        [Precision(10, 2)]
        public decimal PrecioCompra { get; set; }

         [System.ComponentModel.DataAnnotations.Display(Name = "Stock")]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum quantity for Purchase is 1")]
        public int Stock { get; set; }

    
    }

}