namespace AppForSEII.API.DTOs.ReponerLibrosDTOs
{
    public class ReposicionDetailDTO : ReposicionForCreateDTO
    {
        public ReposicionDetailDTO()
        {
        }

        public ReposicionDetailDTO(int id, DateTime fechaReposicion, string customerUserName, string nombre, string primerApellido, string segundoApellido, string telefono, string metodoDePago, string? comentario, IList<ReposicionItemDTO> reposicionItems)
            : base(customerUserName, nombre, primerApellido, segundoApellido, telefono, metodoDePago, comentario, reposicionItems)
        {
            Id = id;
            FechaReposicion = fechaReposicion;
        }

        public int Id { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime FechaReposicion { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is ReposicionDetailDTO dto &&
                   base.Equals(obj) &&
                   Id == dto.Id &&
                   FechaReposicion == dto.FechaReposicion;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id, FechaReposicion);
        }
    }
}