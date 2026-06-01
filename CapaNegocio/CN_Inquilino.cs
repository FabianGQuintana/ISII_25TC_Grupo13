using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Inquilino
    {
        private readonly CD_Inquilino _cdInquilino = new();

        public List<Persona> ListarInquilinos()
        {
            return _cdInquilino.ListarInquilinos();
        }

        public List<Persona> ListarConContratosActivos()
        {
            return _cdInquilino.ListarConContratosActivos();
        }

        public Persona? ObtenerPorDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
                return null;

            return _cdInquilino.ObtenerPorDni(dni.Trim());
        }

        public Persona? ObtenerPorId(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return _cdInquilino.ObtenerPorId(id);
        }
    }
}
