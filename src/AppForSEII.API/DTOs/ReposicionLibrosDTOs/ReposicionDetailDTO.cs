// DTO del DETAILS del CU2: resumen de una reposición ya guardada
// Hereda de ReposicionForCreateDTO (tiene todos sus datos) y añade Id y FechaReposicion
// Lo devuelven GetDetailsForReposicion (200 OK) y CreateReposicion (201 Created), los dos a través de CrearDetalle
namespace AppForSEII.API.DTOs.ReponerLibrosDTOs
{
         // ": ReposicionForCreateDTO" = herencia
    public class ReposicionDetailDTO : ReposicionForCreateDTO
    {
        // Constructor vacío
        public ReposicionDetailDTO()
        {
        }
        // Constructor con parámetros: ": base(...)" pasa los datos comunes al constructor del padre
        public ReposicionDetailDTO(int id, DateTime fechaReposicion, string customerUserName, string nombre, string primerApellido, string segundoApellido, string telefono, string metodoDePago, string? comentario, IList<ReposicionItemDTO> reposicionItems)
            : base(customerUserName, nombre, primerApellido, segundoApellido, telefono, metodoDePago, comentario, reposicionItems)
        {
            Id = id;
            FechaReposicion = fechaReposicion;
        }
         // Id de la reposición en la base de datos
        public int Id { get; set; }
         // Fecha en que se hizo la reposición
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime FechaReposicion { get; set; }
        // Equals: base.Equals compara los datos del padre y aquí se añaden Id y FechaReposicion
        public override bool Equals(object? obj)
        {
            return obj is ReposicionDetailDTO dto &&
                   base.Equals(obj) &&
                   Id == dto.Id &&
                   FechaReposicion == dto.FechaReposicion;
        }
         // GetHashCode: incluye el del padre más Id y FechaReposicion
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id, FechaReposicion);
        }
    }
}