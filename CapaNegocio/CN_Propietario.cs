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

        public Propietario? ObtenerPorDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
                return null;

            return _cd.ObtenerPorDni(dni.Trim());
        }

        public Propietario? ObtenerPorId(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return _cd.ObtenerPorId(id);
        }
    }
}
