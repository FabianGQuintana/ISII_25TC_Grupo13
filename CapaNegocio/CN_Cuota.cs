using System;
using System.Threading.Tasks;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Cuota
    {
        private readonly CD_Cuota _cdCuota = new();
        private readonly CD_CuotaAdicional _cdCuotaAdicional = new();

        private readonly CN_Indice _cnIndice = new();

        public Cuota? ObtenerCuotaPorContrato(Guid contratoId)
        {
            if (contratoId == Guid.Empty)
                throw new ArgumentException("El ID del contrato es requerido");
            return _cdCuota.ObtenerUltimaPendientePorContrato(contratoId);
        }

        public string CambiarEstadoCuotas(Guid idContrato)
        {
            _cdCuota.AnularPendientesPorContrato(idContrato);
            return "Cuotas actualizadas correctamente";
        }

        public async Task<CuotaCalculadaDto?> ObtenerCuotaCalculada(Guid idContrato)
        {
            var cuota = _cdCuota.ObtenerUltimaPendientePorContrato(idContrato);

            if (cuota == null)
                return null;

            var contrato = new CD_Contrato().ObtenerPorId(idContrato);

            if (contrato == null)
                return null;

            var diasAtraso = cuota.FechaVencimiento < DateTime.Today
                ? (DateTime.Today - cuota.FechaVencimiento).Days
                : 0;

            var mora = diasAtraso > 0
                ? contrato.MoraDiariaMonto * diasAtraso
                : 0;

            decimal indice = 1;
            try
            {
                indice = await _cnIndice.ObtenerIndice(cuota.IdCuota);
            }
            catch
            {
                indice = 1;
            }

            var precioBase = contrato.PrecioCuota;
            var importeActualizado = precioBase * indice;
            var adicionales = _cdCuotaAdicional.ObtenerTotalAdicionales(cuota.IdCuota);
            var totalFinal = importeActualizado
                           + mora
                           + adicionales
                           - Math.Max(0, cuota.DescuentoAdicionalTotal);

            return new CuotaCalculadaDto
            {
                IdCuota = cuota.IdCuota.ToString(),
                NroCuota = cuota.NroCuota,
                Periodo = cuota.Periodo,
                FechaVencimiento = cuota.FechaVencimiento,
                PrecioCuota = precioBase,
                ValorIndiceAplicado = indice,
                ImporteActualizado = importeActualizado,
                TotalAdicionales = adicionales,
                TotalDescuentos = cuota.DescuentoAdicionalTotal,
                DiasAtraso = diasAtraso,
                MoraCalculada = mora,
                TotalFinal = totalFinal,
                Estado = cuota.Estado
            };
        }
    }
}
