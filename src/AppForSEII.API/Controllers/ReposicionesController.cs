// CONTROLADOR del CU2 Reponer libros: tiene el DETAILS (GetDetailsForReposicion) y el POST (CreateReposicion)
// El SELECT (GetLibrosParaReponer) está en LibroController porque devuelve libros
// Lo prueban: test/AppForSEII.UT/ReposicionesController_test (GetDetailsForReposiciones.cs y CreateReposicion_test.cs)
using AppForSEII.API.DTOs.ReponerLibrosDTOs;

namespace AppForSEII.API.Controllers
{
    // Las URLs de este controlador empiezan por api/Reposiciones ([controller] = nombre de la clase sin "Controller")
    [Route("api/[controller]")]
     // [ApiController] = comprueba solo los atributos del DTO antes de entrar al método y, si fallan, devuelve 400
    [ApiController]
    public class ReposicionesController : ControllerBase
    {
         // Acceso a la base de datos (tablas Reposiciones, Libros, ApplicationUsers...)
        private readonly ApplicationDbContext _context;
         // Sirve para escribir errores en la terminal (LogError)
        private readonly ILogger<ReposicionesController> _logger;
        // Constructor: ASP.NET le pasa el contexto y el logger solo (inyección de dependencias); en las pruebas se pasan a mano
        public ReposicionesController(ApplicationDbContext context, ILogger<ReposicionesController> logger)
        {
            _context = context;
            _logger = logger;
        }

         // ===== CU2 DETAILS: resumen de una reposición (usuario, fecha, comentario, método de pago y libros) =====
        // Lo llama la web con: GET /api/Reposiciones/GetDetailsForReposicion?id=1
        // También lo usa CreateReposicion en el CreatedAtAction (la URL de la cabecera location del 201)
         [HttpGet]
        [Route("[action]")]
         // Para Swagger: 200 con un ReposicionDetailDTO, o 404 si no existe
        [ProducesResponseType(typeof(ReposicionDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForReposicion(int id)
        {
            // Busca la reposición en la tabla Reposiciones
            var reposicion = await _context.Reposiciones
             // Include = trae también los datos relacionados (JOIN): usuario, método de pago y libros
                .Include(r => r.ApplicationUser)
                .Include(r => r.MetodoDePago)
                .Include(r => r.reposicionItems)
                 // ThenInclude = de cada ReposicionItem trae también su libro
                    .ThenInclude(ri => ri.Libro)
                     // La primera con ese id, o null si no existe
                .FirstOrDefaultAsync(r => r.Id == id);
            // Si no existe: se apunta en el log y se devuelve 404 Not Found
            if (reposicion == null)
            {
                _logger.LogError($"Error: no existe una reposición con id {id}.");
                return NotFound();
            }
            // 200 OK con el resumen; CrearDetalle (abajo) convierte la entidad en DTO (devolver la entidad daba un ciclo en el JSON)
            return Ok(CrearDetalle(reposicion));
        }

         // ===== CU2 POST (pasos 5-6): el usuario confirma la reposición y se guarda =====
        // Lo llama la web con: POST /api/Reposiciones/CreateReposicion y un JSON con un ReposicionForCreateDTO
        // Orden: 1) comprobaciones (400 si falla algo)  2) crear la reposición  3) guardar  4) devolver 201
        [HttpPost]
        [Route("[action]")]
        // Para Swagger: 201 si se crea, 400 si hay errores de validación, 409 si falla al guardar
        [ProducesResponseType(typeof(ReposicionDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateReposicion(ReposicionForCreateDTO reposicionForCreate)
        {
            // FA2: tiene que haber al menos un libro seleccionado
            if (reposicionForCreate.ReposicionItems == null || reposicionForCreate.ReposicionItems.Count == 0)
            {
                // AddModelError(campo, mensaje) = apunta un error; no corta, se siguen haciendo comprobaciones
                ModelState.AddModelError("ReposicionItems", "Debes seleccionar al menos un libro.");
            }

            // El usuario tiene que estar registrado: se busca por su UserName (email)
            var usuario = await _context.ApplicationUsers
             // FirstOrDefaultAsync = el primero que cumpla la condición, o null si no hay ninguno
                .FirstOrDefaultAsync(u => u.UserName == reposicionForCreate.CustomerUserName);

            if (usuario == null)
            {
                ModelState.AddModelError("CustomerUserName", "El usuario no está registrado.");
            }

            // FA4: el método de pago y sus datos son obligatorios
             // ? = puede ser null; se rellena en el switch según el método elegido
            MetodosdePago? metodoDePago = null;
            // switch = según el texto de MetodoDePago entra en un case u otro
            switch (reposicionForCreate.MetodoDePago)
            {
                case "Visa":
                    if (string.IsNullOrWhiteSpace(reposicionForCreate.NumeroTarjeta))
                    {
                        ModelState.AddModelError("NumeroTarjeta", "Debes introducir el número de tarjeta.");
                    }
                    if (!reposicionForCreate.FechaCaducidad.HasValue || reposicionForCreate.FechaCaducidad.Value.Date < DateTime.Today)
                    {
                        ModelState.AddModelError("FechaCaducidad", "Debes introducir una fecha de caducidad válida.");
                    }
                    // Crea el objeto Visa con los datos del DTO (?? = valor por defecto si vienen null)
                    metodoDePago = new Visa
                    {
                        NumeroTarjeta = reposicionForCreate.NumeroTarjeta ?? "",
                        FechaCaducidad = reposicionForCreate.FechaCaducidad ?? DateTime.Today
                    };
                    break;

                case "GooglePay":
                    if (string.IsNullOrWhiteSpace(reposicionForCreate.EmailGooglePay))
                    {
                        ModelState.AddModelError("EmailGooglePay", "Debes introducir el correo de GooglePay.");
                    }
                    metodoDePago = new GooglePay
                    {
                        Email = reposicionForCreate.EmailGooglePay ?? ""
                    };
                    break;

                case "PayPal":
                    if (string.IsNullOrWhiteSpace(reposicionForCreate.TelefonoPayPal))
                    {
                        ModelState.AddModelError("TelefonoPayPal", "Debes introducir el teléfono de PayPal.");
                    }
                    metodoDePago = new PayPal
                    {
                        NumeroTelefono = reposicionForCreate.TelefonoPayPal ?? ""
                    };
                    break;
                 // Cualquier otro texto no es un método de pago válido
                // MODIFICAR: un método de pago nuevo sería otro "case" encima de este default
                default:
                    ModelState.AddModelError("MetodoDePago", "El método de pago debe ser Visa, GooglePay o PayPal.");
                    break;
            }
            // Si se ha apuntado algún error: 400 Bad Request con la lista de errores
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Un libro solo puede aparecer una vez en la misma reposición (clave LibroId + ReposicionId)
              // Se saca la lista con los ids de los libros
            var libroIds = reposicionForCreate.ReposicionItems!
                .Select(ri => ri.LibroId)
                .ToList();
            // Distinct quita los repetidos: si cambia el número de elementos, había algún libro repetido
            if (libroIds.Distinct().Count() != libroIds.Count)
            {
                ModelState.AddModelError("ReposicionItems", "Hay libros repetidos. Indica la cantidad en un único libro.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Se recuperan los libros de la base de datos
            var libros = await _context.Libros
                .Where(l => libroIds.Contains(l.Id))
                .ToListAsync();
            // Se comprueba cada libro de la lista
            foreach (var item in reposicionForCreate.ReposicionItems!)
            {
                // Busca el libro en la lista traída de la BD (null si no existe)
                var libro = libros.FirstOrDefault(l => l.Id == item.LibroId);

                if (libro == null)
                {
                    ModelState.AddModelError("ReposicionItems", $"No existe el libro con identificador {item.LibroId}.");
                }
                else if (libro.Stock > 0)
                {
                    // Paso 2 y FA0: solo se reponen libros no disponibles (stock = 0)
                    ModelState.AddModelError("ReposicionItems", $"El libro '{libro.Titulo}' tiene stock y no necesita reposición.");
                }
                else if (item.Cantidad < 2)
                {
                    // Paso 5: la cantidad de reposición debe ser mayor que 1
                    ModelState.AddModelError("ReposicionItems", $"La cantidad de '{libro.Titulo}' debe ser mayor que 1.");
                }
                 // MODIFICAR: una comprobación nueva por libro (p. ej. una cantidad máxima) sería otro "else if" aquí
            }
              // Si algún libro ha fallado: 400 Bad Request con los errores
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Los datos del usuario del paso 5 se guardan en su ApplicationUser (los dos apellidos van juntos en Surname)
            // usuario! = aquí ya no puede ser null porque se comprobó antes; $"{a} {b}" une los dos apellidos con un espacio
            usuario!.Name = reposicionForCreate.Nombre;
            usuario.Surname = $"{reposicionForCreate.PrimerApellido} {reposicionForCreate.SegundoApellido}";
            usuario.PhoneNumber = reposicionForCreate.Telefono;

            // Se crea la reposición con una lista vacía y se le añaden los libros
             // { ApplicationUser = usuario } = después del constructor se rellena el usuario (no está entre sus parámetros)
            var reposicion = new Reposicion(DateTime.Now, reposicionForCreate.Comentario, metodoDePago!, new List<ReposicionItem>())
            {
                ApplicationUser = usuario
            };
             // Por cada libro se crea un ReposicionItem y se añade a la reposición
            foreach (var item in reposicionForCreate.ReposicionItems!)
            {
                 // First (sin OrDefault) porque aquí el libro seguro que existe
                var libro = libros.First(l => l.Id == item.LibroId);

                // Libro no tiene precio de reposición propio: se usa su precio de compra (igual que en GetLibrosParaReponer)
                reposicion.reposicionItems.Add(new ReposicionItem(item.Cantidad, libro, reposicion, libro.PrecioCompra));

                // Al reponer, el libro vuelve a tener stock
                // MODIFICAR: si cambia cómo se actualiza el stock, es en esta línea
                libro.Stock += item.Cantidad;
            }

            // Precio total = suma de precio de reposición x cantidad
            reposicion.PrecioTotal = reposicion.reposicionItems.Sum(ri => ri.PrecioDeReposicion * ri.CantidadReposicion);
            // Add = marca la reposición (con sus libros y su método de pago) para insertarla; todavía no se guarda
            _context.Reposiciones.Add(reposicion);

            try
            {
                // Guarda la reposición, sus libros, el método de pago, el stock y los datos del usuario
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                  // Si falla al guardar: se apunta en el log y se devuelve 409 Conflict
                _logger.LogError(ex, "Error al guardar la reposición.");
                return Conflict("No se pudo guardar la reposición. Inténtalo de nuevo.");
            }
             // 201 Created con el resumen (CrearDetalle); también da la URL de GetDetailsForReposicion (cabecera location)
            return CreatedAtAction(nameof(GetDetailsForReposicion), new { id = reposicion.Id }, CrearDetalle(reposicion));
        }

        // Convierte una Reposicion en su ReposicionDetailDTO (lo usan el GET y el POST)
        private static ReposicionDetailDTO CrearDetalle(Reposicion reposicion)
        {
            // Los dos apellidos están guardados juntos en Surname: se separan por el primer espacio
            string apellidos = reposicion.ApplicationUser.Surname ?? "";
             // IndexOf = posición del primer espacio (-1 si no hay)
            int espacio = apellidos.IndexOf(' ');
            // Substring(0, espacio) = lo de antes del espacio; Substring(espacio + 1) = lo de después
            string primerApellido = espacio >= 0 ? apellidos.Substring(0, espacio) : apellidos;
            string segundoApellido = espacio >= 0 ? apellidos.Substring(espacio + 1) : "";
            // Convierte cada ReposicionItem en un ReposicionItemDTO
            var items = reposicion.reposicionItems
                .Select(ri => new ReposicionItemDTO(
                    ri.LibroId,
                    ri.Libro.Titulo,
                    ri.Libro.Autor,
                    ri.PrecioDeReposicion,
                    ri.CantidadReposicion))
                .ToList();
             // Crea el DTO del resumen (en el orden de su constructor)
            // MODIFICAR: si se añade un dato al ReposicionDetailDTO, se pasa aquí
            return new ReposicionDetailDTO(
                reposicion.Id,
                reposicion.FechaReposicion,
                reposicion.ApplicationUser.UserName!,
                reposicion.ApplicationUser.Name ?? "",
                primerApellido,
                segundoApellido,
                reposicion.ApplicationUser.PhoneNumber ?? "",
                NombreMetodoDePago(reposicion.MetodoDePago),
                reposicion.Comentario,
                items);
        }

        // Devuelve el nombre del método de pago según su tipo (Visa, GooglePay o PayPal)
         // "is" mira de qué clase hija es el objeto; si no es ninguna, "Desconocido"
        // MODIFICAR: un método de pago nuevo se añadiría aquí con otro "is"
        private static string NombreMetodoDePago(MetodosdePago metodoDePago)
        {
            return metodoDePago is Visa ? "Visa" :
                   metodoDePago is GooglePay ? "GooglePay" :
                   metodoDePago is PayPal ? "PayPal" :
                   "Desconocido";
        }
    }
}