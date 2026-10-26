
namespace AppForSEII.API.DTOs.SubastarLibrosDTOs
{
    public class SubastaForCreateDTO
    {
        public SubastaForCreateDTO()
        {
        }

        public SubastaForCreateDTO(string customerUserName, string nombre, string apellidos, string telefono, string metodoDePago, IList<SubastaItemDTO> subastaItems)
        {
            CustomerUserName = customerUserName;
            Nombre = nombre;
            Apellidos = apellidos;
            Telefono = telefono;
            MetodoDePago = metodoDePago;
            SubastaItems = subastaItems;
        }

        [Required]
        [EmailAddress]
        public string CustomerUserName { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string Apellidos { get; set; }

        [Required]
        [Phone]
        public string Telefono { get; set; }

        [Required]
        [RegularExpression("^(Visa|GooglePay|PayPal)$", ErrorMessage = "El método de pago debe ser Visa, GooglePay o PayPal.")]
        public string MetodoDePago { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Debes seleccionar al menos un libro.")]
        public IList<SubastaItemDTO> SubastaItems { get; set; }

        // Precio total de la subasta = suma de las pujas
        public decimal PrecioTotal => SubastaItems.Sum(si => si.PrecioPuja);

        // Solo para Visa
        [CreditCard]
        public string? NumeroTarjeta { get; set; }

        public DateTime? FechaCaducidad { get; set; }

        // Solo para GooglePay
        [EmailAddress]
        public string? EmailGooglePay { get; set; }

        // Solo para PayPal
        [Phone]
        public string? TelefonoPayPal { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is SubastaForCreateDTO dto &&
                   CustomerUserName == dto.CustomerUserName &&
                   Nombre == dto.Nombre &&
                   Apellidos == dto.Apellidos &&
                   Telefono == dto.Telefono &&
                   MetodoDePago == dto.MetodoDePago &&
                   SubastaItems.SequenceEqual(dto.SubastaItems);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CustomerUserName, Nombre, Apellidos, Telefono, MetodoDePago, SubastaItems);
        }
    }
}