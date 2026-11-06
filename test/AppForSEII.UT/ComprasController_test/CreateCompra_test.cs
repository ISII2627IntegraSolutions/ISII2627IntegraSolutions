
// Importamos el controlador que vamos a probar.
using AppForSEII.API.Controllers;

// Importamos los DTO que utilizamos para representar los datos de una compra.
using AppForSEII.API.DTOs;
using AppForSEII.API.DTOs.ComprarLibrosDTOs;

// Importamos los modelos de nuestra aplicación.
using AppForSEII.API.Models;

// Necesario para comprobar los resultados HTTP del controlador.
using Microsoft.AspNetCore.Mvc;

// Necesario para utilizar ILogger.
using Microsoft.Extensions.Logging;

// Necesario para crear objetos simulados (mocks).
using Moq;

// Namespace donde organizamos las pruebas del controlador de compras.
namespace AppForSEII.UT.ComprasController_test
{
    // Clase que contiene las pruebas unitarias del método CreateCompra.
    // Hereda de AppForSEII4SqliteUT para utilizar una base de datos SQLite
    // independiente para las pruebas.
    public class CreateCompra_test : AppForSEII4SqliteUT
    {
        // CONSTRUCTOR:
        // Se ejecuta antes de cada prueba y prepara los datos iniciales.
        public CreateCompra_test()
        {
            // ARRANGE: creamos los géneros que tendrán los libros.
            var generos = new List<Genero>()
            {
                // Primer género.
                new Genero(1, "Novela"),

                // Segundo género.
                new Genero(2, "Fantasía")
            };

            // ARRANGE: creamos las editoriales de los libros.
            var editoriales = new List<Editorial>()
            {
                // Primera editorial.
                new Editorial(1, "Alfaguara"),

                // Segunda editorial.
                new Editorial(2, "Anagrama")
            };

            // ARRANGE: creamos los libros necesarios para las pruebas.
            var libros = new List<Libro>()
            {
                // Libro 1: tiene 12 unidades disponibles.
                // Se utilizará para comprobar compras correctas,
                // cantidades incorrectas y falta de stock.
                new Libro(
                    1,
                    "Cien años de soledad",
                    "Gabriel García Márquez",
                    19.95m,
                    12,
                    new DateTime(1967, 5, 30),
                    generos[0],
                    editoriales[0])
                {
                    // Indicamos el tipo de encuadernación.
                    TipoLibro = "Tapa dura"
                },

                // Libro 2: segundo libro disponible.
                // Se utiliza para disponer de otro registro en la base de datos.
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
                }
            };

            // ARRANGE: creamos el usuario que podrá realizar compras.
            var usuarios = new List<ApplicationUser>()
            {
                new ApplicationUser(
                    "3",
                    "Peter",
                    "Jackson",
                    "peter@uclm.es")
            };

            // Asignamos el teléfono al usuario.
            usuarios[0].PhoneNumber = "612345600";

            // Indicamos que su correo electrónico está confirmado.
            usuarios[0].EmailConfirmed = true;

            // Añadimos los géneros a la base de datos de pruebas.
            _context.AddRange(generos);

            // Añadimos las editoriales.
            _context.AddRange(editoriales);

            // Añadimos los libros.
            _context.AddRange(libros);

            // Añadimos los usuarios.
            _context.ApplicationUsers.AddRange(usuarios);

            // Guardamos todos los datos para que estén disponibles
            // cuando se ejecuten los métodos de prueba.
            _context.SaveChanges();
        }

        // MEMBERDATA:
        // Este método proporciona los distintos conjuntos de datos
        // que utilizará la prueba parametrizada CreateCompra_Error_test.
        public static IEnumerable<object[]> TestCasesFor_CreateCompra_Error()
        {
            // CASO 1: el cliente no selecciona ningún libro.
            // Esperamos que el controlador rechace una lista vacía.
            var compraSinLibros = new CompraForCreateDTO(
                "peter@uclm.es",
                "Peter",
                "Jackson",
                "Calle Falsa 123",
                "612345600",
                "GooglePay",
                "DEMO01",
                new List<CompraItemDTO>());

            // CASO 2: el cliente selecciona un método de pago no permitido.
            // Bitcoin no está entre los métodos admitidos por el controlador.
            var metodoIncorrecto = new CompraForCreateDTO(
                "peter@uclm.es",
                "Peter",
                "Jackson",
                "Calle Falsa 123",
                "612345600",
                "Bitcoin",
                "DEMO01",
                new List<CompraItemDTO>()
                {
                    // Incluimos un libro válido para que el error
                    // que queremos comprobar sea el método de pago.
                    new CompraItemDTO(
                        1,
                        "Cien años de soledad",
                        "Novela",
                        "Alfaguara",
                        19.95m,
                        2)
                });

            // CASO 3: el usuario que intenta comprar no está registrado.
            // Utilizamos un correo que no existe en nuestra base de datos.
            var usuarioInexistente = new CompraForCreateDTO(
                "john@uclm.es",
                "John",
                "Doe",
                "Calle Falsa 123",
                "612345600",
                "GooglePay",
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

            // CASO 4: el cliente solicita más unidades de las disponibles.
            // El libro tiene 12 unidades, pero se solicitan 13.
            var stockInsuficiente = new CompraForCreateDTO(
                "peter@uclm.es",
                "Peter",
                "Jackson",
                "Calle Falsa 123",
                "612345600",
                "GooglePay",
                "DEMO01",
                new List<CompraItemDTO>()
                {
                    new CompraItemDTO(
                        1,
                        "Cien años de soledad",
                        "Novela",
                        "Alfaguara",
                        19.95m,
                        13)
                })
            {
                // GooglePay necesita un correo electrónico válido.
                EmailGooglePay = "peter@uclm.es"
            };

            // CASO 5: la cantidad seleccionada no cumple las condiciones.
            // El controlador exige una cantidad mayor que 1.
            var cantidadIncorrecta = new CompraForCreateDTO(
                "peter@uclm.es",
                "Peter",
                "Jackson",
                "Calle Falsa 123",
                "612345600",
                "GooglePay",
                "DEMO01",
                new List<CompraItemDTO>()
                {
                    new CompraItemDTO(
                        1,
                        "Cien años de soledad",
                        "Novela",
                        "Alfaguara",
                        19.95m,
                        1)
                })
            {
                // Proporcionamos el correo necesario para GooglePay.
                EmailGooglePay = "peter@uclm.es"
            };

            // CASO 6: el cliente selecciona un libro que no existe.
            // El identificador 99 no está registrado en la base de datos.
            var libroInexistente = new CompraForCreateDTO(
                "peter@uclm.es",
                "Peter",
                "Jackson",
                "Calle Falsa 123",
                "612345600",
                "GooglePay",
                "DEMO01",
                new List<CompraItemDTO>()
                {
                    new CompraItemDTO(
                        99,
                        "Libro inexistente",
                        "Novela",
                        "Alfaguara",
                        19.95m,
                        2)
                })
            {
                // Proporcionamos el correo necesario para GooglePay.
                EmailGooglePay = "peter@uclm.es"
            };

            // Creamos la lista con todos los casos de prueba.
            // Cada elemento contiene:
            // 1. El DTO que recibirá el controlador.
            // 2. El nombre del campo que debe producir el error.
            // 3. El mensaje de error que esperamos recibir.
            var allTests = new List<object[]>()
            {
                // Caso 1: lista de libros vacía.
                new object[]
                {
                    compraSinLibros,
                    "CompraItems",
                    "Debes seleccionar al menos un libro."
                },

                // Caso 2: método de pago incorrecto.
                new object[]
                {
                    metodoIncorrecto,
                    "MetodoDePago",
                    "El método de pago debe ser Visa, GooglePay o PayPal."
                },

                // Caso 3: usuario inexistente.
                new object[]
                {
                    usuarioInexistente,
                    "CustomerUserName",
                    "El usuario no está registrado."
                },

                // Caso 4: stock insuficiente.
                new object[]
                {
                    stockInsuficiente,
                    "CompraItems",
                    "No hay stock suficiente de 'Cien años de soledad'. Unidades disponibles: 12."
                },

                // Caso 5: cantidad incorrecta.
                new object[]
                {
                    cantidadIncorrecta,
                    "CompraItems",
                    "La cantidad de 'Cien años de soledad' debe ser mayor que 1."
                },

                // Caso 6: libro inexistente.
                new object[]
                {
                    libroInexistente,
                    "CompraItems",
                    "No existe el libro con identificador 99."
                }
            };

            // Devolvemos todos los casos para que xUnit los ejecute.
            return allTests;
        }

        // THEORY:
        // Ejecuta el mismo método de prueba una vez por cada elemento
        // proporcionado por TestCasesFor_CreateCompra_Error.
        [Theory]

        // Indicamos a xUnit de dónde obtiene los datos de cada ejecución.
        [MemberData(nameof(TestCasesFor_CreateCompra_Error))]

        // Identificamos el tipo de prueba y la configuración de base de datos.
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]

        // Recibimos los tres parámetros correspondientes a cada escenario.
        public async Task CreateCompra_Error_test(
            CompraForCreateDTO compraDTO,
            string campoEsperado,
            string errorEsperado)
        {
            // ARRANGE:

            // Creamos un mock del logger.
            // Un mock es un objeto simulado que sustituye al logger real.
            var mock = new Mock<ILogger<ComprasController>>();

            // Obtenemos el objeto simulado para pasárselo al controlador.
            ILogger<ComprasController> logger = mock.Object;

            // Creamos el controlador con el contexto SQLite y el logger.
            var controller = new ComprasController(_context, logger);

            // ACT:

            // Ejecutamos el método CreateCompra utilizando
            // el DTO correspondiente al escenario de prueba.
            var result = await controller.CreateCompra(compraDTO);

            // ASSERT:

            // Comprobamos que la respuesta HTTP sea BadRequest (400).
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            // Comprobamos que el contenido de la respuesta
            // sea un objeto ValidationProblemDetails.
            var validationProblem =
                Assert.IsType<ValidationProblemDetails>(
                    badRequestResult.Value);

            // Comprobamos que el error contiene la clave esperada.
            // Si falla, mostramos los campos que realmente devuelve.
            Assert.True(
                validationProblem.Errors.ContainsKey(campoEsperado),
                $"Caso fallido. Campo esperado: {campoEsperado}. " +
                $"Campos recibidos: {string.Join(", ", validationProblem.Errors.Keys)}");

            // Comprobamos que el mensaje de error esperado
            // aparece entre los mensajes asociados a ese campo.
            Assert.Contains(
                errorEsperado,
                validationProblem.Errors[campoEsperado]);
        }

        // FACT:
        // Esta prueba comprueba el escenario de una compra correcta.
        // A diferencia de Theory, Fact ejecuta un único escenario.
        [Fact]

        // Indicamos las características de esta prueba.
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]

        // Método de prueba que comprueba la creación satisfactoria.
        public async Task CreateCompra_Success_test()
        {
            // ARRANGE:

            // Creamos el mock del logger.
            var mock = new Mock<ILogger<ComprasController>>();

            // Obtenemos el logger simulado.
            ILogger<ComprasController> logger = mock.Object;

            // Creamos el controlador que queremos probar.
            var controller = new ComprasController(_context, logger);

            // Creamos el DTO con los datos de una compra válida.
            var compraDTO = new CompraForCreateDTO(
                "peter@uclm.es",
                "Peter",
                "Jackson",
                "Calle Falsa 123",
                "612345600",
                "GooglePay",
                "DEMO01",
                new List<CompraItemDTO>()
                {
                    // El cliente compra dos unidades del libro con Id 1.
                    new CompraItemDTO(
                        1,
                        "Cien años de soledad",
                        "Novela",
                        "Alfaguara",
                        19.95m,
                        2)
                })
            {
                // Indicamos el correo asociado al pago con GooglePay.
                EmailGooglePay = "peter@uclm.es"
            };

            // Guardamos el stock inicial del libro antes de comprar.
            var stockInicial = _context.Libros.Find(1)!.Stock;

            // Guardamos el instante inmediatamente anterior a la operación.
            // Servirá para comprobar posteriormente la fecha de compra.
            var fechaAntes = DateTime.Now;

            // ACT:

            // Ejecutamos el método CreateCompra con los datos válidos.
            var result = await controller.CreateCompra(compraDTO);

            // Guardamos el instante posterior a la ejecución.
            var fechaDespues = DateTime.Now;

            // ASSERT:

            // Comprobamos que la respuesta sea CreatedAtActionResult.
            // Este resultado representa una respuesta HTTP 201 Created.
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            // Comprobamos que el contenido devuelto sea un ComprasDetailDTO.
            var compraDetailDTOReal =
                Assert.IsType<ComprasDetailDTO>(createdResult.Value);

            // Comprobamos que la fecha de compra esté comprendida
            // entre el instante anterior y el posterior a la operación.
            Assert.InRange(
                compraDetailDTOReal.FechaCompra,
                fechaAntes,
                fechaDespues);

            // Comprobamos que todos los datos comunes del DTO de entrada
            // coincidan con los datos devueltos por el controlador.
            // Para ello se utiliza el Equals implementado en CompraForCreateDTO.
            Assert.Equal(compraDTO, compraDetailDTOReal);

            // Comprobamos que el artículo devuelto tenga todos
            // los datos esperados: Id, título, género, editorial,
            // precio y cantidad.
            // Se utiliza Equals implementado en CompraItemDTO.
            Assert.Equal(
                new CompraItemDTO(
                    1,
                    "Cien años de soledad",
                    "Novela",
                    "Alfaguara",
                    19.95m,
                    2),
                compraDetailDTOReal.CompraItems[0]);

            // Comprobamos que el libro sigue existiendo en la base de datos
            // y recuperamos su información actualizada.
            var libroActualizado = await _context.Libros.FindAsync(1);

            // Comprobamos que el stock se ha reducido en dos unidades.
            // Stock inicial: 12.
            // Unidades compradas: 2.
            // Stock final esperado: 10.
            Assert.Equal(
                stockInicial - 2,
                libroActualizado!.Stock);

            // Comprobamos que la compra se ha guardado en la base de datos.
            // Esperamos encontrar exactamente una compra.
            Assert.Equal(1, _context.Compras.Count());

            // Comprobamos que también se ha guardado el artículo de compra.
            // Esperamos encontrar exactamente un ComprarItem.
            Assert.Equal(1, _context.Set<ComprarItem>().Count());
        }
    }
}
