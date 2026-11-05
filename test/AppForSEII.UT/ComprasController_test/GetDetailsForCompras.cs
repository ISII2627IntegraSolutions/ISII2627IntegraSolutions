using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs;

namespace AppForSEII.UT.ComprasController_test
{
    public class GetDetailsForCompras_test : AppForSEII4SqliteUT
    {
        public GetDetailsForCompras_test()
        {
            var genero = new Genero(1, "Novela", new List<Libro>());

            var editorial =
                new Editorial(1, "Alfaguara", new List<Libro>());

            var libro = new Libro(
                "Cien años de soledad",
                "Gabriel García Márquez",
                new DateTime(1967, 5, 30),
                19.95m,
                12)
            {
                Id = 1,
                Genero = genero,
                Editorial = editorial,
                TipoLibro = "Tapa dura"
            };

            var usuario = new ApplicationUser(
                "3",
                "Peter",
                "Jackson",
                "peter@uclm.es");

            var metodoPago = new Visa()
            {
                NumeroTarjeta = "4111111111111111",
                FechaCaducidad = new DateTime(2030, 12, 31)
            };

            var compra = new Compra(
                new DateTime(2026, 10, 1),
                "DEMO01",
                metodoPago,
                new List<ComprarItem>())
            {
                Id = 1,
                ApplicationUser = usuario,
                Nombre = "Peter",
                Apellidos = "Jackson",
                Direccion = "Calle Mayor 1, Albacete",
                Telefono = "612345600"
            };

            compra.ComprarItems.Add(
                new ComprarItem(libro, 2, compra));

            // El constructor recibió una lista vacía:
            // recalculamos el total después de añadir el artículo.
            compra.PrecioTotal = compra.ComprarItems.Sum(
                item => item.PrecioCompra * item.Cantidad);

            _context.Add(genero);
            _context.Add(editorial);
            _context.Add(libro);
            _context.ApplicationUsers.Add(usuario);
            _context.Add(compra);
            _context.SaveChanges();
        }

        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForCompras_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;

            var controller = new ComprasController(_context, logger);

            // Act
            var result = await controller.GetDetailsForCompras(0);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDetailsForCompras_Found_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ComprasController>>();
            ILogger<ComprasController> logger = mock.Object;

            var controller = new ComprasController(_context, logger);

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
                    new CompraItemDTO(
                        1,
                        "Cien años de soledad",
                        "Novela",
                        "Alfaguara",
                        19.95m,
                        2)
                });

            // Act
            var result = await controller.GetDetailsForCompras(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var compraDTOActual =
                Assert.IsType<ComprasDetailDTO>(okResult.Value);

            var eq = expectedCompra.Equals(compraDTOActual);
            Assert.Equal(expectedCompra, compraDTOActual);
        }
    }
}