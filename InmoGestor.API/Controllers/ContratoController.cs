using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CapaNegocio;

namespace InmoGestor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContratoController : ControllerBase
    {
        private readonly CN_Contrato _cnContrato;

        public ContratoController()
        {
            _cnContrato = new CN_Contrato();
        }

        [HttpPost]
        public IActionResult CrearContrato([FromBody] CrearContratoRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var idUsuario))
            {
                return Unauthorized(new { success = false, mensaje = "Token inválido" });
            }

            var (success, contrato, mensaje) = _cnContrato.CrearContrato(
                request.IdInmueble,
                request.IdInquilino,
                idUsuario,
                request.IdTipoIndice,
                request.FechaInicio,
                request.FechaFin,
                request.CantidadCuotas,
                request.PrecioCuota,
                request.FrecuenciaAjuste,
                request.TasaMoraMensual
            );

            if (!success || contrato == null)
            {
                return BadRequest(new { success = false, mensaje });
            }

            return CreatedAtAction(nameof(ObtenerPorId), new { id = contrato.IdContrato }, new
            {
                success = true,
                mensaje,
                data = new
                {
                    contrato.IdContrato,
                    contrato.FechaInicio,
                    contrato.FechaFin,
                    contrato.CantidadCuotas,
                    contrato.PrecioCuota,
                    contrato.MontoTotal,
                    contrato.Estado
                }
            });
        }

        [HttpGet("{id}")]
        public IActionResult ObtenerPorId(Guid id)
        {
            var contrato = _cnContrato.ObtenerPorId(id);

            if (contrato == null)
            {
                return NotFound(new { success = false, mensaje = "Contrato no encontrado" });
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    contrato.IdContrato,
                    contrato.FechaInicio,
                    contrato.FechaFin,
                    contrato.CantidadCuotas,
                    contrato.PrecioCuota,
                    contrato.FrecuenciaAjuste,
                    contrato.TasaMoraMensual,
                    contrato.Estado,
                    contrato.MontoTotal,
                    Inmueble = new
                    {
                        contrato.OInmueble?.IdInmueble,
                        contrato.OInmueble?.Direccion
                    },
                    Inquilino = new
                    {
                        contrato.OInquilino?.IdPersona,
                        contrato.OInquilino?.NombreCompleto
                    },
                    TipoIndice = new
                    {
                        contrato.OTipoIndice?.IdTipoIndice,
                        contrato.OTipoIndice?.Nombre
                    }
                }
            });
        }
    }

    public class CrearContratoRequest
    {
        public Guid IdInmueble { get; set; }
        public Guid IdInquilino { get; set; }
        public Guid IdTipoIndice { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int CantidadCuotas { get; set; }
        public decimal PrecioCuota { get; set; }
        public string FrecuenciaAjuste { get; set; } = string.Empty;
        public decimal TasaMoraMensual { get; set; }
    }
}