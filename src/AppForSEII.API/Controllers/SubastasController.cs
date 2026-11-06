// CONTROLADOR del CU3 Subastar libros: tiene el DETAILS (GetDetailsForSubasta) y el POST (CreateSubasta)
// El SELECT (GetLibrosParaSubastar) está en LibroController porque devuelve libros
// Lo prueban: test/AppForSEII.UT/SubastasController_test (GetDetailsForSubasta_test y CreateSubasta_test)
using AppForSEII.API.DTOs.SubastarLibrosDTOs;

namespace AppForSEII.API.Controllers
{
    // Las URLs de este controlador empiezan por api/Subastas ([controller] = nombre de la clase sin "Controller")
    [Route("api/[controller]")]
    // [ApiController] = comprueba solo los atributos del DTO antes de entrar al método y, si fallan, devuelve 400
    [ApiController]
    public class SubastasController : ControllerBase
    {
        // Acceso a la base de datos (tablas Subastas, Libros, ApplicationUsers...)
        private readonly ApplicationDbContext _context;
        // Sirve para escribir errores en la terminal (LogError)
        private readonly ILogger<SubastasController> _logger;

        // Constructor: ASP.NET le pasa el contexto y el logger solo (inyección de dependencias); en las pruebas se pasan a mano
        public SubastasController(ApplicationDbContext context, ILogger<SubastasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ===== CU3 DETAILS (paso 7): resumen de una subasta (cliente, fecha, precio total, método de pago y libros) =====
        // Lo llama la web con: GET /api/Subastas/GetDetailsForSubasta?id=1
        // También lo usa CreateSubasta en el CreatedAtAction (la URL de la cabecera location del 201)
        [HttpGet]
        [Route("[action]")]
        // Para Swagger: 200 con un SubastaDetailDTO, o 404 si la subasta no existe
        [ProducesResponseType(typeof(SubastaDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForSubasta(int id)
        {
            // Busca la subasta en la tabla Subastas
            var subasta = await _context.Subastas
                // Solo la subasta con ese id
                .Where(s => s.Id == id)
                // Include = datos relacionados (JOIN): cliente, método de pago y pujas
                .Include(s => s.ApplicationUser)
                .Include(s => s.MetodoDePago)
                .Include(s => s.SubastaItems)
                    // ThenInclude = de cada puja trae su libro, y de cada libro su editorial
                    .ThenInclude(si => si.Libro)
                        .ThenInclude(l => l.Editorial)
                // Select = convierte la entidad Subasta en un SubastaDetailDTO (en el orden de su constructor)
                .Select(s => new SubastaDetailDTO(
                    s.Id,
                    s.FechaSubasta,
                    // ! = le dice al compilador que UserName no es null
                    s.ApplicationUser.UserName!,
                    // ?? "" = si es null, pone texto vacío
                    s.ApplicationUser.Name ?? "",
                    s.ApplicationUser.Surname ?? "",
                    s.ApplicationUser.PhoneNumber ?? "",
                    // "is" mira de qué clase hija es el método de pago y devuelve su nombre
                    // MODIFICAR: un método de pago nuevo se añadiría aquí con otro "is"
                    s.MetodoDePago is Visa ? "Visa" :
                        s.MetodoDePago is GooglePay ? "GooglePay" :
                        s.MetodoDePago is PayPal ? "PayPal" :
                        // Las subastas del SeedData usan el MetodosdePago genérico, por eso salen como "Desconocido"
                        "Desconocido",
                    // Convierte cada SubastaItem (puja) en un SubastaItemDTO
                    s.SubastaItems
                        .Select(si => new SubastaItemDTO(
                            si.LibroId,
                            si.Libro.Titulo,
                            si.Libro.Autor,
                            si.Libro.Editorial.Nombre,
                            si.PrecioPuja,
                            si.Descripcion))
                        .ToList()))
                // Devuelve la primera que encuentre, o null si no hay ninguna
                .FirstOrDefaultAsync();

            // Si no existe: se apunta en el log y se devuelve 404 Not Found
            if (subasta == null)
            {
                _logger.LogError($"Error: no existe una subasta con id {id}.");
                return NotFound();
            }

            // 200 OK con el resumen de la subasta
            return Ok(subasta);
        }

        // ===== CU3 POST (pasos 5-6): el cliente confirma las pujas y se guarda la subasta =====
        // Lo llama la web con: POST /api/Subastas/CreateSubasta y un JSON con un SubastaForCreateDTO
        // Orden: 1) comprobaciones (400 si falla algo)  2) crear la subasta  3) guardar  4) devolver 201
        [HttpPost]
        [Route("[action]")]
        // Para Swagger: 201 si se crea, 400 si hay errores de validación, 409 si falla al guardar
        [ProducesResponseType(typeof(SubastaDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateSubasta(SubastaForCreateDTO subastaForCreate)
        {
            // FA4: tiene que haber al menos un libro en la lista de pujas
            if (subastaForCreate.SubastaItems == null || subastaForCreate.SubastaItems.Count == 0)
            {
                // AddModelError(campo, mensaje) = apunta un error; no corta, se siguen haciendo comprobaciones
                ModelState.AddModelError("SubastaItems", "Debes seleccionar al menos un libro.");
            }

            // El usuario tiene que estar registrado: se busca por su UserName (email)
            var usuario = await _context.ApplicationUsers
                // FirstOrDefaultAsync = el primero que cumpla la condición, o null si no hay ninguno
                .FirstOrDefaultAsync(u => u.UserName == subastaForCreate.CustomerUserName);

            if (usuario == null)
            {
                ModelState.AddModelError("CustomerUserName", "El usuario no está registrado.");
            }

            // FA5: el método de pago y sus datos son obligatorios
            // ? = puede ser null; se rellena en el switch según el método elegido
            MetodosdePago? metodoDePago = null;

            // switch = según el texto de MetodoDePago entra en un case u otro
            switch (subastaForCreate.MetodoDePago)
            {
                case "Visa":
                    // Visa necesita número de tarjeta
                    if (string.IsNullOrWhiteSpace(subastaForCreate.NumeroTarjeta))
                    {
                        ModelState.AddModelError("NumeroTarjeta", "Debes introducir el número de tarjeta.");
                    }
                    // Y una fecha de caducidad que no haya pasado ya
                    if (!subastaForCreate.FechaCaducidad.HasValue || subastaForCreate.FechaCaducidad.Value.Date < DateTime.Today)
                    {
                        ModelState.AddModelError("FechaCaducidad", "Debes introducir una fecha de caducidad válida.");
                    }
                    // Crea el objeto Visa con los datos del DTO (?? = valor por defecto si vienen null)
                    metodoDePago = new Visa
                    {
                        NumeroTarjeta = subastaForCreate.NumeroTarjeta ?? "",
                        FechaCaducidad = subastaForCreate.FechaCaducidad ?? DateTime.Today
                    };
                    // break = sale del switch
                    break;

                case "GooglePay":
                    // GooglePay necesita el correo
                    if (string.IsNullOrWhiteSpace(subastaForCreate.EmailGooglePay))
                    {
                        ModelState.AddModelError("EmailGooglePay", "Debes introducir el correo de GooglePay.");
                    }
                    metodoDePago = new GooglePay
                    {
                        Email = subastaForCreate.EmailGooglePay ?? ""
                    };
                    break;

                case "PayPal":
                    // PayPal necesita el teléfono
                    if (string.IsNullOrWhiteSpace(subastaForCreate.TelefonoPayPal))
                    {
                        ModelState.AddModelError("TelefonoPayPal", "Debes introducir el teléfono de PayPal.");
                    }
                    metodoDePago = new PayPal
                    {
                        NumeroTelefono = subastaForCreate.TelefonoPayPal ?? ""
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

            // Un libro solo puede aparecer una vez en la misma subasta (clave LibroId + SubastaId)
            // Se saca la lista con los ids de los libros de las pujas
            var libroIds = subastaForCreate.SubastaItems!
                .Select(si => si.LibroId)
                .ToList();

            // Distinct quita los repetidos: si cambia el número de elementos, había algún libro repetido
            if (libroIds.Distinct().Count() != libroIds.Count)
            {
                ModelState.AddModelError("SubastaItems", "Hay libros repetidos en la lista de pujas.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Se recuperan de la base de datos los libros de la lista (con su editorial, para el resumen final)
            var libros = await _context.Libros
                .Include(l => l.Editorial)
                .Where(l => libroIds.Contains(l.Id))
                .ToListAsync();

            // Se comprueba cada puja con su libro
            foreach (var item in subastaForCreate.SubastaItems!)
            {
                // Busca el libro en la lista traída de la BD (null si no existe)
                var libro = libros.FirstOrDefault(l => l.Id == item.LibroId);

                if (libro == null)
                {
                    ModelState.AddModelError("SubastaItems", $"No existe el libro con identificador {item.LibroId}.");
                }
                else if (libro.Stock <= 0)
                {
                    // FA0: solo se subastan libros con stock
                    ModelState.AddModelError("SubastaItems", $"El libro '{libro.Titulo}' no está disponible para subastar.");
                }
                else if (item.PrecioPuja < 1)
                {
                    // FA2: precio de puja incorrecto
                    ModelState.AddModelError("SubastaItems", $"La puja por '{libro.Titulo}' debe ser como mínimo 1.");
                }
                // MODIFICAR: una comprobación nueva por libro (p. ej. una puja máxima) sería otro "else if" aquí
            }

            // Si algún libro ha fallado: 400 Bad Request con los errores
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Los datos del cliente del paso 5 se guardan en su usuario (ApplicationUser)
            // usuario! = aquí ya no puede ser null porque se comprobó antes
            usuario!.Name = subastaForCreate.Nombre;
            usuario.Surname = subastaForCreate.Apellidos;
            usuario.PhoneNumber = subastaForCreate.Telefono;

            // Se crea la subasta con su constructor y una lista vacía, y se le añaden las pujas
            // DateTime.Now = fecha y hora de ahora; metodoDePago! = ya no es null
            var subasta = new Subasta(DateTime.Now, usuario, metodoDePago!, new List<SubastaItem>());

            // Por cada puja se crea un SubastaItem con su libro y se añade a la subasta
            foreach (var item in subastaForCreate.SubastaItems!)
            {
                // First (sin OrDefault) porque aquí el libro seguro que existe
                var libro = libros.First(l => l.Id == item.LibroId);
                subasta.SubastaItems.Add(new SubastaItem(libro, subasta, item.PrecioPuja, item.Descripcion));
            }

            // Precio total = suma de las pujas (el constructor lo calculó con la lista vacía, por eso se recalcula)
            subasta.PrecioSubasta = subasta.SubastaItems.Sum(si => si.PrecioPuja);

            // Add = marca la subasta (con sus pujas y su método de pago) para insertarla; todavía no se guarda
            _context.Subastas.Add(subasta);

            try
            {
                // Guarda la subasta, sus pujas, el método de pago y los datos del usuario (todo o nada)
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Si falla al guardar: se apunta en el log y se devuelve 409 Conflict
                _logger.LogError(ex, "Error al guardar la subasta.");
                return Conflict("No se pudo guardar la subasta. Inténtalo de nuevo.");
            }

            // Paso 7: se devuelve el resumen de la subasta creada (el Id ya lo ha puesto la BD al guardar)
            var subastaDetail = new SubastaDetailDTO(
                subasta.Id,
                subasta.FechaSubasta,
                usuario.UserName!,
                subastaForCreate.Nombre,
                subastaForCreate.Apellidos,
                subastaForCreate.Telefono,
                subastaForCreate.MetodoDePago,
                subasta.SubastaItems
                    .Select(si => new SubastaItemDTO(
                        si.LibroId,
                        si.Libro.Titulo,
                        si.Libro.Autor,
                        si.Libro.Editorial.Nombre,
                        si.PrecioPuja,
                        si.Descripcion))
                    .ToList());

            // 201 Created con el resumen; también da la URL de GetDetailsForSubasta para consultarla (cabecera location)
            return CreatedAtAction(nameof(GetDetailsForSubasta), new { id = subasta.Id }, subastaDetail);
        }
    }
}