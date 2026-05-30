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

        public List<Localidad> ListarPorProvincia(Guid idProvincia) => _cd.ListarPorProvincia(idProvincia);
    }
}
