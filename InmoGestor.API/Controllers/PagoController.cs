#nullable enable
using CapaEntidades;
using CapaNegocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using InmoGestor.API.DTOs;
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

        [HttpGet("calcular/{idContrato}")]
        public async Task<IActionResult> CalcularCuota(string idContrato)
        {
            if (!Guid.TryParse(idContrato, out var guidIdContrato))
                return BadRequest(new { success = false, message = "ID de contrato inválido" });

            var (detalle, metodos) = await _cnPago.MostrarDetallePago(guidIdContrato);

            if (detalle == null)
                return NotFound(new { success = false, message = "No se encontró la cuota pendiente" });

            var response = new DetallePagoResponse
            {
                Cuota = new DTOs.CuotaCalculadaDto
                {
                    IdCuota = detalle.IdCuota,
                    NroCuota = detalle.NroCuota,
                    Periodo = detalle.Periodo,
                    FechaVencimiento = detalle.FechaVencimiento,
                    PrecioCuota = detalle.PrecioCuota,
                    ValorIndiceAplicado = detalle.ValorIndiceAplicado,
                    ImporteActualizado = detalle.ImporteActualizado,
                    TotalAdicionales = detalle.TotalAdicionales,
                    TotalDescuentos = detalle.TotalDescuentos,
                    DiasAtraso = detalle.DiasAtraso,
                    MoraCalculada = detalle.MoraCalculada,
                    TotalFinal = detalle.TotalFinal,
                    Estado = detalle.Estado
                },
                MetodosPago = metodos.Select(m => new MetodoPagoDto
                {
                    Id = m.IdMetodoPago.ToString(),
                    Nombre = m.Nombre,
                    Descripcion = m.Descripcion
                }).ToList()
            };

            return Ok(new { success = true, data = response });
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarPago([FromBody] RegistrarPagoRequest pago)
        {
            if (pago == null)
                return BadRequest(new { success = false, message = "El cuerpo de la solicitud no puede estar vacío" });

            if (!Guid.TryParse(pago.IdCuota, out var idCuota))
                return BadRequest(new { success = false, message = "ID de cuota inválido" });

            if (!Guid.TryParse(pago.IdMetodoPago, out var idMetodoPago))
                return BadRequest(new { success = false, message = "ID de método de pago inválido" });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "Usuario no autenticado" });

            var resultado = await _cnPago.RegistrarPago(
                idCuota,
                idMetodoPago,
                userId);

            return resultado.success
                ? Ok(new
                {
                    success = true,
                    message = resultado.message,
                    data = new { pagoId = resultado.pagoId.ToString() }
                })
                : BadRequest(new
                {
                    success = false,
                    message = resultado.message
                });
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

        [HttpGet]
        public IActionResult Listar([FromQuery] int? estado)
        {
            var pagos = _cnPago.Listar(estado);
            var response = PagoMapper.ToResponseList(pagos);
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

            return Ok(new { success = true, data = PagoMapper.ToResponse(pago) });
        }

        [HttpGet("contrato/{contratoId}")]
        public IActionResult ObtenerPorContrato(string contratoId)
        {
            if (!Guid.TryParse(contratoId, out var guidContratoId))
                return BadRequest(new { success = false, message = "ID de contrato inválido" });

            var pagos = _cnPago.ListarPorContrato(guidContratoId);
            var response = PagoMapper.ToResponseList(pagos);
            return Ok(new { success = true, data = response });
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

        [HttpPost("{id}/anular")]
        [Authorize(Roles = "Superior,SUPERIOR")]
        public IActionResult AnularPago(string id, [FromBody] AnularPagoRequestDto dto)
        {
            if (!Guid.TryParse(id, out var guidId))
                return BadRequest(new ResultadoDto { Success = false, Message = "ID inválido" });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new ResultadoDto { Success = false, Message = "Usuario no autenticado" });

            var (success, message) = _cnPago.ProcesarAnulacionPago(guidId, dto?.Motivo ?? "", userId);

            return success
                ? Ok(new ResultadoDto { Success = success, Message = message })
                : BadRequest(new ResultadoDto { Success = success, Message = message });
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
