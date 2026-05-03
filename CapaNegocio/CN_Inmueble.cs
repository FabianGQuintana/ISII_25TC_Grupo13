using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Inmueble
    {
        private readonly CD_Inmueble _cdInmueble = new();
        
        public List<Inmueble> ObtenerTodos()
        {
            return _cdInmueble.ObtenerTodos();
        }
    }
}