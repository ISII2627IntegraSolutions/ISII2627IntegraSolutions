
// MODELO del CU2 Reponer libros: tabla Reposiciones de la base de datos (la "cabecera" de la reposición)
// Una reposición = se piden más unidades de uno o varios libros sin stock; cada libro es un ReposicionItem
// La crean CreateReposicion (POST) y SeedReposiciones; la lee GetDetailsForReposicion (details)
namespace AppForSEII.API.Models
{
    public class Reposicion
    {
         // Constructor vacío: lo usa Entity Framework al leer una fila de la tabla
        public Reposicion()
        {

        }
        // Constructor con parámetros: lo usan CreateReposicion, SeedReposiciones y las pruebas (el usuario se asigna aparte)
        public Reposicion(DateTime fechaReposicion, string? comentario, MetodosdePago metodoDePago, IList<ReposicionItem> reposicionItem)
        {
            // Precio total = suma de (precio x cantidad); si la lista llega vacía da 0 (por eso CreateReposicion lo recalcula)
            PrecioTotal = reposicionItem.Sum(ri => ri.PrecioDeReposicion * ri.CantidadReposicion);
            FechaReposicion = fechaReposicion;
            MetodoDePago = metodoDePago;
            Comentario = comentario;
            reposicionItems = reposicionItem;

        }
        // Clave primaria; al ser int, la base de datos la autoincrementa (1, 2, 3...)
        [Key]
        public int Id { get; set; }
        // Fecha obligatoria; Display, DataType y DisplayFormat solo cambian cómo se muestra (dd/MM/yyyy)
        [Required]
        [System.ComponentModel.DataAnnotations.Display(Name = "Fecha Reposicion")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime FechaReposicion { get; set; }
        // Comentario opcional (? = puede ser null); si se escribe, entre 20 y 100 caracteres
        // MODIFICAR: si cambian la longitud, se cambia aquí y también en ReposicionForCreateDTO
        [StringLength(100, MinimumLength = 20, ErrorMessage = "El comentario debe tener entre 20 y 100 caracteres.")]
        public string? Comentario { get; set; }
        // Precio total; Precision(10, 2) = decimal(10,2) en SQL Server; decimal porque es dinero
        [Precision(10, 2)]
        public decimal PrecioTotal { get; set; }
        // Método de pago: puede ser un Visa, un PayPal o un GooglePay porque los tres heredan de MetodosdePago
        public MetodosdePago MetodoDePago { get; set; }
        // Usuario que hace la reposición (1 usuario : N reposiciones); no va en el constructor, se asigna después
        public ApplicationUser ApplicationUser { get; set; }
        // Libros de la reposición (1 reposición : N ReposicionItem); ojo: el nombre empieza en minúscula
        // MODIFICAR: un dato nuevo se añade aquí como propiedad, en el constructor y después hace falta una migración nueva
        public IList<ReposicionItem> reposicionItems { get; set; }

    }

}