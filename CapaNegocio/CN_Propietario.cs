using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Propietario
    {
        private readonly CD_Propietario _cd = new();

        public List<Propietario> ListarPropietarios() => _cd.ListarPropietarios();

        public Propietario? ObtenerPorId(Guid id) => _cd.ObtenerPorId(id);
    }
}
