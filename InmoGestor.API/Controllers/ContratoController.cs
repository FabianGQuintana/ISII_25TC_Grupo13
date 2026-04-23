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
        [Authorize]
        public IActionResult Crear([FromBody] CrearContratoRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { success = false, mensaje = "Token inválido" });
            }

            if (string.IsNullOrWhiteSpace(request.InmuebleId) || !Guid.TryParse(request.InmuebleId, out var inmuebleId))
            {
                return BadRequest(new { success = false, mensaje = "El inmueble es requerido" });
            }

            if (string.IsNullOrWhiteSpace(request.DniInquilino))
            {
                return BadRequest(new { success = false, mensaje = "El DNI del inquilino es requerido" });
            }

            var inquilino = _cnInquilino.ObtenerPorDni(request.DniInquilino!);
            if (inquilino == null)
            {
                return BadRequest(new { success = false, mensaje = "No se encontró un inquilino con ese DNI" });
            }

            if (request.CantidadCuotas <= 0)
            {
                return BadRequest(new { success = false, mensaje = "La cantidad de cuotas debe ser mayor a 0" });
            }

            if (request.PrecioCuota <= 0)
            {
                return BadRequest(new { success = false, mensaje = "El precio de la cuota debe ser mayor a 0" });
            }

            var (validSuccess, validMessage) = _cnContrato.ValidarInmuebleDisponible(inmuebleId);
            if (!validSuccess)
            {
                return BadRequest(new { success = false, mensaje = validMessage });
            }

            Guid rolInquilinoId = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(request.RolInquilinoId))
            {
                Guid.TryParse(request.RolInquilinoId, out rolInquilinoId);
            }

            DateTime fechaInicioParseada = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(request.FechaInicio))
            {
                if (!DateTime.TryParse(request.FechaInicio, out var tempInicio))
                {
                    return BadRequest(new { success = false, mensaje = "El formato de Fecha Inicio es inválido" });
                }
                fechaInicioParseada = tempInicio.Date;
            }

            DateTime fechaFinParseada;
            if (string.IsNullOrWhiteSpace(request.FechaFin))
            {
                fechaFinParseada = fechaInicioParseada.AddMonths(request.CantidadCuotas);
            }
            else if (!DateTime.TryParse(request.FechaFin, out fechaFinParseada))
            {
                return BadRequest(new { success = false, mensaje = "El formato de Fecha Fin es inválido" });
            }
            else
            {
                fechaFinParseada = fechaFinParseada.Date;
            }

            Guid? idTipoIndice = null;
            if (!string.IsNullOrWhiteSpace(request.IdTipoIndice) && Guid.TryParse(request.IdTipoIndice, out var tempIdIndice))
            {
                idTipoIndice = tempIdIndice;
            }

            var contrato = new ContratoAlquiler
            {
                FechaInicio = fechaInicioParseada,
                FechaFin = fechaFinParseada,
                CantidadCuotas = request.CantidadCuotas,
                PrecioCuota = request.PrecioCuota,
                TasaMoraMensual = request.TasaMoraMensual,
                Condiciones = request.Condiciones,
                IdInmueble = inmuebleId,
                IdPersonaInquilino = inquilino.IdPersona,
                IdRolClienteInquilino = rolInquilinoId,
                IdUsuarioCreador = userId,
                FrecuenciaAjuste = request.FrecuenciaAjuste,
                IdTipoIndice = idTipoIndice,
                ValorIndiceInicio = request.ValorIndiceInicio
            };

            var (success, message, contratoId) = _cnContrato.CrearContrato(contrato);

            if (!success)
            {
                return BadRequest(new { success = false, mensaje = message });
            }

            return Ok(new { success = true, mensaje = message, data = new { contratoId } });
        }

        [HttpPut("rescindir/{id}")]
        [Authorize(Roles = "Superior")]
        public IActionResult Rescindir(string idContrato)
        {
            if (!Guid.TryParse(idContrato, out var contratoId))
            {
                return BadRequest(new { success = false, mensaje = "ID inválido" });
            }

            var (success, message) = _cnContrato.Rescindir(contratoId);

            if (!success)
            {
                return BadRequest(new { success = false, mensaje = message });
            }

            return Ok(new { success = true, mensaje = message });
        }
    }
}
