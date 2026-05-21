using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Localidad
    {
        private readonly CD_Localidad _cd = new();

        public List<Localidad> Listar(Guid? idProvincia = null) => _cd.Listar(idProvincia);
    }
}
