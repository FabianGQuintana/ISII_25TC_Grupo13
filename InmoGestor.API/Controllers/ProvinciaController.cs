using CapaNegocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InmoGestor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProvinciaController : ControllerBase
    {
        private readonly CN_Provincia _cn;

        public ProvinciaController(CN_Provincia cn) => _cn = cn;

        [HttpGet]
        public IActionResult Listar()
        {
            var data = _cn.ListarProvincias();
            return Ok(new { success = true, data });
        }
    }
}
