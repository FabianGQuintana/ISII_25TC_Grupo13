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
                return BadRequest(new { success = false, mensaje = "ID inválido" });

            var contrato = _cnContrato.ObtenerPorId(id);
            if (contrato == null)
                return NotFound(new { success = false, mensaje = "Contrato no encontrado" });

            var cuotas = _cnCuota.ListarPendientesPorContrato(id);

            var result = cuotas.Select(c => {
                var diasAtraso = c.FechaVencimiento < DateTime.Today ? (DateTime.Today - c.FechaVencimiento).Days : 0;
                var moraCalculada = diasAtraso > 0 ? (contrato.MoraDiariaMonto * diasAtraso) : 0;
                var importeActualizado = c.ImporteBase * (c.ValorIndiceAplicado > 0 ? c.ValorIndiceAplicado : 1m);
                var totalFinal = importeActualizado + moraCalculada;

                return new CuotaPendienteDto
                {
                    IdCuota = c.IdCuota.ToString(),
                    NroCuota = c.NroCuota,
                    Periodo = c.Periodo,
                    FechaVencimiento = c.FechaVencimiento,
                    ImporteBase = c.ImporteBase,
                    ValorIndiceAplicado = c.ValorIndiceAplicado > 0 ? c.ValorIndiceAplicado : 1m,
                    ImporteActualizado = importeActualizado,
                    DiasAtraso = diasAtraso,
                    MoraCalculada = moraCalculada,
                    TotalFinal = totalFinal,
                    Estado = diasAtraso > 0 ? "Vencida" : c.Estado
                };
            }).ToList();

            return Ok(new { success = true, data = result });
        }
    }
}
