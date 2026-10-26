using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs.LibroDTOs;
namespace AppForSEII.API.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class LibroController : ControllerBase
    {
        //Usamos el servicio de ApplicationDbContext para acceder a la base de datos
        private readonly ApplicationDbContext _context;
        //Usamos el logger para registrar los errores y la información del controlador
        private readonly ILogger<LibroController> _logger;

        public LibroController(ApplicationDbContext context, ILogger<LibroController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(decimal), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> ComputeDivison(decimal op1, decimal op2)
        {
            if (op2 == 0)
            {
                _logger.LogError($"{DateTime.Now} Exception: op2=0, division by 0");
                return BadRequest("op2 cannot be 0");
            }
            decimal result = decimal.Round(op1 / op2, 2);
            return Ok(result);
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<Libro>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetLibros()
        {
            IList<Libro> libros = await _context.Libros.ToListAsync();
            return Ok(libros);
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<LibroParaComprarDTO>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetLibrosParaComprar(string? genero, decimal? precioMaximo)
        {
            if (precioMaximo.HasValue && precioMaximo.Value < 0)
            {
                _logger.LogError("El precio máximo no puede ser negativo.");
                return BadRequest("El precio máximo no puede ser negativo.");
            }
            genero = genero?.Trim();

            var libros = await _context.Libros
            .Where(l => l.Stock > 0 && (string.IsNullOrEmpty(genero) || l.Genero.Nombre == genero) &&
            (!precioMaximo.HasValue || l.PrecioCompra <= precioMaximo.Value))
            .OrderBy(l => l.Titulo)
            .Select(l => new LibroParaComprarDTO(
                l.Id,
                l.Titulo,
                l.FechaLanzamiento,
                l.Editorial.Nombre,
                l.PrecioCompra,
                l.Genero.Nombre
                ))
                .ToListAsync();
            return Ok(libros);
        }

         [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<LibroParaReponerDTO>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetLibrosParaReponer(string? autor, string? genero)
        {
            autor = autor?.Trim();
            genero = genero?.Trim();

            var libros = await _context.Libros
                .Include(l => l.Genero)
                .Where(l => l.Stock == 0
                    && (string.IsNullOrEmpty(autor) || l.Autor.Contains(autor))
                    && (string.IsNullOrEmpty(genero) || l.Genero.Nombre == genero))
                .OrderBy(l => l.Titulo)
                .Select(l => new LibroParaReponerDTO(
                    l.Id,
                    l.Titulo,
                    l.FechaLanzamiento,
                    l.Autor,
                    l.PrecioCompra, // Libro no tiene precio de reposición: se usa el precio de compra
                    l.Genero.Nombre))
                .ToListAsync();

            return Ok(libros);
        }

        
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<LibroParaSubastarDTO>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetLibrosParaSubastar(string? titulo, string? editorial)
        {
            var libros = await _context.Libros
                .Include(l => l.Editorial)
                .Where(l => l.Stock > 0
                    && (l.Titulo.Contains(titulo) || titulo == null)
                    && (l.Editorial.Nombre.Equals(editorial) || editorial == null))
                .OrderBy(l => l.Titulo)
                .Select(l => new LibroParaSubastarDTO(l.Id, l.Titulo, l.Editorial.Nombre, l.Autor, l.TipoLibro))
                .ToListAsync();

            return Ok(libros);
        }
        
    }
}