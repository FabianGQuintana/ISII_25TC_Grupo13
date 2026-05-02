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
    public class InquilinoController : ControllerBase
    {
        private readonly CN_Inquilino _cnInquilino;

        public InquilinoController(CN_Inquilino cnInquilino)
        {
            _cnInquilino = cnInquilino;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var inquilinos = _cnInquilino.Listar();
            var response = MapearInquilinos(inquilinos);
            return Ok(new { success = true, data = response });
        }

        [HttpGet("con-contratos")]
        public IActionResult ListarConContratos()
        {
            var inquilinos = _cnInquilino.ListarConContratosActivos();
            var response = MapearInquilinos(inquilinos);
            return Ok(new { success = true, data = response });
        }

        [HttpGet("{id}")]
        public IActionResult ObtenerPorId(string id)
        {
            if (!Guid.TryParse(id, out var guidId))
            {
                return BadRequest(new { success = false, mensaje = "ID inválido" });
            }

            var inquilino = _cnInquilino.ObtenerPorId(guidId);
            if (inquilino == null)
            {
                return NotFound(new { success = false, mensaje = "Inquilino no encontrado" });
            }

            var response = MapearInquilino(inquilino);
            return Ok(new { success = true, data = response });
        }

        [HttpGet("dni/{dni}")]
        public IActionResult ObtenerPorDni(string dni)
        {
            var inquilino = _cnInquilino.ObtenerPorDni(dni);
            if (inquilino == null)
            {
                return NotFound(new { success = false, mensaje = "Inquilino no encontrado" });
            }

            var response = MapearInquilino(inquilino);
            return Ok(new { success = true, data = response });
        }

        private List<InquilinoResponse> MapearInquilinos(List<Persona> inquilinos)
        {
            var response = new List<InquilinoResponse>();
            foreach (var i in inquilinos)
            {
                response.Add(MapearInquilino(i));
            }
            return response;
        }

        private InquilinoResponse MapearInquilino(Persona i)
        {
            return new InquilinoResponse
            {
                Dni = i.Dni ?? "",
                NombreCompleto = i.NombreCompleto,
                Nombre = i.Nombre,
                Apellido = i.Apellido
            };
        }
    }

    public class InquilinoResponse
    {
        public string Dni { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
    }
}