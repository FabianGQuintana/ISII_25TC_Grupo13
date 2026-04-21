using System;
using CapaDatos;
using CapaEntidades;

namespace CapaNegocio
{
    public class CN_Usuario
    {
        private readonly CD_Usuario _cdUsuario = new();
        
        public (bool success, Usuario? usuario, string message) Login(string dni, string password)
        {
            if (string.IsNullOrWhiteSpace(dni))
            {
                return (false, null, "El DNI es requerido");
            }
            
            if (string.IsNullOrWhiteSpace(password))
            {
                return (false, null, "La contraseña es requerida");
            }
            
            var usuario = _cdUsuario.ValidarUsuario(dni, password);
            
            if (usuario == null)
            {
                return (false, null, "Credenciales inválidas o usuario inactivo");
            }
            
            return (true, usuario, "Login exitoso");
        }
        
        public Usuario? ObtenerPorId(Guid idUsuario)
        {
            return _cdUsuario.ObtenerPorId(idUsuario);
        }
    }
}