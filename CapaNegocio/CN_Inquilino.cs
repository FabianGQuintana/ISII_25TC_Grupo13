using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Inquilino : CN_PersonaRolCliente<Persona>
    {
        private readonly CD_Inquilino _cdInquilino = new();

        public override Persona? ObtenerPorId(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return _cdInquilino.ObtenerPorId(id);
        }

        public override Persona? ObtenerPorDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
                return null;

            return _cdInquilino.ObtenerPorDni(dni.Trim());
        }

        public List<Inquilino> ListarInquilinos()
            => _cdInquilino.ListarInquilinos();

        public List<Inquilino> ListarConContratosActivos()
            => _cdInquilino.ListarConContratosActivos();
    }
}
