namespace AppForSEII.API.Models
{
    public class Subasta
    {
        public Subasta()
        {
        }

        public Subasta(DateTime fechaSubasta, ApplicationUser applicationUser, MetodoDePago metodoDePago, IList<SubastaItem> subastaItems)
        {
            PrecioSubasta = subastaItems.Sum(si => si.PrecioPuja);
            FechaSubasta = fechaSubasta;
            ApplicationUser = applicationUser;
            MetodoDePago = metodoDePago;
            SubastaItems = subastaItems;
        }

        [Key]
        public int Id { get; set; }

        [Display(Name = "Fecha Subasta")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaSubasta { get; set; }

        public double PrecioSubasta { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        public MetodoDePago MetodoDePago { get; set; }

        public IList<SubastaItem> SubastaItems { get; set; }
    }
}

