
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppForSEII.API.Data;
using AppForSEII.API.DTOs.LibroDTOs;

namespace AppForSEII.API.Controllers
{
    // Indicamos que las peticiones de este controlador
    // tendrán la ruta api/Libro.
    [Route("api/[controller]")]

    // Activa funcionalidades automáticas de API,
    // como la validación de los modelos.
    [ApiController]
    public class LibroController : ControllerBase
    {
        // ApplicationDbContext permite acceder a la base de datos
        // mediante Entity Framework Core.
        private readonly ApplicationDbContext _context;

        // ILogger permite registrar errores e información
        // durante la ejecución del controlador.
        private readonly ILogger<LibroController> _logger;

        // CONSTRUCTOR:
        // Recibe las dependencias necesarias mediante inyección de dependencias.
        public LibroController(
            ApplicationDbContext context,
            ILogger<LibroController> logger)
        {
            // Guardamos el contexto recibido.
            _context = context;

            // Guardamos el logger recibido.
            _logger = logger;
        }

        // HTTP GET:
        // Permite ejecutar el método mediante una petición GET.
        [HttpGet]

        // Utiliza el nombre del método como parte de la ruta.
        // Ruta: api/Libro/ComputeDivison
        [Route("[action]")]

        // Indicamos que puede devolver un decimal con HTTP 200.
        [ProducesResponseType(typeof(decimal), (int)HttpStatusCode.OK)]

        // Indicamos que puede devolver un mensaje de error con HTTP 400.
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> ComputeDivison(decimal op1, decimal op2)
        {
            // Comprobamos que el divisor no sea cero.
            if (op2 == 0)
            {
                // Registramos el error en el logger.
                _logger.LogError($"{DateTime.Now} Exception: op2=0, division by 0");

                // Devolvemos BadRequest (HTTP 400).
                return BadRequest("op2 cannot be 0");
            }

            // Realizamos la división y redondeamos el resultado
            // a dos posiciones decimales.
            decimal result = decimal.Round(op1 / op2, 2);

            // Devolvemos el resultado con HTTP 200.
            return Ok(result);
        }

        // HTTP GET:
        // Permite obtener todos los libros de la base de datos.
        [HttpGet]
        [Route("[action]")]

        // Indicamos que devuelve una lista de libros con HTTP 200.
        [ProducesResponseType(typeof(IList<Libro>), (int)HttpStatusCode.OK)]

        // Indicamos que puede devolver un error HTTP 400.
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> GetLibros()
        {
            // Consultamos todos los libros almacenados.
            // ToListAsync ejecuta la consulta de forma asíncrona
            // y convierte el resultado en una lista.
            IList<Libro> libros = await _context.Libros.ToListAsync();

            // Devolvemos la lista de libros con HTTP 200.
            return Ok(libros);
        }

        // HTTP GET:
        // Obtiene los libros disponibles para realizar una compra.
        [HttpGet]
        [Route("[action]")]

        // Indicamos que devuelve una lista de LibroParaComprarDTO.
        [ProducesResponseType(typeof(IList<LibroParaComprarDTO>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetLibrosParaComprar(
            string? genero,
            decimal? precioMaximo)
        {
            // Comprobamos si se ha introducido un precio máximo negativo.
            if (precioMaximo.HasValue && precioMaximo.Value < 0)
            {
                // Registramos el error.
                _logger.LogError("El precio máximo no puede ser negativo.");

                // Devolvemos HTTP 400 porque el parámetro no es válido.
                return BadRequest("El precio máximo no puede ser negativo.");
            }

            // Eliminamos los espacios al principio y al final del género.
            // El operador ?. evita ejecutar Trim si genero es null.
            genero = genero?.Trim();

            // Consultamos los libros de la base de datos.
            var libros = await _context.Libros

                // Aplicamos los filtros mediante Where.
                .Where(l =>
                    // Solo mostramos libros que tengan stock disponible.
                    l.Stock > 0

                    // Si no se indica género, no filtramos por género.
                    // Si se indica, debe coincidir con el género del libro.
                    && (string.IsNullOrEmpty(genero)
                        || l.Genero.Nombre == genero)

                    // Si no se indica precio máximo, no filtramos por precio.
                    // Si se indica, el precio debe ser menor o igual al máximo.
                    && (!precioMaximo.HasValue
                        || l.PrecioCompra <= precioMaximo.Value))

                // Ordenamos los libros alfabéticamente por título.
                .OrderBy(l => l.Titulo)

                // Transformamos cada entidad Libro en un DTO.
                // Solo devolvemos los datos necesarios para la compra.
                .Select(l => new LibroParaComprarDTO(
                    l.Id,
                    l.Titulo,
                    l.FechaLanzamiento,
                    l.Editorial.Nombre,
                    l.PrecioCompra,
                    l.Genero.Nombre
                ))

                // Ejecutamos la consulta y obtenemos una lista asíncronamente.
                .ToListAsync();

            // Devolvemos los libros encontrados con HTTP 200.
            return Ok(libros);
        }

        // HTTP GET:
        // Obtiene los libros que pueden utilizarse para una reposición.
        [HttpGet]
        [Route("[action]")]

        // Indicamos el tipo de respuesta esperado.
        [ProducesResponseType(typeof(IList<LibroParaReponerDTO>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetLibrosParaReponer(
            string? autor,
            string? genero)
        {
            // Eliminamos los espacios innecesarios del autor.
            autor = autor?.Trim();

            // Eliminamos los espacios innecesarios del género.
            genero = genero?.Trim();

            // Consultamos los libros de la base de datos.
            var libros = await _context.Libros

                // Incluimos el género relacionado con cada libro.
                // Así podemos acceder a sus datos de navegación.
                .Include(l => l.Genero)

                // Aplicamos los filtros de la reposición.
                .Where(l =>

                    // Solo seleccionamos libros sin stock.
                    l.Stock == 0

                    // Si no se indica autor, no filtramos por él.
                    // Si se indica, buscamos coincidencias parciales.
                    && (string.IsNullOrEmpty(autor)
                        || l.Autor.Contains(autor))

                    // Si no se indica género, no filtramos por él.
                    // Si se indica, debe coincidir exactamente.
                    && (string.IsNullOrEmpty(genero)
                        || l.Genero.Nombre == genero))

                // Ordenamos alfabéticamente por título.
                .OrderBy(l => l.Titulo)

                // Convertimos cada libro en un DTO de reposición.
                .Select(l => new LibroParaReponerDTO(
                    l.Id,
                    l.Titulo,
                    l.FechaLanzamiento,
                    l.Autor,
                    l.PrecioCompra, // Se utiliza PrecioCompra porque Libro no tiene PrecioReposicion.
                    l.Genero.Nombre))

                // Ejecutamos la consulta de forma asíncrona.
                .ToListAsync();

            // Devolvemos los libros encontrados.
            return Ok(libros);
        }

        // HTTP GET:
        // Obtiene los libros disponibles para realizar una subasta.
        [HttpGet]
        [Route("[action]")]

        // Indicamos que devuelve una lista de LibroParaSubastarDTO.
        [ProducesResponseType(typeof(IList<LibroParaSubastarDTO>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetLibrosParaSubastar(
            string? titulo,
            string? editorial)
        {
            // Consultamos los libros de la base de datos.
            var libros = await _context.Libros

                // Incluimos la editorial relacionada con cada libro.
                .Include(l => l.Editorial)

                // Aplicamos las condiciones de búsqueda.
                .Where(l =>

                    // Solo se pueden seleccionar libros con stock disponible.
                    l.Stock > 0

                    // Si se introduce un título, buscamos coincidencias.
                    // Si titulo es null, no aplicamos este filtro.
                    && (l.Titulo.Contains(titulo) || titulo == null)

                    // Si se introduce una editorial, debe coincidir.
                    // Si editorial es null, no aplicamos este filtro.
                    && (l.Editorial.Nombre.Equals(editorial)
                        || editorial == null))

                // Ordenamos los resultados alfabéticamente.
                .OrderBy(l => l.Titulo)

                // Convertimos cada entidad Libro en un DTO.
                .Select(l => new LibroParaSubastarDTO(
                    l.Id,
                    l.Titulo,
                    l.Editorial.Nombre,
                    l.Autor,
                    l.TipoLibro))

                // Ejecutamos la consulta y obtenemos la lista.
                .ToListAsync();

            // Devolvemos los libros encontrados con HTTP 200.
            return Ok(libros);
        }
    }
}
