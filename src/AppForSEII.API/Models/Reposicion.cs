
namespace AppForSEII.API.Models
{
    public class Reposicion
    {
        public Reposicion()
        {
            
        }
        public Reposicion( int reposicionId, DateTime fechaReposicion, double precioTotal, String comentario, MetodoDePago metodoDePago, IList<ReposicionItem> reposicionItem)
        {
           ReposicionId=reposicionId;
           FechaReposicion=fechaReposicion;
           MetodoDePago=metodoDePago;
           Comentario=comentario;
           reposicionItems=reposicionItem;
          
        }
        public int ReposicionId{get;set;}
        public string Comentario{get;set;}
        public double PrecioTotal{get;set;}
        [System.ComponentModel.DataAnnotations.Display(Name = "Fecha Reposicion")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaReposicion{get;set;}
        public MetodoDePago MetodoDePago{get;set;}
        public ApplicationUser ApplicationUser { get; set; }
        public IList<ReposicionItem> reposicionItems { get; set; }


    }
    public enum metododePagos
    {
        Visa,
        PayPal,
        GooglePay
    }
}