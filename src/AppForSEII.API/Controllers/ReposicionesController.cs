
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

            return Ok(reposicion);
        }
    }
}
        
        