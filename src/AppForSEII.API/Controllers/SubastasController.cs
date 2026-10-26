// SubastasController: métodos de la API para  Subastar libros
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
    }
}