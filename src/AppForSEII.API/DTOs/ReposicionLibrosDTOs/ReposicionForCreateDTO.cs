namespace AppForSEII.API.DTOs.ReponerLibrosDTOs
{
    public class ReposicionForCreateDTO
    {
        public ReposicionForCreateDTO()
        {
        }

        public ReposicionForCreateDTO(string customerUserName, string nombre, string primerApellido, string segundoApellido, string telefono, string metodoDePago, string? comentario, IList<ReposicionItemDTO> reposicionItems)
        {
            CustomerUserName = customerUserName;
            Nombre = nombre;
            PrimerApellido = primerApellido;
            SegundoApellido = segundoApellido;
            Telefono = telefono;
            MetodoDePago = metodoDePago;
            Comentario = comentario;
            ReposicionItems = reposicionItems;
        }

        [Required]
        [EmailAddress]
        public string CustomerUserName { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(50)]
        public string PrimerApellido { get; set; }

        [Required]
        [StringLength(50)]
        public string SegundoApellido { get; set; }

        [Required]
        [Phone]
        public string Telefono { get; set; }

        [Required]
        [RegularExpression("^(Visa|GooglePay|PayPal)$", ErrorMessage = "El método de pago debe ser Visa, GooglePay o PayPal.")]
        public string MetodoDePago { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "El comentario debe tener entre 20 y 100 caracteres.")]
        public string? Comentario { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Debes seleccionar al menos un libro.")]
        public IList<ReposicionItemDTO> ReposicionItems { get; set; }

        // Precio total = suma de precio de reposición x cantidad
        public decimal PrecioTotal => ReposicionItems.Sum(ri => ri.PrecioReposicion * ri.Cantidad);

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
            return obj is ReposicionForCreateDTO dto &&
                   CustomerUserName == dto.CustomerUserName &&
                   Nombre == dto.Nombre &&
                   PrimerApellido == dto.PrimerApellido &&
                   SegundoApellido == dto.SegundoApellido &&
                   Telefono == dto.Telefono &&
                   MetodoDePago == dto.MetodoDePago &&
                   Comentario == dto.Comentario &&
                   ReposicionItems.SequenceEqual(dto.ReposicionItems);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CustomerUserName, Nombre, PrimerApellido, SegundoApellido, Telefono, MetodoDePago, Comentario, ReposicionItems);
        }
    }
}