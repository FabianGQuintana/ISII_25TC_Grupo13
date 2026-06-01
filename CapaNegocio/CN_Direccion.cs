using System;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Direccion
    {
        private readonly CD_Direccion _cd = new();

        public bool AgregarDireccion(Direccion obj)
        {
            if (obj == null)
                return false;

            if (string.IsNullOrWhiteSpace(obj.Calle) || string.IsNullOrWhiteSpace(obj.Altura))
                return false;

            return _cd.AgregarDireccion(obj);
        }

        public bool EditarDireccion(Guid idInmueble, Direccion obj)
        {
            if (idInmueble == Guid.Empty || obj == null)
                return false;

            if (string.IsNullOrWhiteSpace(obj.Calle) || string.IsNullOrWhiteSpace(obj.Altura))
                return false;

            return _cd.EditarDireccion(idInmueble, obj);
        }

        public bool EliminarDireccion(Guid idDireccion)
        {
            if (idDireccion == Guid.Empty)
                return false;

            return _cd.EliminarDireccion(idDireccion);
        }
    }
}
