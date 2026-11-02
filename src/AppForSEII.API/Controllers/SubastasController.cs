// SubastasController: métodos de la API para el CU3 Subastar libros
using AppForSEII.API.DTOs.SubastarLibrosDTOs;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubastasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubastasController> _logger;

        public SubastasController(ApplicationDbContext context, ILogger<SubastasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // CU3 paso 7: resumen de la subasta (datos del cliente, fecha, precio total, método de pago y libros)
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(SubastaDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForSubasta(int id)
        {
            var subasta = await _context.Subastas
                .Where(s => s.Id == id)
                .Include(s => s.ApplicationUser)
                .Include(s => s.MetodoDePago)
                .Include(s => s.SubastaItems)
                    .ThenInclude(si => si.Libro)
                        .ThenInclude(l => l.Editorial)
                .Select(s => new SubastaDetailDTO(
                    s.Id,
                    s.FechaSubasta,
                    s.ApplicationUser.UserName!,
                    s.ApplicationUser.Name ?? "",
                    s.ApplicationUser.Surname ?? "",
                    s.ApplicationUser.PhoneNumber ?? "",
                    s.MetodoDePago is Visa ? "Visa" :
                        s.MetodoDePago is GooglePay ? "GooglePay" :
                        s.MetodoDePago is PayPal ? "PayPal" :
                        "Desconocido",
                    s.SubastaItems
                        .Select(si => new SubastaItemDTO(
                            si.LibroId,
                            si.Libro.Titulo,
                            si.Libro.Autor,
                            si.Libro.Editorial.Nombre,
                            si.PrecioPuja,
                            si.Descripcion))
                        .ToList()))
                .FirstOrDefaultAsync();

            if (subasta == null)
            {
                _logger.LogError($"Error: no existe una subasta con id {id}.");
                return NotFound();
            }

            return Ok(subasta);
        }

        // ⬇⬇⬇ NUEVO
        // CU3 pasos 5-6: el cliente confirma las pujas y se guarda la subasta
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(SubastaDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateSubasta(SubastaForCreateDTO subastaForCreate)
        {
            // FA4: tiene que haber al menos un libro en la lista de pujas
            if (subastaForCreate.SubastaItems == null || subastaForCreate.SubastaItems.Count == 0)
            {
                ModelState.AddModelError("SubastaItems", "Debes seleccionar al menos un libro.");
            }

            // El usuario tiene que estar registrado
            var usuario = await _context.ApplicationUsers
                .FirstOrDefaultAsync(u => u.UserName == subastaForCreate.CustomerUserName);

            if (usuario == null)
            {
                ModelState.AddModelError("CustomerUserName", "El usuario no está registrado.");
            }

            // FA5: el método de pago y sus datos son obligatorios
            MetodosdePago? metodoDePago = null;

            switch (subastaForCreate.MetodoDePago)
            {
                case "Visa":
                    if (string.IsNullOrWhiteSpace(subastaForCreate.NumeroTarjeta))
                    {
                        ModelState.AddModelError("NumeroTarjeta", "Debes introducir el número de tarjeta.");
                    }
                    if (!subastaForCreate.FechaCaducidad.HasValue || subastaForCreate.FechaCaducidad.Value.Date < DateTime.Today)
                    {
                        ModelState.AddModelError("FechaCaducidad", "Debes introducir una fecha de caducidad válida.");
                    }
                    metodoDePago = new Visa
                    {
                        NumeroTarjeta = subastaForCreate.NumeroTarjeta ?? "",
                        FechaCaducidad = subastaForCreate.FechaCaducidad ?? DateTime.Today
                    };
                    break;

                case "GooglePay":
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
                    if (string.IsNullOrWhiteSpace(subastaForCreate.TelefonoPayPal))
                    {
                        ModelState.AddModelError("TelefonoPayPal", "Debes introducir el teléfono de PayPal.");
                    }
                    metodoDePago = new PayPal
                    {
                        NumeroTelefono = subastaForCreate.TelefonoPayPal ?? ""
                    };
                    break;

                default:
                    ModelState.AddModelError("MetodoDePago", "El método de pago debe ser Visa, GooglePay o PayPal.");
                    break;
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Un libro solo puede aparecer una vez en la misma subasta (clave LibroId + SubastaId)
            var libroIds = subastaForCreate.SubastaItems!
                .Select(si => si.LibroId)
                .ToList();

            if (libroIds.Distinct().Count() != libroIds.Count)
            {
                ModelState.AddModelError("SubastaItems", "Hay libros repetidos en la lista de pujas.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Se recuperan los libros de la base de datos
            var libros = await _context.Libros
                .Include(l => l.Editorial)
                .Where(l => libroIds.Contains(l.Id))
                .ToListAsync();

            foreach (var item in subastaForCreate.SubastaItems!)
            {
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
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Los datos del cliente del paso 5 se guardan en su usuario (ApplicationUser)
            usuario!.Name = subastaForCreate.Nombre;
            usuario.Surname = subastaForCreate.Apellidos;
            usuario.PhoneNumber = subastaForCreate.Telefono;

            // Se crea la subasta con su constructor y una lista vacía, y se le añaden las pujas
            var subasta = new Subasta(DateTime.Now, usuario, metodoDePago!, new List<SubastaItem>());

            foreach (var item in subastaForCreate.SubastaItems!)
            {
                var libro = libros.First(l => l.Id == item.LibroId);
                subasta.SubastaItems.Add(new SubastaItem(libro, subasta, item.PrecioPuja, item.Descripcion));
            }

            // Precio total = suma de las pujas
            subasta.PrecioSubasta = subasta.SubastaItems.Sum(si => si.PrecioPuja);

            _context.Subastas.Add(subasta);

            try
            {
                // Guarda la subasta, sus pujas, el método de pago y los datos del usuario
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar la subasta.");
                return Conflict("No se pudo guardar la subasta. Inténtalo de nuevo.");
            }

            // Paso 7: se devuelve el resumen de la subasta creada
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

            return CreatedAtAction(nameof(GetDetailsForSubasta), new { id = subasta.Id }, subastaDetail);
        }
        // ⬆⬆⬆ FIN NUEVO
    }
}