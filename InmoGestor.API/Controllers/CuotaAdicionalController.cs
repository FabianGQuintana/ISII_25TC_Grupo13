using CapaEntidades;
using CapaNegocio;
using InmoGestor.API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace InmoGestor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuotaAdicionalController : ControllerBase
    {
        private readonly CN_CuotaAdicional _cn = new();

        [HttpPost]
        public IActionResult Crear([FromBody] CrearCuotaAdicionalRequest request)
        {
            if (!Guid.TryParse(request.IdCuota, out var idCuota))
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Id cuota inválido"
                });
            }

            if (!Guid.TryParse(request.IdTipoAdicionales, out var idTipo))
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Id tipo adicional inválido"
                });
            }

            var adicional = new CuotaAdicional
            {
                IdCuotaAdicional = Guid.NewGuid(),

                IdCuota = idCuota,

                IdTipoAdicionales = idTipo,

                MontoAplicado = request.MontoAplicado ?? 0,

                DescripcionManual = request.DescripcionManual
            };

            var success = _cn.Insertar(adicional);

            if (!success)
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "No se pudo crear el adicional"
                });
            }

            return Ok(new
            {
                success = true,
                mensaje = "Adicional agregado correctamente"
            });
        }
    }
}
