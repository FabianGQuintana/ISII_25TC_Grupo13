#nullable enable
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CapaNegocio;
using CapaEntidades;
using InmoGestor.API.DTOs;
using InmoGestor.API.Mappers;

namespace InmoGestor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContratoController : ControllerBase
    {
        private readonly CN_Contrato _cnContrato;
        private readonly CN_Inquilino _cnInquilino;

        public ContratoController(CN_Contrato cnContrato, CN_Inquilino cnInquilino)
        {
            _cnContrato = cnContrato;
            _cnInquilino = cnInquilino;
        }

        [HttpGet]
        public IActionResult Listar([FromQuery] int? estado)
        {
            var contratos = _cnContrato.Listar(estado);
            var response = ContratoMapper.ToResponseList(contratos);
            return Ok(new { success = true, data = response });
        }



        [HttpGet("{id}")]
        public IActionResult ObtenerPorId(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
            {
                return BadRequest(new { success = false, mensaje = "ID inválido" });
            }

            var contrato = _cnContrato.ObtenerPorId(guidId);
            if (contrato == null)
            {
                return NotFound(new { success = false, mensaje = "Contrato no encontrado" });
            }

            var response = ContratoMapper.ToResponse(contrato);
            return Ok(new { success = true, data = response });
        }

        [HttpPost]
        public IActionResult CrearContrato([FromBody] CrearContratoRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, mensaje = "Token inválido" });

            if (string.IsNullOrWhiteSpace(request.DniInquilino))
                return BadRequest(new { success = false, mensaje = "El DNI del inquilino es requerido" });

            var inquilino = _cnInquilino.ObtenerPorDni(request.DniInquilino!);
            if (inquilino == null)
                return BadRequest(new { success = false, mensaje = "No se encontró un inquilino con ese DNI" });

            var fechaInicio = request.FechaInicio?.Date ?? DateTime.Today;
            var fechaFin = request.FechaFin?.Date ?? fechaInicio.AddMonths(request.CantidadCuotas);

            var contrato = new ContratoAlquiler
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                CantidadCuotas = request.CantidadCuotas,
                PrecioCuota = request.PrecioCuota,
                TasaMoraMensual = request.TasaMoraMensual,
                Condiciones = request.Condiciones,
                IdInmueble = request.InmuebleId,
                IdPersonaInquilino = inquilino.IdPersona,
                IdRolClienteInquilino = request.RolInquilinoId ?? Guid.Empty,
                IdUsuarioCreador = userId,
                FrecuenciaAjuste = request.FrecuenciaAjuste,
                IdTipoIndice = request.IdTipoIndice,
                ValorIndiceInicio = request.ValorIndiceInicio
            };

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            if (!success)
                return BadRequest(new { success = false, mensaje = message });

            return Ok(new { success = true, mensaje = message, data = new { contratoId } });
        }

        [HttpPut("rescindir/{idContrato}")]
        [Authorize(Roles = "Superior")]
        public IActionResult Rescindir(Guid idContrato)
        {
            var (success, message) = _cnContrato.Rescindir(idContrato);

            if (!success)
            {
                return BadRequest(new { success = false, mensaje = message });
            }

            return Ok(new { success = true, mensaje = message });
        }
    }
}
