#nullable enable
using System;
using System.Linq;
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
        private readonly CN_Contrato _cnContrato;

        public CuotaController(CN_Cuota cnCuota, CN_Contrato cnContrato)
        {
            _cnCuota = cnCuota;
            _cnContrato = cnContrato;
        }

        [HttpGet("pendientes/{contratoId}")]
        public IActionResult Pendientes(string contratoId)
        {
            if (!Guid.TryParse(contratoId, out var id))
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "ID inválido"
                });
            }

            var contrato = _cnContrato.ObtenerPorId(id);

            if (contrato == null)
            {
                return NotFound(new
                {
                    success = false,
                    mensaje = "Contrato no encontrado"
                });
            }

            var cuotas =
                _cnCuota.ObtenerCuotasCalculadas(id, contrato);

            return Ok(new
            {
                success = true,
                data = cuotas
            });
        }
    }
}
