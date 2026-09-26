using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NuGet.Protocol.Plugins;
namespace AppForSEII.API.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Genero
    {
        public Genero()
        {
            
        }
        public Genero(string name)
        {
            Name= name;

        }
        public int Id { get; set;}

        [StringLength(50, ErrorMessage = "El nombre del genero no puede ser mayor de 50 caracteres", MinimumLength = 4)]
        public string Name { get; set; }
    }
}