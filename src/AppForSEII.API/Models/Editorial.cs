using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.Models
{
    public class Editorial
    {
        public Editorial()
        {
            
        }
        public Editorial(int id, string nombre, IList<Libro> libros)
        {
            Id = id;
            Nombre = nombre;
            Libros = libros;
        }
        //añadimos otro constructor para el testing. no modificamos el anterior para evitar conflictos con las clases donde se use
        public Editorial(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }
        
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        public IList<Libro> Libros { get; set; }
    }
}