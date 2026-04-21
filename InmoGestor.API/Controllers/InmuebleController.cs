using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CapaNegocio;
using CapaEntidades;

namespace InmoGestor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InmuebleController : ControllerBase
    {
        private readonly CN_Inmueble _cnInmueble;

        public InmuebleController()
        {
            _cnInmueble = new CN_Inmueble();
        }

        [HttpGet]
        public IActionResult Listar([FromQuery] bool disponibles = false)
        {
            var inmuebles = _cnInmueble.Listar(disponibles);
            var response = MapearInmuebles(inmuebles);
            return Ok(new { success = true, data = response });
        }

        [HttpGet("{id}")]
        public IActionResult ObtenerPorId(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
            {
                return BadRequest(new { success = false, mensaje = "ID inválido" });
            }

            var inmueble = _cnInmueble.ObtenerPorId(guidId);
            if (inmueble == null)
            {
                return NotFound(new { success = false, mensaje = "Inmueble no encontrado" });
            }

            var response = MapearInmueble(inmueble);
            return Ok(new { success = true, data = response });
        }

        private List<InmuebleResponse> MapearInmuebles(List<Inmueble> inmuebles)
        {
            var response = new List<InmuebleResponse>();
            foreach (var i in inmuebles)
            {
                response.Add(MapearInmueble(i));
            }
            return response;
        }

        private InmuebleResponse MapearInmueble(Inmueble i)
        {
            return new InmuebleResponse
            {
                Id = i.IdInmueble.ToString(),
                Direccion = i.ODireccion?.Calle ?? "",
                Descripcion = i.Descripcion ?? "",
                Disponibilidad = i.Disponibilidad ? 1 : 0,
                DisponibilidadNombre = i.Disponibilidad ? "Disponible" : "No disponible",
                PropietarioNombreCompleto = i.OPropietario != null ? $"{i.OPropietario.Nombre} {i.OPropietario.Apellido}" : "",
                TipoNombre = ""
            };
        }
    }

    public class InmuebleResponse
    {
        public string Id { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int Disponibilidad { get; set; }
        public string DisponibilidadNombre { get; set; } = "";
        public string PropietarioNombreCompleto { get; set; } = "";
        public string TipoNombre { get; set; } = "";
    }
}