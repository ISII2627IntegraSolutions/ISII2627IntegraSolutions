using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.SubastarLibrosDTOs;

namespace AppForSEII.UT.SubastasController_test
{
    // Pruebas del post del CU3: CreateSubasta
    public class CreateSubasta_test : AppForSEII4SqliteUT
    {
        // Constructor: dos libros con stock, uno sin stock y un usuario registrado
        public CreateSubasta_test()
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

            var libro3 = new Libro("It", "Stephen King", new DateTime(1986, 9, 15), 18.00m, 0);
            libro3.Id = 3;
            libro3.TipoLibro = "Bolsillo";
            libro3.Editorial = alfaguara;
            libro3.Genero = genero;

            var usuario = new ApplicationUser("1", "Lucía", "García López", "lucia@uclm.es");
            usuario.PhoneNumber = "612345671";

            _context.Generos.Add(genero);
            _context.Editoriales.AddRange(alfaguara, salamandra);
            _context.Libros.AddRange(libro1, libro2, libro3);
            _context.ApplicationUsers.Add(usuario);
            _context.SaveChanges();
        }

        // Casos de error: (datos de la subasta, mensaje de error esperado)
        public static IEnumerable<object[]> TestCasesFor_CreateSubasta_Error()
        {
            var item1 = new SubastaItemDTO(1, "Cien años de soledad", "Gabriel García Márquez", "Alfaguara", 15, null);

            // FA4: ningún libro
            var sinLibros = new SubastaForCreateDTO("lucia@uclm.es", "Lucía", "García López", "612345671", "PayPal", new List<SubastaItemDTO>());
            sinLibros.TelefonoPayPal = "612345671";

            // Usuario no registrado
            var usuarioNoRegistrado = new SubastaForCreateDTO("noexiste@uclm.es", "Lucía", "García López", "612345671", "PayPal", new List<SubastaItemDTO> { item1 });
            usuarioNoRegistrado.TelefonoPayPal = "612345671";

            // FA5: método de pago que no existe
            var metodoIncorrecto = new SubastaForCreateDTO("lucia@uclm.es", "Lucía", "García López", "612345671", "Bizum", new List<SubastaItemDTO> { item1 });

            // FA5: Visa sin número de tarjeta
            var visaSinNumero = new SubastaForCreateDTO("lucia@uclm.es", "Lucía", "García López", "612345671", "Visa", new List<SubastaItemDTO> { item1 });
            visaSinNumero.FechaCaducidad = DateTime.Today.AddYears(1);

            // FA5: Visa caducada
            var visaCaducada = new SubastaForCreateDTO("lucia@uclm.es", "Lucía", "García López", "612345671", "Visa", new List<SubastaItemDTO> { item1 });
            visaCaducada.NumeroTarjeta = "4111111111111111";
            visaCaducada.FechaCaducidad = DateTime.Today.AddDays(-1);

            // FA5: GooglePay sin correo
            var googlePaySinEmail = new SubastaForCreateDTO("lucia@uclm.es", "Lucía", "García López", "612345671", "GooglePay", new List<SubastaItemDTO> { item1 });

            // FA5: PayPal sin teléfono
            var payPalSinTelefono = new SubastaForCreateDTO("lucia@uclm.es", "Lucía", "García López", "612345671", "PayPal", new List<SubastaItemDTO> { item1 });

            // El mismo libro dos veces
            var libroRepetido = new SubastaForCreateDTO("lucia@uclm.es", "Lucía", "García López", "612345671", "PayPal", new List<SubastaItemDTO> { item1, item1 });
            libroRepetido.TelefonoPayPal = "612345671";

            // Libro que no existe
            var libroNoExiste = new SubastaForCreateDTO("lucia@uclm.es", "Lucía", "García López", "612345671", "PayPal",
                new List<SubastaItemDTO> { new SubastaItemDTO(99, "No existe", "Nadie", "Ninguna", 15, null) });
            libroNoExiste.TelefonoPayPal = "612345671";

            // FA0: libro sin stock
            var libroSinStock = new SubastaForCreateDTO("lucia@uclm.es", "Lucía", "García López", "612345671", "PayPal",
                new List<SubastaItemDTO> { new SubastaItemDTO(3, "It", "Stephen King", "Alfaguara", 15, null) });
            libroSinStock.TelefonoPayPal = "612345671";

            // FA2: puja menor que 1
            var pujaIncorrecta = new SubastaForCreateDTO("lucia@uclm.es", "Lucía", "García López", "612345671", "PayPal",
                new List<SubastaItemDTO> { new SubastaItemDTO(1, "Cien años de soledad", "Gabriel García Márquez", "Alfaguara", 0, null) });
            pujaIncorrecta.TelefonoPayPal = "612345671";

            var allTests = new List<object[]>
            {
                new object[] { sinLibros, "Debes seleccionar al menos un libro." },
                new object[] { usuarioNoRegistrado, "El usuario no está registrado." },
                new object[] { metodoIncorrecto, "El método de pago debe ser Visa, GooglePay o PayPal." },
                new object[] { visaSinNumero, "Debes introducir el número de tarjeta." },
                new object[] { visaCaducada, "Debes introducir una fecha de caducidad válida." },
                new object[] { googlePaySinEmail, "Debes introducir el correo de GooglePay." },
                new object[] { payPalSinTelefono, "Debes introducir el teléfono de PayPal." },
                new object[] { libroRepetido, "Hay libros repetidos en la lista de pujas." },
                new object[] { libroNoExiste, "No existe el libro con identificador 99." },
                new object[] { libroSinStock, "El libro 'It' no está disponible para subastar." },
                new object[] { pujaIncorrecta, "La puja por 'Cien años de soledad' debe ser como mínimo 1." }
            };

            return allTests;
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_CreateSubasta_Error))]
        public async Task CreateSubasta_Error_test(SubastaForCreateDTO subastaForCreate, string errorExpected)
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            // Act
            var result = await controller.CreateSubasta(subastaForCreate);

            // Assert: devuelve 400 con el mensaje de error esperado
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            var errorActual = problemDetails.Errors.First().Value[0];
            Assert.Equal(errorExpected, errorActual);
        }

        [Fact]
        public async Task CreateSubasta_Success_test()
        {
            // Arrange
            var mock = new Mock<ILogger<SubastasController>>();
            ILogger<SubastasController> logger = mock.Object;
            var controller = new SubastasController(_context, logger);

            var items = new List<SubastaItemDTO>
            {
                new SubastaItemDTO(1, "Cien años de soledad", "Gabriel García Márquez", "Alfaguara", 15, "Libro en muy buen estado de conservación"),
                new SubastaItemDTO(2, "El nombre del viento", "Patrick Rothfuss", "Salamandra", 20, null)
            };
            var subastaForCreate = new SubastaForCreateDTO("lucia@uclm.es", "Lucía", "García López", "612345671", "PayPal", items);
            subastaForCreate.TelefonoPayPal = "612345671";

            var expectedSubasta = new SubastaDetailDTO(1, DateTime.Today, "lucia@uclm.es", "Lucía", "García López", "612345671", "PayPal", items);

            // Act
            var result = await controller.CreateSubasta(subastaForCreate);

            // Assert: devuelve 201 con el resumen de la subasta
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualSubasta = Assert.IsType<SubastaDetailDTO>(createdResult.Value);

            // La fecha la pone el controlador con DateTime.Now: se comprueba el día y se copia para comparar el resto
            Assert.Equal(DateTime.Today, actualSubasta.FechaSubasta.Date);
            expectedSubasta.FechaSubasta = actualSubasta.FechaSubasta;
            Assert.Equal(expectedSubasta, actualSubasta);
        }
    }
}