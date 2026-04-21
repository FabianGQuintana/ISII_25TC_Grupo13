using System;
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
        private CN_Indice _cnIndice = new CN_Indice();

        [HttpGet("{id}")]
        public IActionResult ObtenerActual(Guid id)
        {
            try
            {
                var indice = _cnIndice.ObtenerActual(id);
                if (indice == null)
                    return Ok(new { success = false, mensaje = "No hay registro actual guardado para esta fecha." });

                return Ok(new { success = true, data = indice });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Guardar([FromBody] HistoricoIndice obj)
        {
            try
            {
                _cnIndice.GuardarHistorico(obj);
                // Retornamos también la entidad por si el front la necesita fresca
                return Ok(new { success = true, data = obj, mensaje = "Índice cacheado exitosamente." });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, mensaje = ex.Message });
            }
        }
    }
}
