using CapaEntidades;
using CapaNegocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace InmoGestor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PagoController : ControllerBase
    {
        private readonly CN_Pago _cnPago;

        public PagoController(CN_Pago cnPago)
        {
            _cnPago = cnPago;
        }

        [HttpPost]
        public IActionResult Registrar([FromBody] RegistrarPagoRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            bool esSuperior = User.IsInRole("SUPERIOR"); //aca esta verificando el rol del usuario para saber si es superior o no

            var pago = new Pago
            {
                IdPago = Guid.NewGuid(),
                IdCuota = Guid.Parse(request.IdCuota),
                IdUsuarioCreador = userId,
                MontoTotal = request.Monto
            };

            var (success, message) = _cnPago.RegistrarPago(pago, esSuperior);

            return success ? Ok(new { success, message }) : BadRequest(new { success, message });
        }

        [HttpPost("{id}/aprobar")]
        [Authorize(Roles = "SUPERIOR")]
        public IActionResult Aprobar(string id)
        {
            
            if (!Guid.TryParse(id, out var guidId))
            {
                return BadRequest(new { success = false, mensaje = "ID inválido" });
            }

           
            var pago = _cnPago.ObtenerPorId(guidId);

            if (pago == null)
            {
                return NotFound(new { success = false, mensaje = "Pago no encontrado" });
            }

            
            var result = _cnPago.AprobarPago(pago.IdPago, pago.IdCuota);

            return Ok(new { success = result, mensaje = result ? "Pago aprobado correctamente" : "Error al aprobar" });
        }

        [HttpGet]
        public IActionResult Listar([FromQuery] int? estado)
        {
            try
            {
                Console.WriteLine("=== INICIO LISTAR PAGOS ===");
                var pagos = _cnPago.Listar(estado);
                Console.WriteLine($"=== PAGOS ENCONTRADOS: {pagos?.Count ?? 0} ===");
                return Ok(new { success = true, data = pagos });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR: {ex.Message} ===");
                Console.WriteLine($"=== STACK: {ex.StackTrace} ===");
                if (ex.InnerException != null)
                    Console.WriteLine($"=== INNER: {ex.InnerException.Message} ===");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("{id}")]
        public IActionResult ObtenerPorId(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out var guidId))
                    return BadRequest(new { success = false, message = "ID inválido" });

                var pago = _cnPago.ObtenerPorId(guidId);
                if (pago == null)
                    return NotFound(new { success = false, message = "Pago no encontrado" });

                return Ok(new { success = true, data = pago });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("contrato/{contratoId}")]
        public IActionResult ObtenerPorContrato(string contratoId)
        {
            try
            {
                if (!Guid.TryParse(contratoId, out var guidContratoId))
                    return BadRequest(new { success = false, message = "ID de contrato inválido" });

                var pagos = _cnPago.ListarPorContrato(guidContratoId);
                return Ok(new { success = true, data = pagos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPatch("{id}/confirmar")]
        [Authorize(Roles = "SUPERIOR")]
        public IActionResult Confirmar(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out var guidId))
                    return BadRequest(new { success = false, message = "ID inválido" });

                var pago = _cnPago.ObtenerPorId(guidId);
                if (pago == null)
                    return NotFound(new { success = false, message = "Pago no encontrado" });

                var resultado = _cnPago.AprobarPago(pago.IdPago, pago.IdCuota);
                if (!resultado)
                    return BadRequest(new { success = false, message = "No se pudo confirmar el pago" });

                return Ok(new { success = true, message = "Pago confirmado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPatch("{id}/rechazar")]
        [Authorize(Roles = "SUPERIOR")]
        public IActionResult Rechazar(string id, [FromBody] MotivoRechazoDto dto)
        {
            try
            {
                if (!Guid.TryParse(id, out var guidId))
                    return BadRequest(new { success = false, message = "ID inválido" });

                var resultado = _cnPago.Rechazar(guidId, dto?.Motivo);
                if (!resultado)
                    return BadRequest(new { success = false, message = "No se pudo rechazar el pago" });

                return Ok(new { success = true, message = "Pago rechazado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPatch("{id}/anular")]
        [Authorize(Roles = "SUPERIOR")]
        public IActionResult Anular(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out var guidId))
                    return BadRequest(new { success = false, message = "ID inválido" });

                var resultado = _cnPago.Anular(guidId);
                if (!resultado)
                    return BadRequest(new { success = false, message = "No se pudo anular el pago" });

                return Ok(new { success = true, message = "Pago anulado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public class RegistrarPagoRequest
        {
        public string IdCuota { get; set; } = "";
        public string IdMetodoPago { get; set; } = "";
        public decimal Monto { get; set; }
        public string Periodo { get; set; } = "";
        }
        public class MotivoRechazoDto
        {
            public string? Motivo { get; set; }
        }
    }
}