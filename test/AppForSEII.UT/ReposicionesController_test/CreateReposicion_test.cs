// PRUEBA UNITARIA del POST del CU2: CreateReposicion (ReposicionesController)
// Error_test: un Theory con un caso por cada error del controlador (todos devuelven 400 con su mensaje)
// Success_test: con datos correctos crea la reposición, devuelve 201 y sube el stock del libro
using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.ReponerLibrosDTOs;

namespace AppForSEII.UT.ReposicionesController_test
{
    // Pruebas del post del CU2: CreateReposicion
    public class CreateReposicion_test : AppForSEII4SqliteUT
    {
        // Constructor: dos libros sin stock, uno con stock y un usuario registrado
        public CreateReposicion_test()
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
             // Libro con stock: para probar que no se puede reponer (FA0)
            var libro3 = new Libro("Cien años de soledad", "Gabriel García Márquez", new DateTime(1967, 5, 30), 19.95m, 12);
            libro3.Id = 3;
            libro3.TipoLibro = "Tapa dura";
            libro3.Editorial = alfaguara;
            libro3.Genero = novela;
             // Único usuario registrado: elena@uclm.es
            var usuario = new ApplicationUser("1", "Elena", "Navarro Martínez", "elena@uclm.es");
            usuario.PhoneNumber = "600000000";

            _context.Generos.AddRange(novela, terror);
            _context.Editoriales.Add(alfaguara);
            _context.Libros.AddRange(libro1, libro2, libro3);
            _context.ApplicationUsers.Add(usuario);
            _context.SaveChanges();
        }

        // Casos de error: (datos de la reposición, mensaje de error esperado)
          // Cada caso provoca un solo error, para poder comprobar su mensaje exacto
        // MODIFICAR: si en el examen añades una validación al POST, añade aquí un caso con su mensaje
        public static IEnumerable<object[]> TestCasesFor_CreateReposicion_Error()
        {
            // Libro correcto que se reutiliza en varios casos
            var item1 = new ReposicionItemDTO(1, "It", "Stephen King", 18.00m, 5);

            // FA2: ningún libro
            var sinLibros = new ReposicionForCreateDTO("elena@uclm.es", "Elena", "Navarro", "Martínez", "600000000", "PayPal", null, new List<ReposicionItemDTO>());
            sinLibros.TelefonoPayPal = "600000000";

            // Usuario no registrado
            var usuarioNoRegistrado = new ReposicionForCreateDTO("noexiste@uclm.es", "Elena", "Navarro", "Martínez", "600000000", "PayPal", null, new List<ReposicionItemDTO> { item1 });
            usuarioNoRegistrado.TelefonoPayPal = "600000000";

            // FA4: método de pago que no existe
            var metodoIncorrecto = new ReposicionForCreateDTO("elena@uclm.es", "Elena", "Navarro", "Martínez", "600000000", "Bizum", null, new List<ReposicionItemDTO> { item1 });

            // FA4: Visa sin número de tarjeta
            var visaSinNumero = new ReposicionForCreateDTO("elena@uclm.es", "Elena", "Navarro", "Martínez", "600000000", "Visa", null, new List<ReposicionItemDTO> { item1 });
            visaSinNumero.FechaCaducidad = DateTime.Today.AddYears(1);

            // FA4: Visa caducada
            var visaCaducada = new ReposicionForCreateDTO("elena@uclm.es", "Elena", "Navarro", "Martínez", "600000000", "Visa", null, new List<ReposicionItemDTO> { item1 });
            visaCaducada.NumeroTarjeta = "4111111111111111";
            visaCaducada.FechaCaducidad = DateTime.Today.AddDays(-1);

            // FA4: GooglePay sin correo
            var googlePaySinEmail = new ReposicionForCreateDTO("elena@uclm.es", "Elena", "Navarro", "Martínez", "600000000", "GooglePay", null, new List<ReposicionItemDTO> { item1 });

            // FA4: PayPal sin teléfono
            var payPalSinTelefono = new ReposicionForCreateDTO("elena@uclm.es", "Elena", "Navarro", "Martínez", "600000000", "PayPal", null, new List<ReposicionItemDTO> { item1 });

            // El mismo libro dos veces
            var libroRepetido = new ReposicionForCreateDTO("elena@uclm.es", "Elena", "Navarro", "Martínez", "600000000", "PayPal", null, new List<ReposicionItemDTO> { item1, item1 });
            libroRepetido.TelefonoPayPal = "600000000";

            // Libro que no existe
            var libroNoExiste = new ReposicionForCreateDTO("elena@uclm.es", "Elena", "Navarro", "Martínez", "600000000", "PayPal", null,
                new List<ReposicionItemDTO> { new ReposicionItemDTO(99, "No existe", "Nadie", 10.00m, 5) });
            libroNoExiste.TelefonoPayPal = "600000000";

            // FA0: libro que tiene stock
            var libroConStock = new ReposicionForCreateDTO("elena@uclm.es", "Elena", "Navarro", "Martínez", "600000000", "PayPal", null,
                new List<ReposicionItemDTO> { new ReposicionItemDTO(3, "Cien años de soledad", "Gabriel García Márquez", 19.95m, 5) });
            libroConStock.TelefonoPayPal = "600000000";

            // Paso 5: cantidad menor que 2
            var cantidadIncorrecta = new ReposicionForCreateDTO("elena@uclm.es", "Elena", "Navarro", "Martínez", "600000000", "PayPal", null,
                new List<ReposicionItemDTO> { new ReposicionItemDTO(1, "It", "Stephen King", 18.00m, 1) });
            cantidadIncorrecta.TelefonoPayPal = "600000000";
            // Cada fila: los datos que se mandan y el mensaje que tiene que devolver el controlador
            var allTests = new List<object[]>
            {
                new object[] { sinLibros, "Debes seleccionar al menos un libro." },
                new object[] { usuarioNoRegistrado, "El usuario no está registrado." },
                new object[] { metodoIncorrecto, "El método de pago debe ser Visa, GooglePay o PayPal." },
                new object[] { visaSinNumero, "Debes introducir el número de tarjeta." },
                new object[] { visaCaducada, "Debes introducir una fecha de caducidad válida." },
                new object[] { googlePaySinEmail, "Debes introducir el correo de GooglePay." },
                new object[] { payPalSinTelefono, "Debes introducir el teléfono de PayPal." },
                new object[] { libroRepetido, "Hay libros repetidos. Indica la cantidad en un único libro." },
                new object[] { libroNoExiste, "No existe el libro con identificador 99." },
                new object[] { libroConStock, "El libro 'Cien años de soledad' tiene stock y no necesita reposición." },
                new object[] { cantidadIncorrecta, "La cantidad de 'It' debe ser mayor que 1." }
            };

            return allTests;
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_CreateReposicion_Error))]
        public async Task CreateReposicion_Error_test(ReposicionForCreateDTO reposicionForCreate, string errorExpected)
        {
            // Arrange
            var mock = new Mock<ILogger<ReposicionesController>>();
            ILogger<ReposicionesController> logger = mock.Object;
            var controller = new ReposicionesController(_context, logger);

            // Act
            var result = await controller.CreateReposicion(reposicionForCreate);

            // Assert: devuelve 400 con el mensaje de error esperado
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
             // Errors es un diccionario (campo -> mensajes): se coge el primer mensaje del primer campo
            var errorActual = problemDetails.Errors.First().Value[0];
            Assert.Equal(errorExpected, errorActual);
        }

        [Fact]
        public async Task CreateReposicion_Success_test()
        {
            // Arrange
            var mock = new Mock<ILogger<ReposicionesController>>();
            ILogger<ReposicionesController> logger = mock.Object;
            var controller = new ReposicionesController(_context, logger);

            // El precio de reposición es el precio de compra del libro
            var items = new List<ReposicionItemDTO>
            {
                new ReposicionItemDTO(1, "It", "Stephen King", 18.00m, 5),
                new ReposicionItemDTO(2, "Diez negritos", "Agatha Christie", 10.50m, 3)
            };
            var reposicionForCreate = new ReposicionForCreateDTO("elena@uclm.es", "Elena", "Navarro", "Martínez", "600000000", "Visa",
                "Reposición urgente para la tienda", items);
            reposicionForCreate.NumeroTarjeta = "4111111111111111";
            reposicionForCreate.FechaCaducidad = DateTime.Today.AddYears(1);
             // Resumen esperado: Id 1 porque es la primera reposición de la base de datos en memoria
            var expectedReposicion = new ReposicionDetailDTO(1, DateTime.Today, "elena@uclm.es", "Elena", "Navarro", "Martínez",
                "600000000", "Visa", "Reposición urgente para la tienda", items);

            // Act
            var result = await controller.CreateReposicion(reposicionForCreate);

            // Assert: devuelve 201 con el resumen de la reposición
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualReposicion = Assert.IsType<ReposicionDetailDTO>(createdResult.Value);

            // La fecha la pone el controlador con DateTime.Now: se comprueba el día y se copia para comparar el resto
            Assert.Equal(DateTime.Today, actualReposicion.FechaReposicion.Date);
            expectedReposicion.FechaReposicion = actualReposicion.FechaReposicion;
            Assert.Equal(expectedReposicion, actualReposicion);

            // Al reponer, el stock de It pasa de 0 a 5
            var libroIt = _context.Libros.First(l => l.Id == 1);
            Assert.Equal(5, libroIt.Stock);
        }
    }
}