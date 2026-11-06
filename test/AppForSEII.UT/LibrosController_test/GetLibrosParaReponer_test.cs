using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.LibroDTOs;

namespace AppForSEII.UT.LibroController_test
{
    // Pruebas del select del CU2: GetLibrosParaReponer
    public class GetLibrosParaReponer_test : AppForSEII4SqliteUT
    {
        // Constructor: mete los datos de prueba en la base de datos en memoria
        public GetLibrosParaReponer_test()
        {
            var novela = new Genero(1, "Novela", new List<Libro>());
            var terror = new Genero(2, "Terror", new List<Libro>());
            var alfaguara = new Editorial(1, "Alfaguara", new List<Libro>());

            // Dos libros sin stock (se pueden reponer)
            var libro1 = new Libro("It", "Stephen King", new DateTime(1986, 9, 15), 18.00m, 0);
            libro1.Id = 1;
            libro1.TipoLibro = "Bolsillo";
            libro1.Editorial = alfaguara;
            libro1.Genero = terror;

            var libro2 = new Libro("Diez negritos", "Agatha Christie", new DateTime(1939, 11, 6), 10.50m, 0);
            libro2.Id = 2;
            libro2.TipoLibro = "Bolsillo";
            libro2.Editorial = alfaguara;
            libro2.Genero = novela;

            // Libro con stock: no debe salir nunca
            var libro3 = new Libro("Cien años de soledad", "Gabriel García Márquez", new DateTime(1967, 5, 30), 19.95m, 12);
            libro3.Id = 3;
            libro3.TipoLibro = "Tapa dura";
            libro3.Editorial = alfaguara;
            libro3.Genero = novela;

            _context.Generos.AddRange(novela, terror);
            _context.Editoriales.Add(alfaguara);
            _context.Libros.AddRange(libro1, libro2, libro3);
            _context.SaveChanges();
        }

        // Casos: (filtro autor, filtro género, libros que se esperan)
        public static IEnumerable<object?[]> TestCasesFor_GetLibrosParaReponer_OK()
        {
            var libroDTO1 = new LibroParaReponerDTO(1, "It", new DateTime(1986, 9, 15), "Stephen King", 18.00m, "Terror");
            var libroDTO2 = new LibroParaReponerDTO(2, "Diez negritos", new DateTime(1939, 11, 6), "Agatha Christie", 10.50m, "Novela");

            var allTests = new List<object?[]>
            {
                // Sin filtros: todos los libros sin stock, ordenados por título
                new object?[] { null, null, new List<LibroParaReponerDTO> { libroDTO2, libroDTO1 } },
                // Filtro por autor
                new object?[] { "King", null, new List<LibroParaReponerDTO> { libroDTO1 } },
                // Filtro por género (Cien años es Novela pero tiene stock)
                new object?[] { null, "Novela", new List<LibroParaReponerDTO> { libroDTO2 } },
                // El único libro que coincide tiene stock: lista vacía
                new object?[] { "García", null, new List<LibroParaReponerDTO>() }
            };

            return allTests;
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_GetLibrosParaReponer_OK))]
        public async Task GetLibrosParaReponer_OK_test(string? autor, string? genero, List<LibroParaReponerDTO> expectedLibros)
        {
            // Arrange
            var mock = new Mock<ILogger<LibroController>>();
            ILogger<LibroController> logger = mock.Object;
            var controller = new LibroController(_context, logger);

            // Act
            var result = await controller.GetLibrosParaReponer(autor, genero);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualLibros = Assert.IsType<List<LibroParaReponerDTO>>(okResult.Value);
            Assert.Equal(expectedLibros, actualLibros);
        }
    }
}