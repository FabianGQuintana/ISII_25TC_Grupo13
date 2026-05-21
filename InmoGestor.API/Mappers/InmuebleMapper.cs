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
            var dir = i.ODireccion;
            var loc = dir?.OLocalidad;
            var prov = loc?.OProvincia;

            return new InmuebleResponse
            {
                Id = i.IdInmueble.ToString(),
                Direccion = dir != null ? $"{dir.Calle} {dir.Altura}".Trim() : "",
                Descripcion = i.Descripcion ?? "",
                Disponibilidad = i.Disponibilidad ? 1 : 0,
                DisponibilidadNombre = i.Disponibilidad ? "Disponible" : "No disponible",
                PropietarioNombreCompleto = i.OPropietario?.NombreCompleto ?? "",
                Localidad = loc?.Nombre,
                Provincia = prov?.Nombre,
                IdLocalidad = loc?.IdLocalidad.ToString(),
                IdProvincia = prov?.IdProvincia.ToString()
            };
        }

        public static List<InmuebleResponse> ToResponseList(List<Inmueble> inmuebles)
            => inmuebles.Select(ToResponse).ToList();
    }
}
