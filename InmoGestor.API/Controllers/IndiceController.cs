using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CapaEntidades;
using CapaNegocio;

namespace InmoGestor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IndiceController : ControllerBase
    {
        private readonly CN_Indice _cnIndice;

        public IndiceController(CN_Indice cnIndice)
        {
            _cnIndice = cnIndice;
        }

        [HttpGet("tipos")]
        public IActionResult ListarTipos()
        {
            var lista = _cnIndice.ListarTipos();
            return Ok(new { success = true, data = lista });
        }

        /// <summary>
        /// Devuelve el valor vigente del índice.
        /// Toda la lógica (caché, fetch externo, guardado) vive en CN_Indice.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerActual(Guid id)
        {
            var indice = await _cnIndice.ObtenerOActualizar(id);
            return Ok(new { success = true, data = indice });
        }

        /// <summary>
        /// Escritura manual. Se mantiene por compatibilidad pero el flujo
        /// normal ya no requiere que el front lo llame.
        /// </summary>
        [HttpPost]
        public IActionResult Guardar([FromBody] HistoricoIndice obj)
        {
            _cnIndice.GuardarHistorico(obj);
            return Ok(new { success = true, data = obj, mensaje = "Índice cacheado exitosamente." });
        }
    }
}
