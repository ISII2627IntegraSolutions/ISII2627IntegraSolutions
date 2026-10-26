// SubastaDetailDTO: resumen de la subasta con su Id y fecha 
namespace AppForSEII.API.DTOs.SubastarLibrosDTOs
{
    public class SubastaDetailDTO : SubastaForCreateDTO
    {

        public SubastaDetailDTO()
        {
        }
        
        public SubastaDetailDTO(int id, DateTime fechaSubasta, string customerUserName, string nombre, string apellidos, string telefono, string metodoDePago, IList<SubastaItemDTO> subastaItems)
            : base(customerUserName, nombre, apellidos, telefono, metodoDePago, subastaItems)
        {
            Id = id;
            FechaSubasta = fechaSubasta;
        }

        public int Id { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime FechaSubasta { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is SubastaDetailDTO dto &&
                   base.Equals(obj) &&
                   Id == dto.Id &&
                   FechaSubasta == dto.FechaSubasta;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id, FechaSubasta);
        }
    }
}