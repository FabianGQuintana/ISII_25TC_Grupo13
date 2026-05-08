using System.Collections.Generic;
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
        {
            var response = new List<InquilinoResponse>();
            foreach (var i in inquilinos)
                response.Add(ToResponse(i));
            return response;
        }
    }
}
