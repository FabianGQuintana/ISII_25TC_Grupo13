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
    public class InmueblesController : ControllerBase
    {
        private readonly CN_Inmueble _cnInmueble;

        public InmueblesController()
        {
            _cnInmueble = new CN_Inmueble();
        }

        [HttpGet]
        public IActionResult ObtenerTodos()
        {
            var inmuebles = _cnInmueble.ObtenerTodos();

            return Ok(new
            {
                success = true,
                data = inmuebles
            });
        }
    }
}