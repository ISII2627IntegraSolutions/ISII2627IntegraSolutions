// MODELO del CU3 Subastar libros: tabla Subastas de la base de datos (la "cabecera" de la subasta)
// Una subasta = un cliente puja por uno o varios libros; cada puja es un SubastaItem
// La crean CreateSubasta (POST) y SeedSubastas; la lee GetDetailsForSubasta (details)
namespace AppForSEII.API.Models
{
    public class Subasta
    {
        // Constructor vacío: lo usa Entity Framework al leer una fila de la tabla
        public Subasta()
        {
        }

        // Constructor con parámetros: lo usan CreateSubasta, SeedSubastas y las pruebas
        public Subasta(DateTime fechaSubasta, ApplicationUser applicationUser, MetodosdePago metodoDePago, IList<SubastaItem> subastaItems)
        {
            // Precio total = suma de las pujas; si la lista llega vacía da 0 (por eso CreateSubasta lo recalcula después)
            PrecioSubasta = subastaItems.Sum(si => si.PrecioPuja);
            FechaSubasta = fechaSubasta;
            ApplicationUser = applicationUser;
            MetodoDePago = metodoDePago;
            SubastaItems = subastaItems;
        }

        // Clave primaria; al ser int, la base de datos la autoincrementa (1, 2, 3...)
        [Key]
        public int Id { get; set; }

        // Display, DataType y DisplayFormat solo cambian cómo se muestra la fecha en la web (dd/MM/yyyy)
        [System.ComponentModel.DataAnnotations.Display(Name = "Fecha Subasta")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaSubasta { get; set; }

        // Precio total (paso 7); Precision(10, 2) = decimal(10,2) en SQL Server; decimal porque es dinero
        [Precision(10, 2)]
        public decimal PrecioSubasta { get; set; }

        // Cliente que hace la subasta (1 cliente : N subastas); EF crea la clave ajena ApplicationUserId
        public ApplicationUser ApplicationUser { get; set; }

        // Método de pago: puede ser un Visa, un PayPal o un GooglePay porque los tres heredan de MetodosdePago
        public MetodosdePago MetodoDePago { get; set; }

        // Pujas de la subasta (1 subasta : N SubastaItem); SubastaItem es la tabla intermedia con Libro
        // MODIFICAR: un dato nuevo en la subasta se añade aquí como propiedad, en el constructor y después hace falta una migración nueva
        public IList<SubastaItem> SubastaItems { get; set; }
    }
}