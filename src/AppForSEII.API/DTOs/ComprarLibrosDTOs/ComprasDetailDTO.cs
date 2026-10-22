using AppForSEII.API.DTOs.ComprarLibrosDTOs;

namespace AppForSEII.API.DTOs
{
    public class ComprasDetailDTO : CompraForCreateDTO
    {
       public ComprasDetailDTO(int id, DateTime fechaCompra,string customerUserName, string nombreComprador, string apellidoComprador, string direccion, string telefonoComprador, string metodoDePago, string? codigoDescuento, IList<CompraItemDTO> compraItems): base(customerUserName,nombreComprador, apellidoComprador, direccion, telefonoComprador,metodoDePago, codigoDescuento, compraItems)
        {
            Id = id;
            FechaCompra = fechaCompra;

        }
        public int Id {get; set;}
        public DateTime FechaCompra {get; set;}
        public override bool Equals(object? obj)
        {
            return obj is ComprasDetailDTO dTO &&
            base.Equals(obj) &&
            Id == dTO.Id &&
            FechaCompra == dTO.FechaCompra;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id, FechaCompra);
        }
    }
}