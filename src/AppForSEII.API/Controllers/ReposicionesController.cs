
using AppForSEII.API.DTOs.ReponerLibrosDTOs;

namespace AppForSEII.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReposicionesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReposicionesController> _logger;

        public ReposicionesController(ApplicationDbContext context, ILogger<ReposicionesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        
         [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(ReposicionDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForReposicion(int id)
        {
            var reposicion = await _context.Reposiciones
                .Include(r => r.ApplicationUser)
                .Include(r => r.MetodoDePago)
                .Include(r => r.reposicionItems)
                    .ThenInclude(ri => ri.Libro)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reposicion == null)
            {
                _logger.LogError($"Error: no existe una reposición con id {id}.");
                return NotFound();
            }

            return Ok(CrearDetalle(reposicion));
        }

        // CU2 pasos 5-6: el usuario confirma la reposición y se guarda
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ReposicionDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateReposicion(ReposicionForCreateDTO reposicionForCreate)
        {
            // FA2: tiene que haber al menos un libro seleccionado
            if (reposicionForCreate.ReposicionItems == null || reposicionForCreate.ReposicionItems.Count == 0)
            {
                ModelState.AddModelError("ReposicionItems", "Debes seleccionar al menos un libro.");
            }

            // El usuario tiene que estar registrado
            var usuario = await _context.ApplicationUsers
                .FirstOrDefaultAsync(u => u.UserName == reposicionForCreate.CustomerUserName);

            if (usuario == null)
            {
                ModelState.AddModelError("CustomerUserName", "El usuario no está registrado.");
            }

            // FA4: el método de pago y sus datos son obligatorios
            MetodosdePago? metodoDePago = null;

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

                default:
                    ModelState.AddModelError("MetodoDePago", "El método de pago debe ser Visa, GooglePay o PayPal.");
                    break;
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Un libro solo puede aparecer una vez en la misma reposición (clave LibroId + ReposicionId)
            var libroIds = reposicionForCreate.ReposicionItems!
                .Select(ri => ri.LibroId)
                .ToList();

            if (libroIds.Distinct().Count() != libroIds.Count)
            {
                ModelState.AddModelError("ReposicionItems", "Hay libros repetidos. Indica la cantidad en un único libro.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Se recuperan los libros de la base de datos
            var libros = await _context.Libros
                .Where(l => libroIds.Contains(l.Id))
                .ToListAsync();

            foreach (var item in reposicionForCreate.ReposicionItems!)
            {
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
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Los datos del usuario del paso 5 se guardan en su ApplicationUser (los dos apellidos van juntos en Surname)
            usuario!.Name = reposicionForCreate.Nombre;
            usuario.Surname = $"{reposicionForCreate.PrimerApellido} {reposicionForCreate.SegundoApellido}";
            usuario.PhoneNumber = reposicionForCreate.Telefono;

            // Se crea la reposición con una lista vacía y se le añaden los libros
            var reposicion = new Reposicion(DateTime.Now, reposicionForCreate.Comentario, metodoDePago!, new List<ReposicionItem>())
            {
                ApplicationUser = usuario
            };

            foreach (var item in reposicionForCreate.ReposicionItems!)
            {
                var libro = libros.First(l => l.Id == item.LibroId);

                // Libro no tiene precio de reposición propio: se usa su precio de compra (igual que en GetLibrosParaReponer)
                reposicion.reposicionItems.Add(new ReposicionItem(item.Cantidad, libro, reposicion, libro.PrecioCompra));

                // Al reponer, el libro vuelve a tener stock
                libro.Stock += item.Cantidad;
            }

            // Precio total = suma de precio de reposición x cantidad
            reposicion.PrecioTotal = reposicion.reposicionItems.Sum(ri => ri.PrecioDeReposicion * ri.CantidadReposicion);

            _context.Reposiciones.Add(reposicion);

            try
            {
                // Guarda la reposición, sus libros, el método de pago, el stock y los datos del usuario
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar la reposición.");
                return Conflict("No se pudo guardar la reposición. Inténtalo de nuevo.");
            }

            return CreatedAtAction(nameof(GetDetailsForReposicion), new { id = reposicion.Id }, CrearDetalle(reposicion));
        }

        // Convierte una Reposicion en su ReposicionDetailDTO (lo usan el GET y el POST)
        private static ReposicionDetailDTO CrearDetalle(Reposicion reposicion)
        {
            // Los dos apellidos están guardados juntos en Surname: se separan por el primer espacio
            string apellidos = reposicion.ApplicationUser.Surname ?? "";
            int espacio = apellidos.IndexOf(' ');
            string primerApellido = espacio >= 0 ? apellidos.Substring(0, espacio) : apellidos;
            string segundoApellido = espacio >= 0 ? apellidos.Substring(espacio + 1) : "";

            var items = reposicion.reposicionItems
                .Select(ri => new ReposicionItemDTO(
                    ri.LibroId,
                    ri.Libro.Titulo,
                    ri.Libro.Autor,
                    ri.PrecioDeReposicion,
                    ri.CantidadReposicion))
                .ToList();

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
        private static string NombreMetodoDePago(MetodosdePago metodoDePago)
        {
            return metodoDePago is Visa ? "Visa" :
                   metodoDePago is GooglePay ? "GooglePay" :
                   metodoDePago is PayPal ? "PayPal" :
                   "Desconocido";
        }
    }
}