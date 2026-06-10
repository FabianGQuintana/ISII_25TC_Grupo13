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
                Id = i.IdPersona.ToString(),
                Dni = i.Dni ?? "",
                NombreCompleto = i.NombreCompleto,
                Nombre = i.Nombre,
                Apellido = i.Apellido
            };
        }

        public static InquilinoResponse ToResponse(Inquilino i)
            => ToResponse(i.OPersona!);

        public static List<InquilinoResponse> ToResponseList(List<Inquilino> inquilinos)
            => inquilinos.Select(ToResponse).ToList();
    }
}
