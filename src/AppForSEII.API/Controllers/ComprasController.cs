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

        [HttpPost]
[Route("[action]")]
[ProducesResponseType(
    typeof(ComprasDetailDTO),
    (int)HttpStatusCode.Created)]
[ProducesResponseType(
    typeof(ValidationProblemDetails),
    (int)HttpStatusCode.BadRequest)]
[ProducesResponseType(
    typeof(string),
    (int)HttpStatusCode.Conflict)]
public async Task<ActionResult> CreateCompra(
    CompraForCreateDTO compraForCreate)
{
    // Comprobar que hay libros seleccionados.
    if (compraForCreate.CompraItems == null ||
        compraForCreate.CompraItems.Count == 0)
    {
        ModelState.AddModelError(
            "CompraItems",
            "Debes seleccionar al menos un libro.");
    }

    // Buscar el usuario, igual que en CreateRental.
    var usuario = await _context.ApplicationUsers
        .FirstOrDefaultAsync(u =>
            u.UserName == compraForCreate.CustomerUserName);

    if (usuario == null)
    {
        ModelState.AddModelError(
            "CustomerUserName",
            "El usuario no está registrado.");
    }

    // Validar los datos específicos del método de pago.
    MetodosdePago? metodoPago = null;

    switch (compraForCreate.MetodoDePago)
    {
        case "Visa":
            if (string.IsNullOrWhiteSpace(compraForCreate.NumeroTarjeta))
            {
                ModelState.AddModelError(
                    "NumeroTarjeta",
                    "Debes introducir el número de tarjeta.");
            }

            if (!compraForCreate.FechaCaducidad.HasValue ||
                compraForCreate.FechaCaducidad.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "FechaCaducidad",
                    "Debes introducir una fecha de caducidad válida.");
            }

            metodoPago = new Visa
            {
                NumeroTarjeta = compraForCreate.NumeroTarjeta ?? "",
                FechaCaducidad =
                    compraForCreate.FechaCaducidad ?? DateTime.Today
            };
            break;

        case "GooglePay":
            if (string.IsNullOrWhiteSpace(compraForCreate.EmailGooglePay))
            {
                ModelState.AddModelError(
                    "EmailGooglePay",
                    "Debes introducir el correo de GooglePay.");
            }

            metodoPago = new GooglePay
            {
                Email = compraForCreate.EmailGooglePay ?? ""
            };
            break;

        case "PayPal":
            if (string.IsNullOrWhiteSpace(compraForCreate.TelefonoPayPal))
            {
                ModelState.AddModelError(
                    "TelefonoPayPal",
                    "Debes introducir el teléfono de PayPal.");
            }

            metodoPago = new PayPal
            {
                NumeroTelefono = compraForCreate.TelefonoPayPal ?? ""
            };
            break;

        default:
            ModelState.AddModelError(
                "MetodoDePago",
                "El método de pago debe ser Visa, GooglePay o PayPal.");
            break;
    }

    if (!ModelState.IsValid)
    {
        return BadRequest(new ValidationProblemDetails(ModelState));
    }

    // Un libro solo puede aparecer una vez en la misma compra.
    var libroIds = compraForCreate.CompraItems!
        .Select(item => item.LibroId)
        .ToList();

    if (libroIds.Distinct().Count() != libroIds.Count)
    {
        ModelState.AddModelError(
            "CompraItems",
            "Hay libros repetidos. Indica la cantidad en un único artículo.");

        return BadRequest(new ValidationProblemDetails(ModelState));
    }

    // Recuperar los libros y sus precios reales.
    var libros = await _context.Libros
        .Include(l => l.Genero)
        .Include(l => l.Editorial)
        .Where(l => libroIds.Contains(l.Id))
        .ToListAsync();

    foreach (var item in compraForCreate.CompraItems!)
    {
        var libro = libros.FirstOrDefault(l => l.Id == item.LibroId);

        if (libro == null)
        {
            ModelState.AddModelError(
                "CompraItems",
                $"No existe el libro con identificador {item.LibroId}.");
        }
        else if (item.Cantidad < 2)
        {
            ModelState.AddModelError(
                "CompraItems",
                $"La cantidad de '{libro.Titulo}' debe ser mayor que 1.");
        }
        else if (item.Cantidad > libro.Stock)
        {
            ModelState.AddModelError(
                "CompraItems",
                $"No hay stock suficiente de '{libro.Titulo}'. " +
                $"Unidades disponibles: {libro.Stock}.");
        }
    }

    if (!ModelState.IsValid)
    {
        return BadRequest(new ValidationProblemDetails(ModelState));
    }

    // Crear la compra con una lista inicialmente vacía.
    var compra = new Compra(
        DateTime.Now,
        compraForCreate.CodigoDescuento,
        metodoPago!,
        new List<ComprarItem>())
    {
        ApplicationUser = usuario!,
        Nombre = compraForCreate.NombreComprador,
        Apellidos = compraForCreate.ApellidoComprador,
        Direccion = compraForCreate.Direccion,
        Telefono = compraForCreate.TelefonoComprador
    };

    // Crear los artículos y actualizar el stock.
    foreach (var item in compraForCreate.CompraItems!)
    {
        var libro = libros.First(l => l.Id == item.LibroId);

        compra.ComprarItems.Add(
            new ComprarItem(libro, item.Cantidad, compra));

        libro.Stock -= item.Cantidad;
    }

    compra.PrecioTotal = compra.ComprarItems.Sum(
        item => item.PrecioCompra * item.Cantidad);

    // Preparar el detalle utilizando los datos de la base de datos.
    var detalleItems = compra.ComprarItems
        .Select(item => new CompraItemDTO(
            item.LibroId,
            item.Libro.Titulo,
            item.Libro.Genero.Nombre,
            item.Libro.Editorial.Nombre,
            item.PrecioCompra,
            item.Cantidad))
        .ToList();

    _context.Compras.Add(compra);

    try
    {
        // Guarda la compra, los artículos, el pago y el stock.
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Error al guardar la compra.");

        return Conflict(
            "No se pudo guardar la compra. Inténtalo de nuevo.");
    }

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

    return CreatedAtAction(
        nameof(GetDetailsForCompras),
        new { id = compra.Id },
        compraDetail);
}
    }
}