// DTO del DETAILS del CU3 (paso 7): resumen de una subasta ya guardada
// Hereda de SubastaForCreateDTO (tiene todos sus datos) y añade Id y FechaSubasta
// Lo devuelven GetDetailsForSubasta (200 OK) y CreateSubasta (201 Created)
namespace AppForSEII.API.DTOs.SubastarLibrosDTOs
{
    // ": SubastaForCreateDTO" = herencia
    public class SubastaDetailDTO : SubastaForCreateDTO
    {

        // Constructor vacío
        public SubastaDetailDTO()
        {
        }
        
        // Constructor con parámetros: ": base(...)" pasa los datos comunes al constructor del padre
        public SubastaDetailDTO(int id, DateTime fechaSubasta, string customerUserName, string nombre, string apellidos, string telefono, string metodoDePago, IList<SubastaItemDTO> subastaItems)
            : base(customerUserName, nombre, apellidos, telefono, metodoDePago, subastaItems)
        {
            Id = id;
            FechaSubasta = fechaSubasta;
        }

        // Id de la subasta en la base de datos
        public int Id { get; set; }

        // Fecha en que se hizo la subasta
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime FechaSubasta { get; set; }

        // Equals: base.Equals compara los datos del padre y aquí se añaden Id y FechaSubasta
        public override bool Equals(object? obj)
        {
            return obj is SubastaDetailDTO dto &&
                   base.Equals(obj) &&
                   Id == dto.Id &&
                   FechaSubasta == dto.FechaSubasta;
        }

        // GetHashCode: incluye el del padre más Id y FechaSubasta
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id, FechaSubasta);
        }
    }
}