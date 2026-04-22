using System;
using System.Collections.Generic;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Inmueble
    {
        private readonly CD_Inmueble _cdInmueble = new();

        public List<Inmueble> Listar(bool soloDisponibles = false)
        {
            return _cdInmueble.Listar(soloDisponibles);
        }

        public Inmueble? ObtenerPorId(Guid id)
        {
            return _cdInmueble.ObtenerPorId(id);
        }
        public bool Registrar(Inmueble obj) => _cdInmueble.Registrar(obj);
        public bool Editar(Inmueble obj) => _cdInmueble.Editar(obj);
        public bool Eliminar(Guid id) => _cdInmueble.Eliminar(id);
    }
}