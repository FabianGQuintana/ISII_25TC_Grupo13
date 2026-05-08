#nullable enable
using CapaEntidades;
using CapaNegocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using InmoGestor.API.Mappers;

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
            bool esSuperior = User.IsInRole("SUPERIOR");

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
                return BadRequest(new { success = false, mensaje = "ID inválido" });

            var pago = _cnPago.ObtenerPorId(guidId);
            if (pago == null)
                return NotFound(new { success = false, mensaje = "Pago no encontrado" });

            var result = _cnPago.AprobarPago(pago.IdPago, pago.IdCuota);

            return Ok(new { success = result, mensaje = result ? "Pago aprobado correctamente" : "Error al aprobar" });
        }


        [HttpGet("activos-por-inquilino")]
        public IActionResult ListarActivosPorInquilino([FromQuery] Guid idInquilino)
        {
            var contratos = _cnPago.ListarActivosPorInquilino(idInquilino);
            var response = ContratoMapper.ToResponseList(contratos);
            return Ok(new { success = true, data = response });
        }

        [HttpGet]
        public IActionResult Listar([FromQuery] int? estado)
        {
            var pagos = _cnPago.Listar(estado);
            return Ok(new { success = true, data = pagos });
        }

        [HttpGet("{id}")]
        public IActionResult ObtenerPorId(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
                return BadRequest(new { success = false, message = "ID inválido" });

            var pago = _cnPago.ObtenerPorId(guidId);
            if (pago == null)
                return NotFound(new { success = false, message = "Pago no encontrado" });

            return Ok(new { success = true, data = pago });
        }

        [HttpGet("contrato/{contratoId}")]
        public IActionResult ObtenerPorContrato(string contratoId)
        {
            if (!Guid.TryParse(contratoId, out var guidContratoId))
                return BadRequest(new { success = false, message = "ID de contrato inválido" });

            var pagos = _cnPago.ListarPorContrato(guidContratoId);
            return Ok(new { success = true, data = pagos });
        }

        [HttpPatch("{id}/confirmar")]
        [Authorize(Roles = "SUPERIOR")]
        public IActionResult Confirmar(string id)
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

        [HttpPatch("{id}/rechazar")]
        [Authorize(Roles = "SUPERIOR")]
        public IActionResult Rechazar(string id, [FromBody] MotivoRechazoDto dto)
        {
            if (!Guid.TryParse(id, out var guidId))
                return BadRequest(new { success = false, message = "ID inválido" });

            var resultado = _cnPago.Rechazar(guidId, dto?.Motivo);
            if (!resultado)
                return BadRequest(new { success = false, message = "No se pudo rechazar el pago" });

            return Ok(new { success = true, message = "Pago rechazado exitosamente" });
        }

        [HttpPatch("{id}/anular")]
        [Authorize(Roles = "SUPERIOR")]
        public IActionResult Anular(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
                return BadRequest(new { success = false, message = "ID inválido" });

            var resultado = _cnPago.Anular(guidId);
            if (!resultado)
                return BadRequest(new { success = false, message = "No se pudo anular el pago" });

            return Ok(new { success = true, message = "Pago anulado exitosamente" });
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
