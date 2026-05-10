#nullable enable
using System.Collections.Generic;
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
                IdPersonaInquilino = c.IdPersonaInquilino.ToString(),
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
        {
            var response = new List<ContratoResponse>();
            foreach (var c in contratos)
                response.Add(ToResponse(c));
            return response;
        }
    }
}
