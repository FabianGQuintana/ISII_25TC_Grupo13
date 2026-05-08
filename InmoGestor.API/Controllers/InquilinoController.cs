using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CapaNegocio;
using InmoGestor.API.Mappers;

namespace InmoGestor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InquilinoController : ControllerBase
    {
        private readonly CN_Inquilino _cnInquilino;

        public InquilinoController(CN_Inquilino cnInquilino)
        {
            _cnInquilino = cnInquilino;
        }

        [HttpGet]
        public IActionResult ListarInquilinos()
        {
            var inquilinos = _cnInquilino.ListarInquilinos();
            return Ok(new { success = true, data = InquilinoMapper.ToResponseList(inquilinos) });
        }

        [HttpGet("con-contratos")]
        public IActionResult ListarConContratos()
        {
            var inquilinos = _cnInquilino.ListarConContratosActivos();
            return Ok(new { success = true, data = InquilinoMapper.ToResponseList(inquilinos) });
        }

        [HttpGet("{id}")]
        public IActionResult ObtenerPorId(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
                return BadRequest(new { success = false, mensaje = "ID inválido" });

            var inquilino = _cnInquilino.ObtenerPorId(guidId);
            if (inquilino == null)
                return NotFound(new { success = false, mensaje = "Inquilino no encontrado" });

            return Ok(new { success = true, data = InquilinoMapper.ToResponse(inquilino) });
        }

        [HttpGet("dni/{dni}")]
        public IActionResult ObtenerPorDni(string dni)
        {
            var inquilino = _cnInquilino.ObtenerPorDni(dni);
            if (inquilino == null)
                return NotFound(new { success = false, mensaje = "Inquilino no encontrado" });

            return Ok(new { success = true, data = InquilinoMapper.ToResponse(inquilino) });
        }
    }
}
