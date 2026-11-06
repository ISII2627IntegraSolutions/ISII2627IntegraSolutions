using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.ReponerLibrosDTOs;

namespace AppForSEII.UT.ReposicionesController_test
{
    // Pruebas del details del CU2: GetDetailsForReposicion
    public class GetDetailsForReposicion_test : AppForSEII4SqliteUT
    {
        // Constructor: crea una reposición de dos libros pagada con Visa
        public GetDetailsForReposicion_test()
        {
            var novela = new Genero(1, "Novela", new List<Libro>());
            var terror = new Genero(2, "Terror", new List<Libro>());
            var alfaguara = new Editorial(1, "Alfaguara", new List<Libro>());

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

            // Los dos apellidos van juntos en Surname
            var usuario = new ApplicationUser("1", "Elena", "Navarro Martínez", "elena@uclm.es");
            usuario.PhoneNumber = "600000000";

            var visa = new Visa();
            visa.NumeroTarjeta = "4111111111111111";
            visa.FechaCaducidad = new DateTime(2028, 12, 31);

            // Reposición con dos libros
            var reposicion = new Reposicion(new DateTime(2026, 10, 20), "Reposición urgente para la tienda", visa, new List<ReposicionItem>());
            reposicion.ApplicationUser = usuario;
            reposicion.reposicionItems.Add(new ReposicionItem(5, libro1, reposicion, 18.00m));
            reposicion.reposicionItems.Add(new ReposicionItem(3, libro2, reposicion, 10.50m));
            reposicion.PrecioTotal = 5 * 18.00m + 3 * 10.50m;

            _context.Generos.AddRange(novela, terror);
            _context.Editoriales.Add(alfaguara);
            _context.Libros.AddRange(libro1, libro2);
            _context.ApplicationUsers.Add(usuario);
            _context.Reposiciones.Add(reposicion);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetDetailsForReposicion_NotFound_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ReposicionesController>>();
            ILogger<ReposicionesController> logger = mock.Object;
            var controller = new ReposicionesController(_context, logger);

            // Act: el id 0 no existe
            var result = await controller.GetDetailsForReposicion(0);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetDetailsForReposicion_Found_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ReposicionesController>>();
            ILogger<ReposicionesController> logger = mock.Object;
            var controller = new ReposicionesController(_context, logger);

            var expectedItems = new List<ReposicionItemDTO>
            {
                new ReposicionItemDTO(1, "It", "Stephen King", 18.00m, 5),
                new ReposicionItemDTO(2, "Diez negritos", "Agatha Christie", 10.50m, 3)
            };
            var expectedReposicion = new ReposicionDetailDTO(1, new DateTime(2026, 10, 20), "elena@uclm.es", "Elena", "Navarro", "Martínez",
                "600000000", "Visa", "Reposición urgente para la tienda", expectedItems);

            // Act
            var result = await controller.GetDetailsForReposicion(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualReposicion = Assert.IsType<ReposicionDetailDTO>(okResult.Value);
            Assert.Equal(expectedReposicion, actualReposicion);
        }
    }
}