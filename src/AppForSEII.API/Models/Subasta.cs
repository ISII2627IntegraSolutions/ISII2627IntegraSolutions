

namespace AppForSEII.API.Models{
    

public class Subasta{
    
    public Subasta(){
    }

    [Key]
    public int Id { get; set; }
    public DateTime FechaSubasta { get; set; }
    public double PrecioSubasta { get; set; }


    public ApplicationUser ApplicationUser { get; set; }

        
  public MetodoDePago MetodoDePago { get; set; }

  public IList<SubastaItem> SubastaItems { get; set; }

}
}