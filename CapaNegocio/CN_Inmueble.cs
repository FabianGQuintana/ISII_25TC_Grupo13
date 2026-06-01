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
            if (id == Guid.Empty)
                return null;

            return _cdInmueble.ObtenerPorId(id);
        }

        public bool RegistrarInmueble(Inmueble obj)
        {
            if (obj == null)
                return false;

            if (obj.OPropietario == null || obj.OPropietario.IdPersona == Guid.Empty)
                return false;

            return _cdInmueble.RegistrarInmueble(obj);
        }

        public bool EditarInmueble(Inmueble obj)
        {
            if (obj == null || obj.IdInmueble == Guid.Empty)
                return false;

            return _cdInmueble.EditarInmueble(obj);
        }

        public bool EliminarInmueble(Guid id)
        {
            if (id == Guid.Empty)
                return false;

            return _cdInmueble.EliminarInmueble(id);
        }
    }
}
