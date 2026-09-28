using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppForMovies.Models
{
    public class Editorial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
    }
}