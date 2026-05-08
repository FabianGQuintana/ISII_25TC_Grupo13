using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CapaNegocio;
using CapaEntidades;
using InmoGestor.API.Mappers;

namespace InmoGestor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InmuebleController : ControllerBase
    {
        private readonly CN_Inmueble _cnInmueble;

        public InmuebleController(CN_Inmueble cnInmueble)
        {
            _cnInmueble = cnInmueble;
        }

        [HttpGet]
        public IActionResult ListarInmuebles([FromQuery] bool disponibles = false)
        {
            var inmuebles = _cnInmueble.ListarInmuebles(disponibles);
            return Ok(new { success = true, data = InmuebleMapper.ToResponseList(inmuebles) });
        }

        [HttpGet("{id}")]
        public IActionResult ObtenerPorId(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
                return BadRequest(new { success = false, mensaje = "ID inválido" });

            var inmueble = _cnInmueble.ObtenerPorId(guidId);
            if (inmueble == null)
                return NotFound(new { success = false, mensaje = "Inmueble no encontrado" });

            return Ok(new { success = true, data = InmuebleMapper.ToResponse(inmueble) });
        }

        [HttpPost]
        public IActionResult Registrar([FromBody] Inmueble request)
        {
            var resultado = _cnInmueble.Registrar(request);
            return Ok(new { success = resultado });
        }

        [HttpPut]
        public IActionResult Editar([FromBody] Inmueble request)
        {
            var resultado = _cnInmueble.Editar(request);
            return Ok(new { success = resultado });
        }

        [HttpDelete("{id}")]
        public IActionResult Eliminar(string id)
        {
            if (!Guid.TryParse(id, out var guidId)) return BadRequest();
            var resultado = _cnInmueble.Eliminar(guidId);
            return Ok(new { success = resultado });
        }
    }
}
