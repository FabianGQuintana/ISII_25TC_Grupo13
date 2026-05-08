using System.Collections.Generic;
using System.Linq;
using CapaEntidades;
using InmoGestor.API.DTOs;

namespace InmoGestor.API.Mappers
{
    public static class InquilinoMapper
    {
        public static InquilinoResponse ToResponse(Persona i)
        {
            return new InquilinoResponse
            {
                Dni = i.Dni ?? "",
                NombreCompleto = i.NombreCompleto,
                Nombre = i.Nombre,
                Apellido = i.Apellido
            };
        }

        public static List<InquilinoResponse> ToResponseList(List<Persona> inquilinos)
            => inquilinos.Select(ToResponse).ToList();
    }
}
