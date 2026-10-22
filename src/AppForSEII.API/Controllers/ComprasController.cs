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
    [Route("api/[controller]")]
    [ApiController]
    public class ComprasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ComprasController> _logger;

        public ComprasController(
            ApplicationDbContext context,
            ILogger<ComprasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(
            typeof(ComprasDetailDTO),
            (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetDetailsForCompras(int id)
        {
            var compra = await _context.Compras
                .Where(c => c.Id == id)
                .Include(c => c.ApplicationUser)
                .Include(c => c.MetodoDePago)
                .Include(c => c.ComprarItems)
                    .ThenInclude(item => item.Libro)
                        .ThenInclude(libro => libro.Genero)
                .Include(c => c.ComprarItems)
                    .ThenInclude(item => item.Libro)
                        .ThenInclude(libro => libro.Editorial)
                .Select(c => new ComprasDetailDTO(
                    c.Id,
                    c.FechaCompra,
                    c.ApplicationUser.UserName!,
                    c.Nombre,
                    c.Apellidos,
                    c.Direccion,
                    c.Telefono,
                    c.MetodoDePago is Visa ? "Visa" :
                        c.MetodoDePago is GooglePay ? "GooglePay" :
                        c.MetodoDePago is PayPal ? "PayPal" :
                        "Desconocido",
                    c.CodigoDescuento,
                    c.ComprarItems
                        .Select(item => new CompraItemDTO(
                            item.LibroId,
                            item.Libro.Titulo,
                            item.Libro.Genero.Nombre,
                            item.Libro.Editorial.Nombre,
                            item.PrecioCompra,
                            item.Cantidad))
                        .ToList()))
                .FirstOrDefaultAsync();

            if (compra == null)
            {
                _logger.LogError(
                    $"Error: no existe una compra con id {id}.");

                return NotFound();
            }

            return Ok(compra);
        }
    }
}