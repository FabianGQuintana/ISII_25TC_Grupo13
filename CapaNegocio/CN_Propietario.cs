using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Propietario : CN_PersonaRolCliente<Propietario>
    {
        private readonly CD_Propietario _cd = new();

        public override Propietario? ObtenerPorId(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return _cd.ObtenerPorId(id);
        }

        public override Propietario? ObtenerPorDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
                return null;

            return _cd.ObtenerPorDni(dni.Trim());
        }

        public List<Propietario> ListarPropietarios()
            => _cd.ListarPropietarios();
    }
}
