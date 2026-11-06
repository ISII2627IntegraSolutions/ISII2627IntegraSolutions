
using System.Linq;
using System.Net;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs;
using AppForSEII.API.DTOs.ComprarLibrosDTOs;
using AppForSEII.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppForSEII.API.Controllers
{
    // Indicamos que las rutas de este controlador comienzan por api/Compras.
    [Route("api/[controller]")]

    // Activa funcionalidades automáticas de los controladores API.
    [ApiController]
    public class ComprasController : ControllerBase
    {
        // Contexto que permite acceder a la base de datos mediante EF Core.
        private readonly ApplicationDbContext _context;

        // Logger que permite registrar errores e información del controlador.
        private readonly ILogger<ComprasController> _logger;

        // CONSTRUCTOR:
        // Recibe las dependencias necesarias mediante inyección de dependencias.
        public ComprasController(
            ApplicationDbContext context,
            ILogger<ComprasController> logger)
        {
            // Guardamos el contexto recibido.
            _context = context;

            // Guardamos el logger recibido.
            _logger = logger;
        }

        // ============================================================
        // MÉTODO GET: GetDetailsForCompras
        // ============================================================

        // Indicamos que este método responde a peticiones HTTP GET.
        [HttpGet]

        // La ruta utiliza el nombre del método.
        // Ruta: api/Compras/GetDetailsForCompras
        [Route("[action]")]

        // Respuesta esperada cuando la compra existe: HTTP 200.
        [ProducesResponseType(
            typeof(ComprasDetailDTO),
            (int)HttpStatusCode.OK)]

        // Respuesta cuando la compra no existe: HTTP 404.
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForCompras(int id)
        {
            // Buscamos la compra cuyo identificador coincide con el recibido.
            var compra = await _context.Compras

                // Filtramos por el identificador de la compra.
                .Where(c => c.Id == id)

                // Incluimos el usuario asociado a la compra.
                .Include(c => c.ApplicationUser)

                // Incluimos el método de pago.
                .Include(c => c.MetodoDePago)

                // Incluimos los artículos de la compra.
                .Include(c => c.ComprarItems)

                    // Incluimos el libro asociado a cada artículo.
                    .ThenInclude(item => item.Libro)

                        // Incluimos el género de cada libro.
                        .ThenInclude(libro => libro.Genero)

                // Volvemos a incluir los artículos para cargar otra relación.
                .Include(c => c.ComprarItems)

                    // Incluimos el libro asociado a cada artículo.
                    .ThenInclude(item => item.Libro)

                        // Incluimos la editorial de cada libro.
                        .ThenInclude(libro => libro.Editorial)

                // Transformamos la entidad Compra en un ComprasDetailDTO.
                .Select(c => new ComprasDetailDTO(
                    c.Id,
                    c.FechaCompra,
                    c.ApplicationUser.UserName!,
                    c.Nombre,
                    c.Apellidos,
                    c.Direccion,
                    c.Telefono,

                    // Comprobamos qué tipo de método de pago tiene la compra.
                    // Según el tipo, devolvemos el nombre correspondiente.
                    c.MetodoDePago is Visa ? "Visa" :
                        c.MetodoDePago is GooglePay ? "GooglePay" :
                        c.MetodoDePago is PayPal ? "PayPal" :
                        "Desconocido",

                    // Recuperamos el código de descuento.
                    c.CodigoDescuento,

                    // Convertimos cada artículo de compra en un CompraItemDTO.
                    c.ComprarItems
                        .Select(item => new CompraItemDTO(
                            item.LibroId,
                            item.Libro.Titulo,
                            item.Libro.Genero.Nombre,
                            item.Libro.Editorial.Nombre,
                            item.PrecioCompra,
                            item.Cantidad))
                        .ToList()))

                // Ejecutamos la consulta y obtenemos el primer resultado.
                // Si no existe ninguna compra, devuelve null.
                .FirstOrDefaultAsync();

            // Comprobamos si la compra no existe.
            if (compra == null)
            {
                // Registramos el error en el logger.
                _logger.LogError(
                    $"Error: no existe una compra con id {id}.");

                // Devolvemos HTTP 404.
                return NotFound();
            }

            // Si existe, devolvemos el detalle con HTTP 200.
            return Ok(compra);
        }

        // ============================================================
        // MÉTODO POST: CreateCompra
        // ============================================================

        // Indicamos que este método responde a peticiones HTTP POST.
        [HttpPost]

        // La ruta utiliza el nombre del método.
        // Ruta: api/Compras/CreateCompra
        [Route("[action]")]

        // Respuesta cuando la compra se crea correctamente: HTTP 201.
        [ProducesResponseType(
            typeof(ComprasDetailDTO),
            (int)HttpStatusCode.Created)]

        // Respuesta cuando los datos no son válidos: HTTP 400.
        [ProducesResponseType(
            typeof(ValidationProblemDetails),
            (int)HttpStatusCode.BadRequest)]

        // Respuesta cuando se produce un conflicto al guardar: HTTP 409.
        [ProducesResponseType(
            typeof(string),
            (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateCompra(
            CompraForCreateDTO compraForCreate)
        {
            // ------------------------------------------------------------
            // 1. COMPROBAR QUE HAY LIBROS SELECCIONADOS
            // ------------------------------------------------------------

            // Comprobamos que la lista de artículos no sea nula
            // y que contenga al menos un libro.
            if (compraForCreate.CompraItems == null ||
                compraForCreate.CompraItems.Count == 0)
            {
                // Añadimos un error al ModelState.
                // ModelState almacena los errores de validación.
                ModelState.AddModelError(
                    "CompraItems",
                    "Debes seleccionar al menos un libro.");
            }

            // ------------------------------------------------------------
            // 2. COMPROBAR QUE EL USUARIO ESTÁ REGISTRADO
            // ------------------------------------------------------------

            // Buscamos al usuario por su nombre de usuario (correo electrónico).
            // FirstOrDefaultAsync devuelve el primer usuario encontrado
            // o null si no existe.
            var usuario = await _context.ApplicationUsers
                .FirstOrDefaultAsync(u =>
                    u.UserName == compraForCreate.CustomerUserName);

            // Comprobamos si el usuario no existe.
            if (usuario == null)
            {
                // Añadimos el correspondiente error al ModelState.
                ModelState.AddModelError(
                    "CustomerUserName",
                    "El usuario no está registrado.");
            }

            // ------------------------------------------------------------
            // 3. VALIDAR EL MÉTODO DE PAGO
            // ------------------------------------------------------------

            // Declaramos una variable que almacenará el método de pago.
            // Inicialmente es null porque todavía no hemos determinado
            // qué método ha seleccionado el cliente.
            MetodosdePago? metodoPago = null;

            // Comprobamos el método de pago indicado en el DTO.
            switch (compraForCreate.MetodoDePago)
            {
                // CASO VISA:
                case "Visa":

                    // Comprobamos que se haya introducido el número de tarjeta.
                    if (string.IsNullOrWhiteSpace(compraForCreate.NumeroTarjeta))
                    {
                        ModelState.AddModelError(
                            "NumeroTarjeta",
                            "Debes introducir el número de tarjeta.");
                    }

                    // Comprobamos que exista una fecha de caducidad
                    // y que no sea anterior a la fecha actual.
                    if (!compraForCreate.FechaCaducidad.HasValue ||
                        compraForCreate.FechaCaducidad.Value.Date < DateTime.Today)
                    {
                        ModelState.AddModelError(
                            "FechaCaducidad",
                            "Debes introducir una fecha de caducidad válida.");
                    }

                    // Creamos el objeto Visa con los datos recibidos.
                    metodoPago = new Visa
                    {
                        NumeroTarjeta = compraForCreate.NumeroTarjeta ?? "",
                        FechaCaducidad =
                            compraForCreate.FechaCaducidad ?? DateTime.Today
                    };

                    // Terminamos este caso del switch.
                    break;

                // CASO GOOGLE PAY:
                case "GooglePay":

                    // Comprobamos que se haya introducido el correo electrónico.
                    if (string.IsNullOrWhiteSpace(compraForCreate.EmailGooglePay))
                    {
                        ModelState.AddModelError(
                            "EmailGooglePay",
                            "Debes introducir el correo de GooglePay.");
                    }

                    // Creamos el objeto GooglePay.
                    metodoPago = new GooglePay
                    {
                        Email = compraForCreate.EmailGooglePay ?? ""
                    };

                    break;

                // CASO PAYPAL:
                case "PayPal":

                    // Comprobamos que se haya introducido el teléfono.
                    if (string.IsNullOrWhiteSpace(compraForCreate.TelefonoPayPal))
                    {
                        ModelState.AddModelError(
                            "TelefonoPayPal",
                            "Debes introducir el teléfono de PayPal.");
                    }

                    // Creamos el objeto PayPal.
                    metodoPago = new PayPal
                    {
                        NumeroTelefono = compraForCreate.TelefonoPayPal ?? ""
                    };

                    break;

                // SI EL MÉTODO NO ES VÁLIDO:
                default:

                    // Añadimos un error indicando los métodos admitidos.
                    ModelState.AddModelError(
                        "MetodoDePago",
                        "El método de pago debe ser Visa, GooglePay o PayPal.");

                    break;
            }

            // ------------------------------------------------------------
            // 4. COMPROBAR LOS ERRORES DE VALIDACIÓN
            // ------------------------------------------------------------

            // Si hay algún error acumulado en ModelState,
            // devolvemos HTTP 400 y no continuamos con la compra.
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // ------------------------------------------------------------
            // 5. COMPROBAR QUE NO HAY LIBROS REPETIDOS
            // ------------------------------------------------------------

            // Obtenemos los identificadores de todos los libros seleccionados.
            var libroIds = compraForCreate.CompraItems!
                .Select(item => item.LibroId)
                .ToList();

            // Distinct elimina los identificadores duplicados.
            // Si el número de elementos cambia, significa que hay
            // algún libro repetido en la lista.
            if (libroIds.Distinct().Count() != libroIds.Count)
            {
                // Añadimos el error correspondiente.
                ModelState.AddModelError(
                    "CompraItems",
                    "Hay libros repetidos. Indica la cantidad en un único artículo.");

                // Devolvemos HTTP 400.
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // ------------------------------------------------------------
            // 6. RECUPERAR LOS LIBROS DE LA BASE DE DATOS
            // ------------------------------------------------------------

            // Buscamos los libros cuyos identificadores están en libroIds.
            var libros = await _context.Libros

                // Incluimos el género de cada libro.
                .Include(l => l.Genero)

                // Incluimos su editorial.
                .Include(l => l.Editorial)

                // Seleccionamos únicamente los libros solicitados.
                .Where(l => libroIds.Contains(l.Id))

                // Ejecutamos la consulta de forma asíncrona.
                .ToListAsync();

            // ------------------------------------------------------------
            // 7. VALIDAR LOS LIBROS, LAS CANTIDADES Y EL STOCK
            // ------------------------------------------------------------

            // Recorremos todos los artículos que el cliente quiere comprar.
            foreach (var item in compraForCreate.CompraItems!)
            {
                // Buscamos el libro correspondiente al identificador del DTO.
                var libro = libros.FirstOrDefault(l => l.Id == item.LibroId);

                // Comprobamos si el libro no existe.
                if (libro == null)
                {
                    ModelState.AddModelError(
                        "CompraItems",
                        $"No existe el libro con identificador {item.LibroId}.");
                }

                // Si existe, comprobamos que la cantidad sea mayor que 1.
                else if (item.Cantidad < 2)
                {
                    ModelState.AddModelError(
                        "CompraItems",
                        $"La cantidad de '{libro.Titulo}' debe ser mayor que 1.");
                }

                // Si la cantidad es válida, comprobamos que haya stock suficiente.
                else if (item.Cantidad > libro.Stock)
                {
                    ModelState.AddModelError(
                        "CompraItems",
                        $"No hay stock suficiente de '{libro.Titulo}'. " +
                        $"Unidades disponibles: {libro.Stock}.");
                }
            }

            // Comprobamos si se han producido errores en las validaciones.
            if (!ModelState.IsValid)
            {
                // Si existe algún error, devolvemos HTTP 400.
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // ------------------------------------------------------------
            // 8. CREAR LA COMPRA
            // ------------------------------------------------------------

            // Creamos el objeto Compra.
            // Inicialmente le pasamos una lista vacía de artículos.
            var compra = new Compra(
                DateTime.Now,
                compraForCreate.CodigoDescuento,
                metodoPago!,
                new List<ComprarItem>())
            {
                // Asociamos el usuario que realiza la compra.
                // El operador ! indica que confiamos en que no sea null,
                // porque ya hemos comprobado su existencia.
                ApplicationUser = usuario!,

                // Guardamos los datos personales del comprador.
                Nombre = compraForCreate.NombreComprador,
                Apellidos = compraForCreate.ApellidoComprador,
                Direccion = compraForCreate.Direccion,
                Telefono = compraForCreate.TelefonoComprador
            };

            // ------------------------------------------------------------
            // 9. CREAR LOS ARTÍCULOS Y ACTUALIZAR EL STOCK
            // ------------------------------------------------------------

            // Recorremos los artículos seleccionados por el cliente.
            foreach (var item in compraForCreate.CompraItems!)
            {
                // Recuperamos el libro correspondiente.
                var libro = libros.First(l => l.Id == item.LibroId);

                // Creamos un ComprarItem y lo añadimos a la compra.
                // El constructor obtiene el precio real del libro.
                compra.ComprarItems.Add(
                    new ComprarItem(libro, item.Cantidad, compra));

                // Restamos del stock las unidades compradas.
                libro.Stock -= item.Cantidad;
            }

            // ------------------------------------------------------------
            // 10. CALCULAR EL PRECIO TOTAL
            // ------------------------------------------------------------

            // Calculamos el precio total sumando:
            // precio de cada libro multiplicado por su cantidad.
            compra.PrecioTotal = compra.ComprarItems.Sum(
                item => item.PrecioCompra * item.Cantidad);

            // ------------------------------------------------------------
            // 11. PREPARAR LOS DTO DE LOS ARTÍCULOS
            // ------------------------------------------------------------

            // Convertimos los artículos de la compra en CompraItemDTO.
            // Utilizamos los datos recuperados de la base de datos,
            // no los datos descriptivos enviados por el cliente.
            var detalleItems = compra.ComprarItems
                .Select(item => new CompraItemDTO(
                    item.LibroId,
                    item.Libro.Titulo,
                    item.Libro.Genero.Nombre,
                    item.Libro.Editorial.Nombre,
                    item.PrecioCompra,
                    item.Cantidad))
                .ToList();

            // Añadimos la compra al contexto.
            // Entity Framework también seguirá las entidades relacionadas
            // que forman parte del nuevo objeto.
            _context.Compras.Add(compra);

            // ------------------------------------------------------------
            // 12. GUARDAR LA COMPRA EN LA BASE DE DATOS
            // ------------------------------------------------------------

            try
            {
                // Guardamos los cambios:
                // compra, artículos, método de pago y actualización del stock.
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Registramos la excepción producida al guardar.
                _logger.LogError(ex, "Error al guardar la compra.");

                // Devolvemos HTTP 409 Conflict.
                return Conflict(
                    "No se pudo guardar la compra. Inténtalo de nuevo.");
            }

            // ------------------------------------------------------------
            // 13. CONSTRUIR EL DTO DE RESPUESTA
            // ------------------------------------------------------------

            // Creamos el DTO con los datos definitivos de la compra.
            var compraDetail = new ComprasDetailDTO(
                compra.Id,
                compra.FechaCompra,
                usuario!.UserName!,
                compra.Nombre,
                compra.Apellidos,
                compra.Direccion,
                compra.Telefono,
                compraForCreate.MetodoDePago,
                compra.CodigoDescuento,
                detalleItems);

            // ------------------------------------------------------------
            // 14. DEVOLVER LA RESPUESTA
            // ------------------------------------------------------------

            // Devolvemos HTTP 201 Created.
            // También indicamos la acción que permite consultar el detalle
            // de la compra recién creada y su identificador.
            return CreatedAtAction(
                nameof(GetDetailsForCompras),
                new { id = compra.Id },
                compraDetail);
        }
    }
}
