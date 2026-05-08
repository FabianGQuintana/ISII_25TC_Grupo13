using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Persona
    {
        private readonly CD_Persona _cdPersona = new();
        
        public List<Persona> ObtenerTodos()
        {
            return _cdPersona.ObtenerTodos();
        }
    }
}