#nullable enable
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CapaNegocio;
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

        [HttpGet("activos-por-inquilino")]
        public IActionResult ListarActivosPorInquilino([FromQuery] Guid idInquilino)
        {
            var contratos = _cnContrato.ListarActivosPorInquilino(idInquilino);
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
        public IActionResult CrearContrato([FromBody] CrearContratoRequest contrato)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, mensaje = "Token inválido" });

            if (contrato.InmuebleId == Guid.Empty)
                return BadRequest(new { success = false, mensaje = "El inmueble es requerido" });

            if (string.IsNullOrWhiteSpace(contrato.DniInquilino))
                return BadRequest(new { success = false, mensaje = "El DNI del inquilino es requerido" });

            var inquilino = _cnInquilino.ObtenerPorDni(contrato.DniInquilino!);
            if (inquilino == null)
                return BadRequest(new { success = false, mensaje = "No se encontró un inquilino con ese DNI" });

            if (contrato.CantidadCuotas <= 0)
                return BadRequest(new { success = false, mensaje = "La cantidad de cuotas debe ser mayor a 0" });

            if (contrato.PrecioCuota <= 0)
                return BadRequest(new { success = false, mensaje = "El precio de la cuota debe ser mayor a 0" });

            var (validSuccess, validMessage) = _cnContrato.ValidarInmuebleDisponible(contrato.InmuebleId);
            if (!validSuccess)
                return BadRequest(new { success = false, mensaje = validMessage });

            var nuevoContrato = ContratoMapper.ToEntity(contrato, inquilino, userId);

            var (success, message, contratoId) = _cnContrato.CrearContrato(nuevoContrato);

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
