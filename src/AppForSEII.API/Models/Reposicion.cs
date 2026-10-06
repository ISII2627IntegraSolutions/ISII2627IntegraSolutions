

namespace AppForSEII.API.Models
{
    public class Reposicion
    {
        public Reposicion()
        {
            
        }
        public Reposicion( DateTime fechaReposicion, string? comentario, MetodosdePago metodoDePago, IList<ReposicionItem> reposicionItem)
        {
           PrecioTotal = reposicionItem.Sum(ri => ri.PrecioDeReposicion * ri.CantidadReposicion);
           FechaReposicion=fechaReposicion;
           MetodoDePago=metodoDePago;
           Comentario=comentario;
           reposicionItems=reposicionItem;
          
        }
        [Key]
        public int Id{get;set;}
        [Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Fecha Reposicion")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaReposicion{get;set;}
        [StringLength(100, MinimumLength = 20, ErrorMessage = "El comentario debe tener entre 20 y 100 caracteres.")]
        public string? Comentario{get;set;}
        public double PrecioTotal{get;set;}
        public MetodosdePago MetodoDePago{get;set;}
        public ApplicationUser ApplicationUser { get; set; }
        public IList<ReposicionItem> reposicionItems { get; set; }


    }
    
}