namespace AppForSEII.API.Models
{
      // Subclase PayPal
    public class PayPal : MetodosdePago
    {
        [Required(ErrorMessage = "El número de teléfono de PayPal es obligatorio.")]
        [Phone(ErrorMessage = "El número de teléfono no es válido.")]
        public string NumeroTelefono { get; set; } = string.Empty;

        public PayPal() { }

        public override bool Equals(object? obj) => base.Equals(obj);
    }
}