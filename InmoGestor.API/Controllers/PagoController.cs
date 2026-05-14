#nullable enable
using CapaEntidades;
using CapaNegocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using InmoGestor.API.Mappers;
using InmoGestor.API.DTOs;
using System.Linq;

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
            bool esSuperior = User.IsInRole("Superior") || User.IsInRole("SUPERIOR");

            var pago = new Pago
            {
                IdPago = Guid.NewGuid(),
                IdCuota = Guid.Parse(request.IdCuota),
                IdUsuarioCreador = userId,
                MontoTotal = request.Monto,
                IdMetodoPago = Guid.Parse(request.IdMetodoPago)
            };

            var (success, message) = _cnPago.RegistrarPago(pago, esSuperior);

            return success ? Ok(new { success, message }) : BadRequest(new { success, message });
        }

        [HttpPost("{id}/aprobar")]
        [Authorize(Roles = "Superior,SUPERIOR")]
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
        public IActionResult Listar([FromQuery] string? estado)
        {
            var pagos = _cnPago.Listar(estado);
            var response = pagos.Select(MapToDto).ToList();
            return Ok(new { success = true, data = response });
        }

        [HttpGet("{id}")]
        public IActionResult ObtenerPorId(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
                return BadRequest(new { success = false, message = "ID inválido" });

            var pago = _cnPago.ObtenerPorId(guidId);
            if (pago == null)
                return NotFound(new { success = false, message = "Pago no encontrado" });

            return Ok(new { success = true, data = MapToDto(pago) });
        }

        [HttpGet("contrato/{contratoId}")]
        public IActionResult ObtenerPorContrato(string contratoId)
        {
            if (!Guid.TryParse(contratoId, out var guidContratoId))
                return BadRequest(new { success = false, message = "ID de contrato inválido" });

            var pagos = _cnPago.ListarPorContrato(guidContratoId);
            var response = pagos.Select(MapToDto).ToList();
            return Ok(new { success = true, data = response });
        }

        private PagoResponseDto MapToDto(Pago p)
        {
            return new PagoResponseDto
            {
                Id = p.IdPago.ToString(),
                ContratoId = p.IdContrato.ToString(),
                CuotaId = p.IdCuota.ToString(),
                Inquilino = p.Inquilino,
                Inmueble = p.Inmueble,
                NroCuota = p.NroCuota,
                Periodo = p.Periodo,
                FechaVencimiento = p.FechaVencimiento,
                FechaPago = p.FechaPago == DateTime.MinValue ? null : p.FechaPago,
                Monto = p.MontoTotal,
                Mora = p.MoraCobrada,
                DiasAtraso = p.FechaVencimiento.HasValue ? Math.Max(0, (DateTime.Now - p.FechaVencimiento.Value).Days) : 0,
                TotalPagado = p.MontoTotal,
                Estado = p.Estado,
                EstadoTexto = p.Estado
            };
        }

        [HttpPatch("{id}/confirmar")]
        [Authorize(Roles = "Superior,SUPERIOR")]
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
        [Authorize(Roles = "Superior,SUPERIOR")]
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
        [Authorize(Roles = "Superior,SUPERIOR")]
        public IActionResult Anular(string id, [FromBody] AnularPagoDto dto)
        {
            if (!Guid.TryParse(id, out var guidId))
                return BadRequest(new { success = false, message = "ID inválido" });

            var pago = _cnPago.ObtenerPorId(guidId);
            if (pago == null)
                return NotFound(new { success = false, message = "Pago no encontrado" });

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            var resultado = _cnPago.Anular(pago, userId, dto?.Motivo);
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

        public class AnularPagoDto
        {
            public string? Motivo { get; set; }
        }
    }
}
