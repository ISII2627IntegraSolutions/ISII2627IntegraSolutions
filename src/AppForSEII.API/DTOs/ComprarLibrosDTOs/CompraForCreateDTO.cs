using System.ComponentModel;
using System.Data;
using System.Text.Json.Serialization;

namespace AppForSEII.API.DTOs.ComprarLibrosDTOs
{
    public class CompraForCreateDTO
    {


        public CompraForCreateDTO(string customerUserName, string nombreComprador, string apellidoComprador, string direccion, string telefonoComprador, string metododePago, string? codigoDescuento, IList<CompraItemDTO> compraItems)
        {
            CustomerUserName = customerUserName;
            NombreComprador = nombreComprador ?? throw new ArgumentNullException(nameof(nombreComprador));
            ApellidoComprador = apellidoComprador ?? throw new ArgumentNullException(nameof(apellidoComprador));
            Direccion = direccion ?? throw new ArgumentNullException(nameof(direccion));
            TelefonoComprador = telefonoComprador;
            MetodoDePago = metododePago;
            CodigoDescuento = codigoDescuento;
            CompraItems = compraItems ?? throw new ArgumentNullException(nameof(compraItems));
        }
        [Required]
        [EmailAddress]
        public string CustomerUserName { get; set; }
        [Required]
        [StringLength(50)]
        public string NombreComprador { get; set; }

        [Required]
        [StringLength(100)]
        public string ApellidoComprador { get; set; }

        [Required]
        [StringLength(50)]
        public string Direccion { get; set; }

        [Required]
        public string TelefonoComprador { get; set; }
        [StringLength(10, MinimumLength = 5, ErrorMessage = "El código de descuento debe tener entre 5 y 10 caracteres.")]
        public string? CodigoDescuento { get; set; }
        [Required]
        [RegularExpression("^(Visa|GooglePay|PayPal)$", ErrorMessage = "El método de pago debe ser Visa, GooglePay o PayPal.")]
        public string MetodoDePago { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "Debes seleccionar al menos un libro.")]
        public IList<CompraItemDTO> CompraItems { get; set; }

        [DisplayName("PrecioTotal")]
        [JsonPropertyName("PrecioTotal")]
        public decimal PrecioTotal
        {
            get
            {
                return CompraItems.Sum(ri => ri.PrecioCompra * ri.Cantidad);
            }
        }
        public override bool Equals(object? obj)
        {
            return obj is CompraForCreateDTO dTO &&
            CustomerUserName == dTO.CustomerUserName &&
            NombreComprador == dTO.NombreComprador &&
            ApellidoComprador == dTO.ApellidoComprador &&
            Direccion == dTO.Direccion &&
            TelefonoComprador == dTO.TelefonoComprador &&
            MetodoDePago == dTO.MetodoDePago &&
            CodigoDescuento == dTO.CodigoDescuento &&
            CompraItems.SequenceEqual(dTO.CompraItems) &&
            PrecioTotal == dTO.PrecioTotal;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(
                CustomerUserName,
                NombreComprador,
                ApellidoComprador,
                Direccion,
                TelefonoComprador,
                MetodoDePago,
                CodigoDescuento,
                PrecioTotal);
        }




    }

}