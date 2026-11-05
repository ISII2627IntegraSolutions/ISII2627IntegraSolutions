using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.LibroDTOs;


namespace AppForSEII.UT.LibroController_test
{
    public class GetLibrosParaComprar_test : AppForSEII4SqliteUT
    {
        public GetLibrosParaComprar_test()
        {
            var generos = new List<Genero>()
            {
                new Genero(1, "Novela"),
                new Genero(2, "Fantasía"),
                new Genero(3, "Ciencia Ficción")
            };

            var editoriales = new List<Editorial>
            {
                new Editorial(1, "Planeta"),
                new Editorial(2, "Anagrama"),
                new Editorial(3, "Alfaguara")
            };

            var libros = new List<Libro>()
{
    new Libro(1, "Cien años de soledad", "Gabriel García Márquez",19.95m, 10, new DateTime(1967, 5, 30),generos[0], editoriales[2])
    {
        TipoLibro = "Tapa dura"
    },

    new Libro(2, "El nombre del viento", "Patrick Rothfuss",24.90m, 5, new DateTime(2007, 3, 27),generos[1], editoriales[1])
    {
        TipoLibro = "Tapa blanda"
    },

    new Libro(3, "Dune", "Frank Herbert",21.50m, 8, new DateTime(1965, 8, 1),generos[2], editoriales[1])
    {
        TipoLibro = "Bolsillo"
    },

    new Libro(4, "1984", "George Orwell",14.95m, 0, new DateTime(1949, 6, 8),generos[0], editoriales[0])
    {
        TipoLibro = "Bolsillo"
    }
};

            _context.AddRange(generos);
            _context.AddRange(editoriales);
            _context.AddRange(libros);
            _context.SaveChanges();
        }

        // MemberData para identificar las fuentes de datos de la prueba parametrizada. Cada objeto de la lista representa un conjunto de parámetros para una ejecución de prueba.
        public static IEnumerable<object[]> TestCasesFor_GetLibrosParaComprar_OK()
        {
            var expectedLibros = new List<LibroParaComprarDTO>()
            {
                new LibroParaComprarDTO(1, "Cien años de soledad", new DateTime(1967, 5, 30), "Alfaguara", 19.95m, "Novela"),
                new LibroParaComprarDTO(2, "El nombre del viento", new DateTime(2007, 3, 27), "Anagrama", 24.90m, "Fantasía"),
                new LibroParaComprarDTO(3, "Dune", new DateTime(1965, 8, 1), "Anagrama", 21.50m, "Ciencia Ficción")
            };

            //CASO 1: Sin filtros, se espera que devuelva todos los libros con stock > 0.
            var librosSinFiltros = new List<LibroParaComprarDTO>()
            {
                expectedLibros[0],
                expectedLibros[2],
                expectedLibros[1]
            };

            //CASO 2: Filtro por género "Fantasía", se espera que devuelva solo el libro con Id = 2.
            var librosFiltradosPorGenero = new List<LibroParaComprarDTO>()
            {
                expectedLibros[1]
            };

            //CASO 3: Filtrando por precio de compra maximo de 22.00, se espera que devuelva los libros con Id = 1 y Id = 3.
            var librosFiltradosPorPrecioMaximo = new List<LibroParaComprarDTO>()
            {
                expectedLibros[0],
                expectedLibros[2]
            };

            //CASO 4: Filtrando por género "Novela" y precio de compra maximo de 20.00, se espera que devuelva solo el libro con Id = 1.
            var librosFiltradosPorGeneroYPrecioMaximo = new List<LibroParaComprarDTO>
            {
                expectedLibros[0]
            };

            var allTest = new List<object[]>
            {
                new object[] { null, null, librosSinFiltros },
                new object[] { "Fantasía", null, librosFiltradosPorGenero },
                new object[] { null, 22.00m, librosFiltradosPorPrecioMaximo },
                new object[] { "Novela", 20.00m, librosFiltradosPorGeneroYPrecioMaximo }
            };

            return allTest;
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_GetLibrosParaComprar_OK))]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetLibrosParaComprar_OK_test(string? filtroGenero, decimal? filtroPrecioCompra, IList<LibroParaComprarDTO> expectedLibros)
        {
            // Arrange
            var mock = new Mock<ILogger<LibroController>>();
            ILogger<LibroController> logger = mock.Object;
            var controller = new LibroController(_context, logger);

            // Act
            var result = await controller.GetLibrosParaComprar(filtroGenero, filtroPrecioCompra);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var libroDTOsActual = Assert.IsType<List<LibroParaComprarDTO>>(okResult.Value);

            // Comparar las listas de libros esperadas y obtenidas.
            Assert.Equal(expectedLibros, libroDTOsActual);
        }
    }
}