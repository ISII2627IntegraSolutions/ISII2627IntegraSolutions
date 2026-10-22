using System;
using System.ComponentModel.DataAnnotations;

namespace AppForSEII.API.DTOs{
    public class CompraItemDTO
    {
        public CompraItemDTO()
        {
            
        }
        public CompraItemDTO(int libroId, string titulo, string genero, string editorial, decimal precioCompra, int cantidad)
        {
            LibroId = libroId;
            Titulo = titulo;
            Genero = genero;
            Editorial = editorial;
            PrecioCompra = precioCompra;
            Cantidad = cantidad;
        }
        
        public int LibroId { get; set; }
        public string Titulo { get; set; }
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        public decimal PrecioCompra { get; set; }
        public string Genero {get; set;}
        public string Editorial {get; set;}
        [Range(2, int.MaxValue, ErrorMessage = "La cantidad de compra debe ser mayor que 1.")]
        public int Cantidad { get; set; }


        public override bool Equals(object? obj)
        {
            return obj is CompraItemDTO dto &&
                   LibroId == dto.LibroId &&
                   Titulo == dto.Titulo &&
                   Genero == dto.Genero &&
                   Editorial == dto.Editorial &&
                   PrecioCompra == dto.PrecioCompra &&
                   Cantidad == dto.Cantidad;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                LibroId,
                Titulo,
                Genero,
                Editorial,
                PrecioCompra,
                Cantidad);
        }

    
    }
}