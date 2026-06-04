using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Localidad
    {
        private readonly CD_Localidad _cd = new();

        public List<Localidad> ListarLocalidades() => _cd.ListarLocalidades();

        public List<Localidad> ListarPorProvincia(Guid idProvincia)
        {
            if (idProvincia == Guid.Empty)
                return new List<Localidad>();

            return _cd.ListarPorProvincia(idProvincia);
        }
    }
}
