#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CapaNegocio;
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
        public IActionResult ObtenerPorContrato(string contratoId)
        {
            if (!Guid.TryParse(contratoId, out var id))
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "ID inválido"
                });
            }

            var cuota = _cnCuota.ObtenerCuotaPorContrato(id);

            if (cuota == null)
            {
                return NotFound(new
                {
                    success = false,
                    mensaje = "No hay cuotas pendientes para este contrato"
                });
            }

            var response = new CuotaPendienteDto
            {
                IdCuota = cuota.IdCuota.ToString(),
                NroCuota = cuota.NroCuota,
                Periodo = cuota.Periodo,
                FechaVencimiento = cuota.FechaVencimiento,
                Estado = cuota.Estado
            };

            return Ok(new { success = true, data = response });
        }

        [HttpGet("calcular/{idContrato}")]
        public IActionResult Calcular(string idContrato)
        {
            if (!Guid.TryParse(idContrato, out var id))
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "ID inválido"
                });
            }

            var cuota = _cnCuota.ObtenerCuotaCalculada(id);

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
    }
}
