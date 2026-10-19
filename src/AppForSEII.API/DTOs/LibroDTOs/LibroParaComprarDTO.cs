using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppForSEII.API.DTOs.LibroDTOs
{
    public class LibroParaComprarDTO
    {
        public LibroParaComprarDTO()
        {
            
        }
        public LibroParaComprarDTO(int id, string titulo, DateTime fechaLanzamiento, string editorial, decimal precioCompra, Genero genero)
        {
            Id = id;
            Titulo = titulo;
            FechaLanzamiento = fechaLanzamiento;
            Editorial = editorial;
            PrecioCompra = precioCompra;
            Genero = genero;
        }
        

        public int Id { get; set; }

        [StringLength(50, ErrorMessage = "El título no puede tener más de 50 caracteres.")]
        public string Titulo { get; set; }
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Fecha Lanzamiento")]
        public DateTime FechaLanzamiento { get; set; }

        [StringLength(50, ErrorMessage = "La editorial no puede tener más de 50 caracteres.")]
        public string Editorial { get; set; }

        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Range(typeof(decimal), "0.50", "99999999.99",
        ErrorMessage = "El precio debe estar entre 0,50 y 99999999,99.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "PrecioCompra")]
        [Precision(10, 2)]
        public decimal PrecioCompra { get; set; }
        [StringLength(50, ErrorMessage = "El género no puede tener más de 50 caracteres.")]
        public Genero Genero { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is LibroParaComprarDTO dto &&
                   Id == dto.Id &&
                   Titulo == dto.Titulo &&
                   FechaLanzamiento == dto.FechaLanzamiento &&
                   Editorial == dto.Editorial &&
                   PrecioCompra == dto.PrecioCompra &&
                   Genero == dto.Genero;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Titulo, FechaLanzamiento, Editorial, PrecioCompra, Genero);
        }
    }

}