using CapaNegocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace InmoGestor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TipoAdicionalController : ControllerBase
    {
        private readonly CN_TipoAdicional _cnTipoAdicional;

        public TipoAdicionalController(CN_TipoAdicional cnTipoAdicional)
        {
            _cnTipoAdicional = cnTipoAdicional;
        }

        [HttpGet]
        public IActionResult ListarAdicionales()
        {
            var tipos = _cnTipoAdicional.ListarAdicionales();
            var response = tipos.Select(t => new
            {
                id = t.IdTipoAdicionales.ToString(),
                descripcion = t.Descripcion,
                montoBase = t.MontoBase
            }).ToList();

            return Ok(new { success = true, data = response });
        }
    }
}