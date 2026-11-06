// DTO del POST del CU3 (pasos 5-6): lo que manda la web cuando el cliente confirma la subasta
// Lo recibe CreateSubasta; SubastaDetailDTO hereda de él y le añade Id y FechaSubasta
// Los atributos ([Required], [Phone]...) los comprueba ASP.NET solo antes de entrar al método: si fallan, 400 automático
namespace AppForSEII.API.DTOs.SubastarLibrosDTOs
{
    public class SubastaForCreateDTO
    {
        // Constructor vacío: lo usa ASP.NET para convertir el JSON del POST en objeto
        public SubastaForCreateDTO()
        {
        }

        // Constructor con parámetros (datos obligatorios); los de pago se rellenan aparte según el método
        public SubastaForCreateDTO(string customerUserName, string nombre, string apellidos, string telefono, string metodoDePago, IList<SubastaItemDTO> subastaItems)
        {
            CustomerUserName = customerUserName;
            Nombre = nombre;
            Apellidos = apellidos;
            Telefono = telefono;
            MetodoDePago = metodoDePago;
            SubastaItems = subastaItems;
        }

        // UserName (email) del cliente; CreateSubasta comprueba que esté registrado
        [Required]
        [EmailAddress]
        public string CustomerUserName { get; set; }

        // Paso 5: datos del cliente; CreateSubasta los guarda en su ApplicationUser (Name, Surname, PhoneNumber)
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string Apellidos { get; set; }

        [Required]
        [Phone]
        public string Telefono { get; set; }

        // FA5: solo se aceptan estas 3 palabras
        // MODIFICAR: un método de pago nuevo se añade aquí, en el switch de CreateSubasta y en GetDetailsForSubasta
        [Required]
        [RegularExpression("^(Visa|GooglePay|PayPal)$", ErrorMessage = "El método de pago debe ser Visa, GooglePay o PayPal.")]
        public string MetodoDePago { get; set; }

        // FA4: al menos un libro en la lista de pujas
        [Required]
        [MinLength(1, ErrorMessage = "Debes seleccionar al menos un libro.")]
        public IList<SubastaItemDTO> SubastaItems { get; set; }

        // Precio total de la subasta = suma de las pujas (propiedad calculada con =>, no se guarda en ningún sitio)
        public decimal PrecioTotal => SubastaItems.Sum(si => si.PrecioPuja);

        // Datos de pago: todos con ? (opcionales); CreateSubasta exige los del método elegido
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

        // Equals: compara los datos del cliente, el método y las pujas (no los datos de pago)
        // SequenceEqual compara las dos listas elemento a elemento usando el Equals de SubastaItemDTO
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

        // GetHashCode: va siempre con Equals
        public override int GetHashCode()
        {
            return HashCode.Combine(CustomerUserName, Nombre, Apellidos, Telefono, MetodoDePago, SubastaItems);
        }
    }
}