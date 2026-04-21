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
            
            var (usuario, errorCode) = _cdUsuario.ValidarUsuario(dni, password);
            
            if (usuario == null)
            {
                return errorCode switch
                {
                    "INACTIVE" => (false, null, "Usuario inactivo. Contacte al administrador."),
                    _ => (false, null, "Credenciales inválidas")
                };
            }
            
            return (true, usuario, "Login exitoso");
        }
        
        public Usuario? ObtenerPorId(Guid idUsuario)
        {
            return _cdUsuario.ObtenerPorId(idUsuario);
        }

        public (bool success, string message) Registrar(string dni, string password, string nombre, string apellido, string email, string rolNombre)
        {
            if (string.IsNullOrWhiteSpace(dni))
            {
                return (false, "El DNI es requerido");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return (false, "El email es requerido");
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                return (false, "La contraseña debe tener al menos 6 caracteres");
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                return (false, "El nombre es requerido");
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                return (false, "El apellido es requerido");
            }

            if (string.IsNullOrWhiteSpace(rolNombre))
            {
                return (false, "El rol es requerido");
            }

            if (rolNombre != "Operador" && rolNombre != "Superior")
            {
                return (false, "El rol debe ser 'Operador' o 'Superior'");
            }

            var persona = new Persona
            {
                Dni = dni,
                Nombre = nombre,
                Apellido = apellido,
                Email = email
            };

            return _cdUsuario.Registrar(persona, password, rolNombre);
        }
    }
}