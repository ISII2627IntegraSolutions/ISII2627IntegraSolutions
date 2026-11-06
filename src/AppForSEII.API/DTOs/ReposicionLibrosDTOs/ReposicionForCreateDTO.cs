// DTO del POST del CU2 (pasos 5-6): lo que manda la web cuando se confirma la reposición
// Lo recibe CreateReposicion; ReposicionDetailDTO hereda de él y le añade Id y FechaReposicion
// Los atributos ([Required], [Phone]...) los comprueba ASP.NET solo antes de entrar al método: si fallan, 400 automático
namespace AppForSEII.API.DTOs.ReponerLibrosDTOs
{
    public class ReposicionForCreateDTO
    {
         // Constructor vacío: lo usa ASP.NET para convertir el JSON del POST en objeto
        public ReposicionForCreateDTO()
        {
        }
        // Constructor con parámetros (datos obligatorios); los de pago se rellenan aparte según el método
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
         // UserName (email) del usuario; CreateReposicion comprueba que esté registrado
        [Required]
        [EmailAddress]
        public string CustomerUserName { get; set; }
         // Paso 5: datos del usuario; CreateReposicion los guarda en su ApplicationUser (los dos apellidos juntos en Surname)
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
         // FA4: solo se aceptan estas 3 palabras
        // MODIFICAR: un método de pago nuevo se añade aquí, en el switch de CreateReposicion y en NombreMetodoDePago
        [Required]
        [RegularExpression("^(Visa|GooglePay|PayPal)$", ErrorMessage = "El método de pago debe ser Visa, GooglePay o PayPal.")]
        public string MetodoDePago { get; set; }
         // Comentario opcional; si se escribe, entre 20 y 100 caracteres
        [StringLength(100, MinimumLength = 20, ErrorMessage = "El comentario debe tener entre 20 y 100 caracteres.")]
        public string? Comentario { get; set; }
        // FA2: al menos un libro en la lista
        [Required]
        [MinLength(1, ErrorMessage = "Debes seleccionar al menos un libro.")]
        public IList<ReposicionItemDTO> ReposicionItems { get; set; }

        // Precio total = suma de precio de reposición x cantidad
        // (propiedad calculada con =>, no se guarda en ningún sitio)
        public decimal PrecioTotal => ReposicionItems.Sum(ri => ri.PrecioReposicion * ri.Cantidad);

        // Datos de pago: todos con ? (opcionales); CreateReposicion exige los del método elegido
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
         // Equals: compara los datos del usuario, el método, el comentario y los libros (no los datos de pago)
        // SequenceEqual compara las dos listas elemento a elemento usando el Equals de ReposicionItemDTO
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
         // GetHashCode: va siempre con Equals
        public override int GetHashCode()
        {
            return HashCode.Combine(CustomerUserName, Nombre, PrimerApellido, SegundoApellido, Telefono, MetodoDePago, Comentario, ReposicionItems);
        }
    }
}