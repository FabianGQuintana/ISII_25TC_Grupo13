#nullable enable
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CapaNegocio;
using CapaEntidades;

namespace InmoGestor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContratoController : ControllerBase
    {
        private readonly CN_Contrato _cnContrato;
        private readonly CN_Inquilino _cnInquilino;

        public ContratoController()
        {
            _cnContrato = new CN_Contrato();
            _cnInquilino = new CN_Inquilino();
        }

        [HttpGet]
        public IActionResult Listar([FromQuery] int? estado)
        {
            var contratos = _cnContrato.Listar(estado);
            var response = MapearContratos(contratos);
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

            var response = MapearContrato(contrato);
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

            DateTime fechaInicioParseada = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(request.FechaInicio))
            {
                if (!DateTime.TryParse(request.FechaInicio, out var tempInicio))
                {
                    return BadRequest(new { success = false, mensaje = "El formato de Fecha Inicio es inválido" });
                }
                fechaInicioParseada = tempInicio;
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

            Guid? idTipoIndice = null;
            if (!string.IsNullOrWhiteSpace(request.IdTipoIndice) && Guid.TryParse(request.IdTipoIndice, out var tempIdIndice))
            {
                idTipoIndice = tempIdIndice;
            }

            var contrato = new ContratoAlquiler
            {
                FechaCreacion = fechaInicioParseada,
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

            var (success, message, contratoId) = _cnContrato.Insertar(contrato);

            if (!success)
            {
                return BadRequest(new { success = false, mensaje = message });
            }

            return Ok(new { success = true, mensaje = message, data = new { contratoId } });
        }

        [HttpPut("{id}/anular")]
        [Authorize(Roles = "Superior")]
        public IActionResult Rescindir(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
            {
                return BadRequest(new { success = false, mensaje = "ID inválido" });
            }

            var (success, message) = _cnContrato.Rescindir(guidId);

            if (!success)
            {
                return BadRequest(new { success = false, mensaje = message });
            }

            return Ok(new { success = true, mensaje = message });
        }

        private List<ContratoResponse> MapearContratos(List<ContratoAlquiler> contratos)
        {
            var response = new List<ContratoResponse>();
            foreach (var c in contratos)
            {
                response.Add(MapearContrato(c));
            }
            return response;
        }

        private ContratoResponse MapearContrato(ContratoAlquiler c)
        {
            var moraDiaria = c.TasaMoraMensual / 30m;
            var moraDiariaMonto = c.PrecioCuota * moraDiaria / 100m;

            return new ContratoResponse
            {
                Id = c.IdContratoAlquiler.ToString(),
                Inquilino = c.OInquilino != null ? c.OInquilino.NombreCompleto : "",
                DniInquilino = c.OInquilino?.Dni ?? "",
                Direccion = c.OInmueble?.ODireccion?.Calle ?? "",
                Inmueble = c.OInmueble?.Descripcion ?? "",
                PrecioCuota = c.PrecioCuota,
                CantCuotas = c.CantidadCuotas,
                FechaInicio = c.FechaCreacion.ToString("yyyy-MM-dd"),
                FechaFin = c.FechaFin.ToString("yyyy-MM-dd"),
                MoraMensual = c.TasaMoraMensual,
                MoraDiaria = moraDiaria,
                MoraDiariaMonto = moraDiariaMonto,
                Estado = c.Estado == "Activo" ? 1 : 0
            };
        }
    }

    public class ContratoResponse
    {
        public string Id { get; set; } = "";
        public string Inquilino { get; set; } = "";
        public string DniInquilino { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Inmueble { get; set; } = "";
        public decimal PrecioCuota { get; set; }
        public int CantCuotas { get; set; }
        public string FechaInicio { get; set; } = "";
        public string FechaFin { get; set; } = "";
        public decimal MoraMensual { get; set; }
        public decimal MoraDiaria { get; set; }
        public decimal MoraDiariaMonto { get; set; }
        public int Estado { get; set; }
    }

    public class CrearContratoRequest
    {
        public string? FechaInicio { get; set; }
        public string? FechaFin { get; set; }
        public int CantidadCuotas { get; set; }
        public decimal PrecioCuota { get; set; }
        public decimal TasaMoraMensual { get; set; }
        public string? Condiciones { get; set; }
        public string? InmuebleId { get; set; }
        public string? DniInquilino { get; set; }
        public string? RolInquilinoId { get; set; }
        
        public string? FrecuenciaAjuste { get; set; }
        public string? IdTipoIndice { get; set; }
        public decimal? ValorIndiceInicio { get; set; }
    }
}