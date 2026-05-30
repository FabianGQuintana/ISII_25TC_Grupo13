using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Inmueble
    {
        private readonly CD_Inmueble _cdInmueble = new();

        public List<Inmueble> ListarInmuebles(bool soloDisponibles = false)
        {
            return _cdInmueble.ListarInmuebles(soloDisponibles);
        }

        public Inmueble? ObtenerPorId(Guid id)
        {
            return _cdInmueble.ObtenerPorId(id);
        }
        public bool RegistrarInmueble(Inmueble obj) => _cdInmueble.RegistrarInmueble(obj);
        public bool EditarInmueble(Inmueble obj) => _cdInmueble.EditarInmueble(obj);
        public bool EliminarInmueble(Guid id) => _cdInmueble.EliminarInmueble(id);
    }
}