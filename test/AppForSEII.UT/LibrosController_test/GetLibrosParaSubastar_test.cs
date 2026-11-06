using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.LibroDTOs;

namespace AppForSEII.UT.LibroController_test
{
    // Pruebas del select del CU3: GetLibrosParaSubastar
    public class GetLibrosParaSubastar_test : AppForSEII4SqliteUT
    {
        // Constructor: mete los datos de prueba en la base de datos en memoria
        public GetLibrosParaSubastar_test()
        {
            var genero = new Genero(1, "Novela", new List<Libro>());
            var alfaguara = new Editorial(1, "Alfaguara", new List<Libro>());
            var salamandra = new Editorial(2, "Salamandra", new List<Libro>());

            // Dos libros con stock
            var libro1 = new Libro("Cien años de soledad", "Gabriel García Márquez", new DateTime(1967, 5, 30), 19.95m, 12);
            libro1.Id = 1;
            libro1.TipoLibro = "Tapa dura";
            libro1.Editorial = alfaguara;
            libro1.Genero = genero;

            var libro2 = new Libro("El nombre del viento", "Patrick Rothfuss", new DateTime(2007, 3, 27), 24.90m, 8);
            libro2.Id = 2;
            libro2.TipoLibro = "Tapa blanda";
            libro2.Editorial = salamandra;
            libro2.Genero = genero;

            // Libro sin stock: no debe salir nunca
            var libro3 = new Libro("It", "Stephen King", new DateTime(1986, 9, 15), 18.00m, 0);
            libro3.Id = 3;
            libro3.TipoLibro = "Bolsillo";
            libro3.Editorial = alfaguara;
            libro3.Genero = genero;

            _context.Generos.Add(genero);
            _context.Editoriales.AddRange(alfaguara, salamandra);
            _context.Libros.AddRange(libro1, libro2, libro3);
            _context.SaveChanges();
        }

        // Casos: (filtro título, filtro editorial, libros que se esperan)
        public static IEnumerable<object?[]> TestCasesFor_GetLibrosParaSubastar_OK()
        {
            var libroDTO1 = new LibroParaSubastarDTO(1, "Cien años de soledad", "Alfaguara", "Gabriel García Márquez", "Tapa dura");
            var libroDTO2 = new LibroParaSubastarDTO(2, "El nombre del viento", "Salamandra", "Patrick Rothfuss", "Tapa blanda");

            var allTests = new List<object?[]>
            {
                // Sin filtros: todos los libros con stock, ordenados por título
                new object?[] { null, null, new List<LibroParaSubastarDTO> { libroDTO1, libroDTO2 } },
                // Filtro por título
                new object?[] { "nombre", null, new List<LibroParaSubastarDTO> { libroDTO2 } },
                // Filtro por editorial (It es de Alfaguara pero no tiene stock)
                new object?[] { null, "Alfaguara", new List<LibroParaSubastarDTO> { libroDTO1 } },
                // El único libro que coincide no tiene stock: lista vacía
                new object?[] { "It", null, new List<LibroParaSubastarDTO>() }
            };

            return allTests;
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_GetLibrosParaSubastar_OK))]
        public async Task GetLibrosParaSubastar_OK_test(string? titulo, string? editorial, List<LibroParaSubastarDTO> expectedLibros)
        {
            // Arrange
            var mock = new Mock<ILogger<LibroController>>();
            ILogger<LibroController> logger = mock.Object;
            var controller = new LibroController(_context, logger);

            // Act
            var result = await controller.GetLibrosParaSubastar(titulo, editorial);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualLibros = Assert.IsType<List<LibroParaSubastarDTO>>(okResult.Value);
            Assert.Equal(expectedLibros, actualLibros);
        }
    }
}