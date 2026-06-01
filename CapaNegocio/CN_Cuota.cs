using System;
using System.Collections.Generic;
using System.Linq;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Cuota
    {
        private readonly CD_Cuota _cdCuota = new();
        private readonly CD_CuotaAdicional _cdCuotaAdicional = new();

        public List<Cuota> ListarPendientesPorContrato(Guid idContrato)
        {
            if (idContrato == Guid.Empty)
                return new List<Cuota>();

            return _cdCuota.ListarPendientesPorContrato(idContrato);
        }

        public List<CuotaCalculadaDto> ObtenerCuotasCalculadas(
            Guid idContrato,
            ContratoAlquiler contrato)
        {
            if (idContrato == Guid.Empty || contrato == null)
                return new List<CuotaCalculadaDto>();

            var cuotas = _cdCuota.ListarPendientesPorContrato(idContrato);

            var lista = cuotas.Select(c =>
            {
                var diasAtraso =
                    c.FechaVencimiento < DateTime.Today
                    ? (DateTime.Today - c.FechaVencimiento).Days
                    : 0;

                var mora =
                    diasAtraso > 0
                    ? contrato.MoraDiariaMonto * diasAtraso
                    : 0;

                var indice =
                    c.ValorIndiceAplicado > 0
                    ? c.ValorIndiceAplicado
                    : 1m;

                var precioBase = contrato.PrecioCuota;
                var importeActualizado = precioBase * indice;
                var adicionales = _cdCuotaAdicional.ObtenerTotalAdicionales(c.IdCuota);
                var totalFinal = importeActualizado + mora + adicionales - c.DescuentoAdicionalTotal;

                return new CuotaCalculadaDto
                {
                    IdCuota = c.IdCuota.ToString(),
                    NroCuota = c.NroCuota,
                    Periodo = c.Periodo,
                    FechaVencimiento = c.FechaVencimiento,
                    PrecioCuota = precioBase,
                    ValorIndiceAplicado = indice,
                    ImporteActualizado = importeActualizado,
                    TotalAdicionales = adicionales,
                    TotalDescuentos = c.DescuentoAdicionalTotal,
                    DiasAtraso = diasAtraso,
                    MoraCalculada = mora,
                    TotalFinal = totalFinal,
                    Estado = diasAtraso > 0 ? "Vencida" : c.Estado
                };
            }).ToList();

            return lista;
        }

        public CuotaCalculadaDto? ObtenerCuotaCalculada(Guid idCuota)
        {
            if (idCuota == Guid.Empty)
                return null;

            var cuota = _cdCuota.ObtenerPorId(idCuota);

            if (cuota == null)
                return null;

            var contrato = new CD_Contrato().ObtenerPorId(cuota.IdContratoAlquiler);

            if (contrato == null)
                return null;

            var diasAtraso =
                cuota.FechaVencimiento < DateTime.Today
                ? (DateTime.Today - cuota.FechaVencimiento).Days
                : 0;

            var mora =
                diasAtraso > 0
                ? contrato.MoraDiariaMonto * diasAtraso
                : 0;

            var indice =
                cuota.ValorIndiceAplicado > 0
                ? cuota.ValorIndiceAplicado
                : 1m;

            var precioBase = contrato.PrecioCuota;
            var importeActualizado = precioBase * indice;
            var adicionales = _cdCuotaAdicional.ObtenerTotalAdicionales(cuota.IdCuota);
            var totalFinal = importeActualizado + mora + adicionales - cuota.DescuentoAdicionalTotal;

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
