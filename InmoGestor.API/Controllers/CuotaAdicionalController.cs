using CapaEntidades;
using CapaNegocio;
using InmoGestor.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace InmoGestor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CuotaAdicionalController : ControllerBase
    {
        private readonly CN_CuotaAdicional _cn = new();

        [HttpPost]
        public IActionResult InsertarCuotaAdicional([FromBody] CrearCuotaAdicionalRequest cuotaAdicional)
        {
            if (!Guid.TryParse(cuotaAdicional.IdCuota, out var idCuota))
            {
                return BadRequest(new
                {
                    success = false,
                    mensaje = "Id cuota inválido"
                });
            }

            if (!Guid.TryParse(cuotaAdicional.IdTipoAdicionales, out var idTipo))
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

                MontoAplicado = cuotaAdicional.MontoAplicado ?? 0,

                DescripcionManual = cuotaAdicional.DescripcionManual
            };

            var success = _cn.InsertarCuotaAdicional(adicional);

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
