#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CapaEntidades;
using InmoGestor.API.DTOs;

namespace InmoGestor.API.Mappers
{
    public static class ContratoMapper
    {
        public static ContratoResponse ToResponse(ContratoAlquiler c)
        {
            return new ContratoResponse
            {
                Id = c.IdContratoAlquiler.ToString(),
                Inquilino = c.OInquilino != null ? c.OInquilino.NombreCompleto : "",
                DniInquilino = c.OInquilino?.Dni ?? "",
                Direccion = c.OInmueble?.ODireccion?.Calle ?? "",
                Inmueble = c.OInmueble?.Descripcion ?? "",
                PrecioCuota = c.PrecioCuota,
                CantCuotas = c.CantidadCuotas,
                FechaInicio = c.FechaInicio.ToString("yyyy-MM-dd"),
                FechaFin = c.FechaFin.ToString("yyyy-MM-dd"),
                MoraMensual = c.TasaMoraMensual,
                MoraDiaria = c.MoraDiaria,
                MoraDiariaMonto = c.MoraDiariaMonto,
                Estado = c.Estado == "Activo" ? 1 : 0
            };
        }

        public static List<ContratoResponse> ToResponseList(List<ContratoAlquiler> contratos)
            => contratos.Select(ToResponse).ToList();

        public static ContratoAlquiler ToEntity(CrearContratoRequest request, Persona inquilino, Guid userId)
        {
            var fechaInicio = request.FechaInicio?.Date ?? DateTime.Today;
            var fechaFin = request.FechaFin?.Date ?? fechaInicio.AddMonths(request.CantidadCuotas);
            return new ContratoAlquiler
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                CantidadCuotas = request.CantidadCuotas,
                PrecioCuota = request.PrecioCuota,
                TasaMoraMensual = request.TasaMoraMensual,
                Condiciones = request.Condiciones,
                IdInmueble = request.InmuebleId,
                IdPersonaInquilino = inquilino.IdPersona,
                IdRolClienteInquilino = request.RolInquilinoId ?? Guid.Empty,
                IdUsuarioCreador = userId,
                FrecuenciaAjuste = request.FrecuenciaAjuste,
                IdTipoIndice = request.IdTipoIndice,
                ValorIndiceInicio = request.ValorIndiceInicio
            };
        }
    }
}
