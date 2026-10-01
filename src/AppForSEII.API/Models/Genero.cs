using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NuGet.Protocol.Plugins;
namespace AppForSEII.API.Models
{
    public class Genero
    {
        public Genero()
        {
            
        }
        public Genero(int id, string nombre, IList<Libro> libros)
        {
            Id = id;
            Nombre = nombre;
            Libros = libros;
        }
        
        [Key]
        public int Id { get; set;}

        [Required]
        [StringLength(50, ErrorMessage = "El nombre del genero no puede ser mayor de 50 caracteres", MinimumLength = 4)]
        public string Nombre { get; set; }
        public IList<Libro> Libros { get; set; }
    }
}