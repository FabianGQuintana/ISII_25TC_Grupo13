using System.Collections.Generic;
using System.Linq;
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
                PropietarioNombreCompleto = i.OPropietario?.NombreCompleto ?? "",
                TipoNombre = ""
            };
        }

        public static List<InmuebleResponse> ToResponseList(List<Inmueble> inmuebles)
            => inmuebles.Select(ToResponse).ToList();
    }
}
