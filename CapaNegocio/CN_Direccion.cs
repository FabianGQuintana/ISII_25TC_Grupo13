using System;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Direccion
    {
        private readonly CD_Direccion _cd = new();

        public bool AgregarDireccion(Direccion obj) => _cd.AgregarDireccion(obj);

        public bool EditarDireccion(Guid idInmueble, Direccion obj) => _cd.EditarDireccion(idInmueble, obj);

        public bool EliminarDireccion(Guid idDireccion) => _cd.EliminarDireccion(idDireccion);
    }
}
