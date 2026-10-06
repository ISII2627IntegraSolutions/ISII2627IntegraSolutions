namespace AppForSEII.API.Models
{
    // Clase base abstracta
    // Subclase Visa
    public class Visa : MetodosdePago
    {
        [Required(ErrorMessage = "El número de tarjeta es obligatorio.")]
        [CreditCard(ErrorMessage = "El número de tarjeta no es válido.")]
        public string NumeroTarjeta { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de caducidad es obligatoria.")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime FechaCaducidad { get; set; }

        public Visa() { }

        public override bool Equals(object? obj) => base.Equals(obj);
    }
}