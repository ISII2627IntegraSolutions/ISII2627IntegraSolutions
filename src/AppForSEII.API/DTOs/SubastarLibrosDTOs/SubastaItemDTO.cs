// DTO del CU3: un libro dentro de la subasta, con su puja y su descripción
// Va dentro de SubastaForCreateDTO (lo que recibe el POST) y de SubastaDetailDTO (lo que devuelven el details y el POST)
namespace AppForSEII.API.DTOs.SubastarLibrosDTOs
{
    public class SubastaItemDTO
    {
        // Constructor vacío: lo usa ASP.NET para convertir el JSON del POST en objeto
        public SubastaItemDTO()
        {
        }

        // Constructor con parámetros: lo usan GetDetailsForSubasta, CreateSubasta y las pruebas
        public SubastaItemDTO(int libroId, string titulo, string autor, string editorial, decimal precioPuja, string? descripcion)
        {
            LibroId = libroId;
            Titulo = titulo;
            Autor = autor;
            Editorial = editorial;
            PrecioPuja = precioPuja;
            Descripcion = descripcion;
        }

        // Id del libro por el que se puja; CreateSubasta lo busca en la base de datos
        public int LibroId { get; set; }

        // Título, autor y editorial: datos del libro para enseñarlos en el resumen
        public string Titulo { get; set; }

        public string Autor { get; set; }

        public string Editorial { get; set; }

        // Currency = se muestra como dinero
        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        // FA2: puja mínima 1. Las opciones Invariant hacen que "99999999.99" se lea bien con la cultura española (sin ellas daba error 500)
        // MODIFICAR: para cambiar la puja mínima se cambia el "1" aquí y el "item.PrecioPuja < 1" de CreateSubasta
                [Range(typeof(decimal), "1", "99999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "El precio de la puja debe ser como mínimo 1.")]
        public decimal PrecioPuja { get; set; }

        // Paso 5: descripción opcional; si se escribe, entre 20 y 100 caracteres
        [StringLength(100, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 100 caracteres.")]
        public string? Descripcion { get; set; }

        // Equals: compara todos los campos; lo usan SequenceEqual (en SubastaForCreateDTO) y Assert.Equal
        public override bool Equals(object? obj)
        {
            return obj is SubastaItemDTO dto &&
                   LibroId == dto.LibroId &&
                   Titulo == dto.Titulo &&
                   Autor == dto.Autor &&
                   Editorial == dto.Editorial &&
                   PrecioPuja == dto.PrecioPuja &&
                   Descripcion == dto.Descripcion;
        }

        // GetHashCode: va siempre con Equals y usa los mismos campos
        public override int GetHashCode()
        {
            return HashCode.Combine(LibroId, Titulo, Autor, Editorial, PrecioPuja, Descripcion);
        }
    }
}