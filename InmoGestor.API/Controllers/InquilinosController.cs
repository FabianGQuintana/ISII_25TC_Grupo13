using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CapaNegocio;
using System.Collections.Generic;
using CapaEntidades;

namespace InmoGestor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InquilinosController : ControllerBase
    {
        private readonly CN_Persona _cnPersona;

        public InquilinosController()
        {
            _cnPersona = new CN_Persona();
        }

        [HttpGet]
        public IActionResult ObtenerInquilinos()
        {
            var personas = _cnPersona.ObtenerTodos();

            return Ok(new
            {
                success = true,
                data = personas
            });
        }
    }
}