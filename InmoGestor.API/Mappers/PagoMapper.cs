#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CapaEntidades;
using InmoGestor.API.DTOs;

namespace InmoGestor.API.Mappers
{
    public static class PagoMapper
    {
        public static PagoResponseDto ToResponse(Pago p)
        {
            return new PagoResponseDto
            {
                Id = p.IdPago.ToString(),
                ContratoId = p.IdContrato.ToString(),
                CuotaId = p.IdCuota.ToString(),
                Inquilino = p.Inquilino,
                Inmueble = p.Inmueble,
                NroCuota = p.NroCuota,
                Periodo = p.Periodo,
                FechaVencimiento = p.FechaVencimiento,
                FechaPago = p.FechaPago == DateTime.MinValue ? null : p.FechaPago,
                Monto = p.MontoTotal,
                Mora = p.MoraCobrada,
                DiasAtraso = p.FechaVencimiento.HasValue
                    ? Math.Max(0, (DateTime.Now - p.FechaVencimiento.Value).Days)
                    : 0,
                TotalPagado = p.MontoTotal,
                Estado = p.Estado,
                EstadoTexto = p.Estado
            };
        }

        public static List<PagoResponseDto> ToResponseList(List<Pago> pagos)
            => pagos.Select(ToResponse).ToList();
    }
}
