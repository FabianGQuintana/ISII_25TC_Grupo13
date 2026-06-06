using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Cuota
    {
        private readonly CD_Cuota _cdCuota = new();
        private readonly CD_CuotaAdicional _cdCuotaAdicional = new();
        private readonly CD_Indice _cdIndice = new();
        private readonly CN_Indice _cnIndice = new();

        private static int FrecuenciaAMeses(string? frecuencia)
        {
            if (string.IsNullOrWhiteSpace(frecuencia))
                return 0;

            return frecuencia.Trim().ToLowerInvariant() switch
            {
                "trimestral" => 3,
                "cuatrimestral" => 4,
                "semestral" => 6,
                "anual" => 12,
                _ => 0
            };
        }

        public List<Cuota> GenerarParaContrato(ContratoAlquiler contrato)
        {
            var cuotas = new List<Cuota>();
            var fecha = contrato.FechaCreacion;
            for (int i = 1; i <= contrato.CantidadCuotas; i++)
            {
                fecha = fecha.AddMonths(1);
                cuotas.Add(new Cuota
                {
                    IdCuota = Guid.NewGuid(),
                    IdContratoAlquiler = contrato.IdContratoAlquiler,
                    NroCuota = i,
                    Periodo = fecha.ToString("yyyyMM"),
                    FechaVencimiento = fecha,
                    Estado = "Pendiente",
                    ImporteTotalCalculado = contrato.PrecioCuota
                });
            }
            return cuotas;
        }

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

            var precioBase = contrato.PrecioCuota;

            // --- Ajuste por �ndice escalonado ---
            var mesesFrecuencia = FrecuenciaAMeses(contrato.FrecuenciaAjuste);
            decimal factor = 1m;
            decimal valorIndiceAplicado = 1m;

            if (mesesFrecuencia > 0
                && contrato.IdTipoIndice.HasValue
                && contrato.ValorIndiceInicio.HasValue
                && contrato.ValorIndiceInicio.Value > 0)
            {
                var ajustes = cuota.NroCuota > 0
                    ? (cuota.NroCuota - 1) / mesesFrecuencia
                    : 0;

                if (ajustes > 0)
                {
                    var hitoFecha = contrato.FechaCreacion.AddMonths(ajustes * mesesFrecuencia);

                    try
                    {
                        var indiceHito = await _cnIndice.ObtenerIndicePorFecha(
                            contrato.IdTipoIndice.Value, hitoFecha);

                        factor = indiceHito / contrato.ValorIndiceInicio.Value;
                        valorIndiceAplicado = factor;
                    }
                    catch
                    {
                        factor = 1m;
                        valorIndiceAplicado = 1m;
                    }
                }
            }

            var importeActualizado = precioBase * factor;
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
                ValorIndiceAplicado = valorIndiceAplicado,
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
