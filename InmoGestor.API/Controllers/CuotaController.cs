#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CapaNegocio;
using CapaDatos;
using InmoGestor.API.DTOs;

namespace InmoGestor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CuotaController : ControllerBase
    {
        private readonly CN_Cuota _cnCuota;

        public CuotaController(CN_Cuota cnCuota)
        {
            _cnCuota = cnCuota;
        }

        [HttpGet("por-contrato/{contratoId}")]
        public async Task<IActionResult> ObtenerPorContrato(string contratoId)
        {
            if (!Guid.TryParse(contratoId, out var id))
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "ID inválido"
                });
            }

            var calculada = await _cnCuota.ObtenerCuotaCalculada(id);

            if (calculada == null)
            {
                return NotFound(new
                {
                    success = false,
                    mensaje = "No hay cuotas pendientes para este contrato"
                });
            }

            var response = new CuotaPendienteDto
            {
                IdCuota = calculada.IdCuota,
                NroCuota = calculada.NroCuota,
                Periodo = calculada.Periodo,
                PrecioCuota = calculada.PrecioCuota,
                FechaVencimiento = calculada.FechaVencimiento,
                ValorIndiceAplicado = calculada.ValorIndiceAplicado,
                ImporteActualizado = calculada.ImporteActualizado,
                DiasAtraso = calculada.DiasAtraso,
                MoraCalculada = calculada.MoraCalculada,
                TotalFinal = calculada.TotalFinal,
                Estado = calculada.Estado
            };

            return Ok(new { success = true, data = response });
        }

        [HttpGet("calcular/{idContrato}")]
        public async Task<IActionResult> Calcular(string idContrato)
        {
            if (!Guid.TryParse(idContrato, out var id))
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "ID inválido"
                });
            }

            var cuota = await _cnCuota.ObtenerCuotaCalculada(id);

            if (cuota == null)
            {
                return NotFound(new
                {
                    success = false,
                    mensaje = "No se pudo calcular la cuota"
                });
            }

            return Ok(new
            {
                success = true,
                data = cuota
            });
        }

        [HttpPatch("{idCuota}/descuento")]
        public IActionResult ActualizarDescuento(string idCuota, [FromBody] ActualizarDescuentoRequest request)
        {
            if (!Guid.TryParse(idCuota, out var guidId))
            {
                return BadRequest(new { success = false, mensaje = "ID de cuota inválido" });
            }

            var cdCuota = new CD_Cuota();
            var cuota = cdCuota.ObtenerPorId(guidId);
            if (cuota == null)
            {
                return NotFound(new { success = false, mensaje = "Cuota no encontrada" });
            }

            var success = cdCuota.ActualizarDescuento(guidId, request.Descuento);
            if (!success)
            {
                return BadRequest(new { success = false, mensaje = "No se pudo actualizar el descuento" });
            }

            return Ok(new { success = true, mensaje = "Descuento actualizado correctamente" });
        }
    }

    public class ActualizarDescuentoRequest
    {
        public decimal Descuento { get; set; }
    }
}
