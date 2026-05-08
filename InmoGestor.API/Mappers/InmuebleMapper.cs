using System.Collections.Generic;
using CapaEntidades;
using InmoGestor.API.DTOs;

namespace InmoGestor.API.Mappers
{
    public static class InmuebleMapper
    {
        public static InmuebleResponse ToResponse(Inmueble i)
        {
            return new InmuebleResponse
            {
                Id = i.IdInmueble.ToString(),
                Direccion = i.ODireccion?.Calle ?? "",
                Descripcion = i.Descripcion ?? "",
                Disponibilidad = i.Disponibilidad ? 1 : 0,
                DisponibilidadNombre = i.Disponibilidad ? "Disponible" : "No disponible",
                PropietarioNombreCompleto = i.OPropietario != null ? $"{i.OPropietario.Nombre} {i.OPropietario.Apellido}" : "",
                TipoNombre = ""
            };
        }

        public static List<InmuebleResponse> ToResponseList(List<Inmueble> inmuebles)
        {
            var response = new List<InmuebleResponse>();
            foreach (var i in inmuebles)
                response.Add(ToResponse(i));
            return response;
        }
    }
}
