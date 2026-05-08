using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Inquilino
    {
        private readonly CD_Inquilino _cdInquilino = new();

        public List<Persona> Listar() => ListarInquilinos();

        public List<Persona> ListarInquilinos()
        {
            return _cdInquilino.Listar();
        }

        public List<Persona> ListarConContratosActivos()
        {
            return _cdInquilino.ListarConContratosActivos();
        }

        public Persona? ObtenerPorDni(string dni)
        {
            return _cdInquilino.ObtenerPorDni(dni);
        }

        public Persona? ObtenerPorId(Guid id)
        {
            return _cdInquilino.ObtenerPorId(id);
        }
    }
}