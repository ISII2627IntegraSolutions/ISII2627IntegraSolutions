namespace AppForSEII.API.Models
{
    // Clase base abstracta
   
    // Subclase GooglePay
    public class GooglePay : MetodosdePago
    {
        [Required(ErrorMessage = "El correo de GooglePay es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato de correo no es válido.")]
        public string Email { get; set; } = string.Empty;

        public GooglePay() { }

        public override bool Equals(object? obj) => base.Equals(obj);
    }
}