// PRUEBA UNITARIA del DETAILS del CU3: GetDetailsForSubasta (SubastasController)
// Dos pruebas: NotFound (id que no existe -> 404) y Found (id que existe -> 200 con el resumen correcto)
using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.SubastarLibrosDTOs;

namespace AppForSEII.UT.SubastasController_test
{
    // Pruebas del details del CU3: GetDetailsForSubasta
    public class GetDetailsForSubasta_test : AppForSEII4SqliteUT
    {
        // Constructor: crea una subasta con dos libros pagada con PayPal
        public GetDetailsForSubasta_test()
        {
            var genero = new Genero(1, "Novela", new List<Libro>());
            var alfaguara = new Editorial(1, "Alfaguara", new List<Libro>());
            var salamandra = new Editorial(2, "Salamandra", new List<Libro>());

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

            // Cliente de la subasta (el teléfono no va en el constructor, se pone aparte)
            var usuario = new ApplicationUser("1", "Lucía", "García López", "lucia@uclm.es");
            usuario.PhoneNumber = "612345671";

            // Método de pago real (PayPal) para que en el details salga "PayPal" y no "Desconocido"
            var paypal = new PayPal();
            paypal.NumeroTelefono = "612345671";

            // Subasta con dos pujas
            var subasta = new Subasta(new DateTime(2026, 11, 1), usuario, paypal, new List<SubastaItem>());
            subasta.SubastaItems.Add(new SubastaItem(libro1, subasta, 10, "Ejemplar en perfecto estado, sin subrayados"));
            subasta.SubastaItems.Add(new SubastaItem(libro2, subasta, 15, null));
            subasta.PrecioSubasta = 25;

            // Se guarda todo en la base de datos en memoria (la subasta tendrá Id = 1)
            _context.Generos.Add(genero);
            _context.Editoriales.AddRange(alfaguara, salamandra);
            _context.Libros.AddRange(libro1, libro2);
            _context.ApplicationUsers.Add(usuario);
            _context.Subastas.Add(subasta);
            _context.SaveChanges();
        }

        // [Fact] = prueba única, sin casos distintos
        [Fact]
        public async Task GetDetailsForSubasta_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            // Act: el id 0 no existe
            var result = await controller.GetDetailsForSubasta(0);

            // Assert: tiene que devolver 404 (NotFoundResult)
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetDetailsForSubasta_Found_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            // Resumen que debería devolver el controlador para la subasta 1
            // MODIFICAR: si en el examen añades un dato al SubastaDetailDTO, hay que añadirlo también aquí
            var expectedItems = new List<SubastaItemDTO>
            {
                new SubastaItemDTO(1, "Cien años de soledad", "Gabriel García Márquez", "Alfaguara", 10, "Ejemplar en perfecto estado, sin subrayados"),
                new SubastaItemDTO(2, "El nombre del viento", "Patrick Rothfuss", "Salamandra", 15, null)
            };
            var expectedSubasta = new SubastaDetailDTO(1, new DateTime(2026, 11, 1), "lucia@uclm.es", "Lucía", "García López", "612345671", "PayPal", expectedItems);

            // Act
            var result = await controller.GetDetailsForSubasta(1);

            // Assert: 200 (OkObjectResult) con un SubastaDetailDTO igual al esperado (usa su Equals)
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualSubasta = Assert.IsType<SubastaDetailDTO>(okResult.Value);
            Assert.Equal(expectedSubasta, actualSubasta);
        }
    }
}