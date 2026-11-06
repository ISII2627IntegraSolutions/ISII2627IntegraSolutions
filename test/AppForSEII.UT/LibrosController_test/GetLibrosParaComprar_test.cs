
using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.LibroDTOs;

// Namespace donde organizamos las pruebas del controlador Libro.
namespace AppForSEII.UT.LibroController_test
{
    // Clase de pruebas para el método GetLibrosParaComprar.
    // Hereda de AppForSEII4SqliteUT para utilizar SQLite como base de datos
    // durante las pruebas unitarias.
    public class GetLibrosParaComprar_test : AppForSEII4SqliteUT
    {
        // CONSTRUCTOR:
        // Prepara los datos que utilizarán las pruebas.
        public GetLibrosParaComprar_test()
        {
            // ARRANGE: creamos los géneros de los libros.
            var generos = new List<Genero>()
            {
                new Genero(1, "Novela"),
                new Genero(2, "Fantasía"),
                new Genero(3, "Ciencia Ficción")
            };

            // ARRANGE: creamos las editoriales.
            var editoriales = new List<Editorial>
            {
                new Editorial(1, "Planeta"),
                new Editorial(2, "Anagrama"),
                new Editorial(3, "Alfaguara")
            };

            // ARRANGE: creamos cuatro libros con diferentes características.
            // Esto permite comprobar los filtros de género, precio y stock.
            var libros = new List<Libro>()
            {
                // Libro 1: Novela, precio 19.95 y stock 10.
                new Libro(
                    1,
                    "Cien años de soledad",
                    "Gabriel García Márquez",
                    19.95m,
                    10,
                    new DateTime(1967, 5, 30),
                    generos[0],
                    editoriales[2])
                {
                    TipoLibro = "Tapa dura"
                },

                // Libro 2: Fantasía, precio 24.90 y stock 5.
                new Libro(
                    2,
                    "El nombre del viento",
                    "Patrick Rothfuss",
                    24.90m,
                    5,
                    new DateTime(2007, 3, 27),
                    generos[1],
                    editoriales[1])
                {
                    TipoLibro = "Tapa blanda"
                },

                // Libro 3: Ciencia Ficción, precio 21.50 y stock 8.
                new Libro(
                    3,
                    "Dune",
                    "Frank Herbert",
                    21.50m,
                    8,
                    new DateTime(1965, 8, 1),
                    generos[2],
                    editoriales[1])
                {
                    TipoLibro = "Bolsillo"
                },

                // Libro 4: Novela, precio 14.95 y stock 0.
                // Se utiliza para comprobar que los libros sin stock
                // no aparecen en los resultados.
                new Libro(
                    4,
                    "1984",
                    "George Orwell",
                    14.95m,
                    0,
                    new DateTime(1949, 6, 8),
                    generos[0],
                    editoriales[0])
                {
                    TipoLibro = "Bolsillo"
                }
            };

            // Añadimos los géneros a la base de datos de pruebas.
            _context.AddRange(generos);

            // Añadimos las editoriales.
            _context.AddRange(editoriales);

            // Añadimos los libros.
            _context.AddRange(libros);

            // Guardamos los datos para que puedan consultarse desde el controlador.
            _context.SaveChanges();
        }

        // MEMBERDATA:
        // Proporciona los datos que utilizará la prueba parametrizada.
        // Cada elemento representa una ejecución con unos filtros diferentes
        // y una lista de libros que esperamos recibir.
        public static IEnumerable<object[]> TestCasesFor_GetLibrosParaComprar_OK()
        {
            // Creamos la lista de resultados esperados.
            // Estos DTO representan los libros que debería devolver el SELECT.
            var expectedLibros = new List<LibroParaComprarDTO>()
            {
                new LibroParaComprarDTO(
                    1,
                    "Cien años de soledad",
                    new DateTime(1967, 5, 30),
                    "Alfaguara",
                    19.95m,
                    "Novela"),

                new LibroParaComprarDTO(
                    2,
                    "El nombre del viento",
                    new DateTime(2007, 3, 27),
                    "Anagrama",
                    24.90m,
                    "Fantasía"),

                new LibroParaComprarDTO(
                    3,
                    "Dune",
                    new DateTime(1965, 8, 1),
                    "Anagrama",
                    21.50m,
                    "Ciencia Ficción")
            };

            // CASO 1: no aplicamos ningún filtro.
            // Esperamos los tres libros que tienen stock disponible.
            // El libro 4 queda excluido porque su stock es 0.
            // El orden esperado es el que establece el controlador.
            var librosSinFiltros = new List<LibroParaComprarDTO>()
            {
                expectedLibros[0],
                expectedLibros[2],
                expectedLibros[1]
            };

            // CASO 2: filtramos por el género Fantasía.
            // Esperamos únicamente el libro con Id = 2.
            var librosFiltradosPorGenero = new List<LibroParaComprarDTO>()
            {
                expectedLibros[1]
            };

            // CASO 3: establecemos un precio máximo de 22 euros.
            // Esperamos los libros 1 y 3 porque sus precios
            // son inferiores o iguales a 22 euros.
            var librosFiltradosPorPrecioMaximo = new List<LibroParaComprarDTO>()
            {
                expectedLibros[0],
                expectedLibros[2]
            };

            // CASO 4: combinamos el género Novela y el precio máximo de 20 euros.
            // Esperamos únicamente el libro 1.
            var librosFiltradosPorGeneroYPrecioMaximo = new List<LibroParaComprarDTO>
            {
                expectedLibros[0]
            };

            // Creamos la colección de casos de prueba.
            // Cada fila contiene:
            // 1. El género por el que queremos filtrar.
            // 2. El precio máximo permitido.
            // 3. La lista de libros que esperamos obtener.
            var allTest = new List<object[]>
            {
                // Caso 1: sin género y sin precio máximo.
                new object[] { null, null, librosSinFiltros },

                // Caso 2: solamente género Fantasía.
                new object[] { "Fantasía", null, librosFiltradosPorGenero },

                // Caso 3: solamente precio máximo de 22 euros.
                new object[] { null, 22.00m, librosFiltradosPorPrecioMaximo },

                // Caso 4: género Novela y precio máximo de 20 euros.
                new object[] { "Novela", 20.00m, librosFiltradosPorGeneroYPrecioMaximo }
            };

            // Devolvemos todos los casos a xUnit.
            return allTest;
        }

        // THEORY:
        // Ejecuta el mismo test varias veces utilizando los datos de MemberData.
        [Theory]

        // Indicamos que los datos proceden del método anterior.
        [MemberData(nameof(TestCasesFor_GetLibrosParaComprar_OK))]

        // Identificamos las características de la prueba.
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]

        // Recibimos los parámetros correspondientes a cada caso:
        // filtroGenero: género que queremos buscar.
        // filtroPrecioCompra: precio máximo permitido.
        // expectedLibros: libros que esperamos recibir.
        public async Task GetLibrosParaComprar_OK_test(
            string? filtroGenero,
            decimal? filtroPrecioCompra,
            IList<LibroParaComprarDTO> expectedLibros)
        {
            // ARRANGE:

            // Creamos un mock del logger para no necesitar
            // un sistema de registro real.
            var mock = new Mock<ILogger<LibroController>>();

            // Obtenemos el objeto simulado.
            ILogger<LibroController> logger = mock.Object;

            // Creamos el controlador con el contexto SQLite y el logger.
            var controller = new LibroController(_context, logger);

            // ACT:

            // Ejecutamos GetLibrosParaComprar pasando los filtros recibidos.
            var result = await controller.GetLibrosParaComprar(
                filtroGenero,
                filtroPrecioCompra);

            // ASSERT:

            // Comprobamos que el controlador devuelve OkObjectResult (HTTP 200).
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Comprobamos que el contenido de la respuesta
            // sea una lista de LibroParaComprarDTO.
            var libroDTOsActual =
                Assert.IsType<List<LibroParaComprarDTO>>(okResult.Value);

            // Comparamos la lista esperada con la lista obtenida.
            // Comprueba que ambas listas contienen los mismos DTO
            // en el mismo orden.
            // Para que la comparación de objetos funcione correctamente,
            // LibroParaComprarDTO debe implementar Equals.
            Assert.Equal(expectedLibros, libroDTOsActual);
        }
    }
}
