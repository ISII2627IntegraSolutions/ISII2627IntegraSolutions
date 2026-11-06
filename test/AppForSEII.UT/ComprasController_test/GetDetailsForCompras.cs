
using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;

// Namespace donde organizamos las pruebas del controlador de compras.
namespace AppForSEII.UT.ComprasController_test
{
    // Clase de pruebas unitarias para el método GetDetailsForCompras.
    // Hereda de AppForSEII4SqliteUT para utilizar SQLite como base de datos
    // durante la ejecución de las pruebas.
    public class GetDetailsForCompras_test : AppForSEII4SqliteUT
    {
        // CONSTRUCTOR:
        // Prepara los datos que utilizarán las pruebas.
        // Se ejecuta antes de cada prueba.
        public GetDetailsForCompras_test()
        {
            // ARRANGE: creamos el género al que pertenece el libro.
            // El tercer parámetro representa la colección de libros del género.
            var genero = new Genero(1, "Novela", new List<Libro>());

            // ARRANGE: creamos la editorial del libro.
            // Inicializamos también su colección de libros.
            var editorial =
                new Editorial(1, "Alfaguara", new List<Libro>());

            // ARRANGE: creamos el libro que estará asociado a la compra.
            var libro = new Libro(
                "Cien años de soledad",
                "Gabriel García Márquez",
                new DateTime(1967, 5, 30),
                19.95m,
                12)
            {
                // Asignamos el identificador del libro.
                Id = 1,

                // Relacionamos el libro con su género.
                Genero = genero,

                // Relacionamos el libro con su editorial.
                Editorial = editorial,

                // Indicamos su tipo de encuadernación.
                TipoLibro = "Tapa dura"
            };

            // ARRANGE: creamos el usuario que realizó la compra.
            var usuario = new ApplicationUser(
                "3",
                "Peter",
                "Jackson",
                "peter@uclm.es");

            // ARRANGE: creamos el método de pago utilizado.
            // En este caso se trata de una tarjeta Visa.
            var metodoPago = new Visa()
            {
                NumeroTarjeta = "4111111111111111",
                FechaCaducidad = new DateTime(2030, 12, 31)
            };

            // ARRANGE: creamos la compra.
            var compra = new Compra(
                new DateTime(2026, 10, 1),
                "DEMO01",
                metodoPago,
                new List<ComprarItem>())
            {
                // Identificador de la compra.
                Id = 1,

                // Usuario que ha realizado la compra.
                ApplicationUser = usuario,

                // Datos personales del comprador.
                Nombre = "Peter",
                Apellidos = "Jackson",
                Direccion = "Calle Mayor 1, Albacete",
                Telefono = "612345600"
            };

            // Añadimos un artículo a la compra.
            // Se compran dos unidades del libro creado anteriormente.
            compra.ComprarItems.Add(
                new ComprarItem(libro, 2, compra));

            // El constructor de Compra recibió inicialmente
            // una lista vacía de artículos.
            // Por eso recalculamos el precio total después de añadirlos.
            compra.PrecioTotal = compra.ComprarItems.Sum(
                item => item.PrecioCompra * item.Cantidad);

            // Añadimos el género al contexto de Entity Framework.
            _context.Add(genero);

            // Añadimos la editorial.
            _context.Add(editorial);

            // Añadimos el libro.
            _context.Add(libro);

            // Añadimos el usuario.
            _context.ApplicationUsers.Add(usuario);

            // Añadimos la compra y sus relaciones.
            _context.Add(compra);

            // Guardamos todos los datos en la base de datos SQLite.
            _context.SaveChanges();
        }

        // FACT:
        // Comprueba qué sucede cuando solicitamos una compra
        // cuyo identificador no existe en la base de datos.
        [Fact]

        // Indicamos las características de la prueba.
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForCompras_NotFound_test()
        {
            // ARRANGE:

            // Creamos un mock del logger.
            // Simula el servicio de registro que necesita el controlador.
            var mock = new Mock<ILogger<ComprasController>>();

            // Obtenemos el logger simulado.
            ILogger<ComprasController> logger = mock.Object;

            // Creamos el controlador utilizando el contexto SQLite
            // y el logger simulado.
            var controller = new ComprasController(_context, logger);

            // ACT:

            // Solicitamos el detalle de la compra con Id = 0.
            // Como no existe, esperamos que el controlador devuelva NotFound.
            var result = await controller.GetDetailsForCompras(0);

            // ASSERT:

            // Comprobamos que el resultado sea NotFoundResult (HTTP 404).
            Assert.IsType<NotFoundResult>(result);
        }

        // FACT:
        // Comprueba que el controlador devuelve correctamente
        // los detalles de una compra que sí existe.
        [Fact]

        // Indicamos las características de la prueba.
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForCompras_Found_test()
        {
            // ARRANGE:

            // Creamos un mock del logger.
            var mock = new Mock<ILogger<ComprasController>>();

            // Obtenemos el logger simulado.
            ILogger<ComprasController> logger = mock.Object;

            // Creamos el controlador.
            var controller = new ComprasController(_context, logger);

            // Construimos el resultado que esperamos recibir.
            // Utilizamos los mismos datos que hemos introducido
            // en la base de datos desde el constructor.
            var expectedCompra = new ComprasDetailDTO(
                1,
                new DateTime(2026, 10, 1),
                "peter@uclm.es",
                "Peter",
                "Jackson",
                "Calle Mayor 1, Albacete",
                "612345600",
                "Visa",
                "DEMO01",
                new List<CompraItemDTO>()
                {
                    // Esperamos que la compra incluya este artículo.
                    new CompraItemDTO(
                        1,
                        "Cien años de soledad",
                        "Novela",
                        "Alfaguara",
                        19.95m,
                        2)
                });

            // ACT:

            // Ejecutamos GetDetailsForCompras solicitando la compra con Id = 1.
            var result = await controller.GetDetailsForCompras(1);

            // ASSERT:

            // Comprobamos que el controlador devuelve OkObjectResult (HTTP 200).
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Comprobamos que el contenido de la respuesta
            // sea un objeto de tipo ComprasDetailDTO.
            var compraDTOActual =
                Assert.IsType<ComprasDetailDTO>(okResult.Value);

            // Comprobamos que todos los datos de la compra esperada
            // coincidan con los datos devueltos por el controlador.
            // Equals compara las propiedades implementadas en ComprasDetailDTO
            // y, mediante su clase base, las propiedades de CompraForCreateDTO.
            var eq = expectedCompra.Equals(compraDTOActual);

            // Assert.Equal utiliza Equals para comparar ambos DTO.
            // Si los datos no coinciden, la prueba falla.
            Assert.Equal(expectedCompra, compraDTOActual);
        }
    }
}
