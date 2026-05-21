using System;
using CapaNegocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InmoGestor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LocalidadController : ControllerBase
    {
        private readonly CN_Localidad _cn;

        public LocalidadController(CN_Localidad cn) => _cn = cn;

        [HttpGet]
        public IActionResult Listar([FromQuery] string? idProvincia = null)
        {
            Guid? guid = null;
            if (!string.IsNullOrEmpty(idProvincia) && Guid.TryParse(idProvincia, out var parsed))
                guid = parsed;

            var data = _cn.Listar(guid);
            return Ok(new { success = true, data });
        }
    }
}
