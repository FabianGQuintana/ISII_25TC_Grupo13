using System;
using System.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_Usuario
    {
        public Usuario? ValidarUsuario(string dni, string password)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT 
                        u.id_usuario, 
                        u.clave, 
                        u.estado,
                        u.id_persona, 
                        u.id_rol_usuario,
                        p.dni,
                        p.nombre, 
                        p.apellido, 
                        p.email,
                        r.nombre AS rol_nombre
                    FROM usuario u
                    INNER JOIN persona p ON u.id_persona = p.id_persona
                    INNER JOIN rol_usuario r ON u.id_rol_usuario = r.id_rol_usuario
                    WHERE p.dni = @dni 
                        AND u.estado = 'Activo'";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@dni", dni);
                    
                    cn.Open();
                    
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            var claveHash = dr["clave"].ToString();
                            
                            if (BCrypt.Net.BCrypt.Verify(password, claveHash))
                            {
                                return new Usuario
                                {
                                    IdUsuario = Guid.Parse(dr["id_usuario"].ToString()!),
                                    Clave = claveHash!,
                                    Estado = dr["estado"].ToString()!,
                                    IdPersona = Guid.Parse(dr["id_persona"].ToString()!),
                                    IdRolUsuario = Guid.Parse(dr["id_rol_usuario"].ToString()!),
                                    OPersona = new Persona
                                    {
                                        IdPersona = Guid.Parse(dr["id_persona"].ToString()!),
                                        Dni = dr["dni"]?.ToString(),
                                        Nombre = dr["nombre"].ToString()!,
                                        Apellido = dr["apellido"].ToString()!,
                                        Email = dr["email"].ToString()!
                                    },
                                    ORolUsuario = new RolUsuario
                                    {
                                        IdRolUsuario = Guid.Parse(dr["id_rol_usuario"].ToString()!),
                                        Nombre = dr["rol_nombre"].ToString()!
                                    }
                                };
                            }
                        }
                    }
                }
            }
            return null;
        }
        
        public Usuario? ObtenerPorId(Guid idUsuario)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT 
                        u.id_usuario, 
                        u.clave, 
                        u.estado,
                        u.id_persona, 
                        u.id_rol_usuario,
                        p.dni,
                        p.nombre, 
                        p.apellido, 
                        p.email,
                        r.nombre AS rol_nombre
                    FROM usuario u
                    INNER JOIN persona p ON u.id_persona = p.id_persona
                    INNER JOIN rol_usuario r ON u.id_rol_usuario = r.id_rol_usuario
                    WHERE u.id_usuario = @id";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    
                    cn.Open();
                    
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new Usuario
                            {
                                IdUsuario = Guid.Parse(dr["id_usuario"].ToString()!),
                                Clave = dr["clave"].ToString()!,
                                Estado = dr["estado"].ToString()!,
                                IdPersona = Guid.Parse(dr["id_persona"].ToString()!),
                                IdRolUsuario = Guid.Parse(dr["id_rol_usuario"].ToString()!),
                                OPersona = new Persona
                                {
                                    IdPersona = Guid.Parse(dr["id_persona"].ToString()!),
                                    Dni = dr["dni"]?.ToString(),
                                    Nombre = dr["nombre"].ToString()!,
                                    Apellido = dr["apellido"].ToString()!,
                                    Email = dr["email"].ToString()!
                                },
                                ORolUsuario = new RolUsuario
                                {
                                    IdRolUsuario = Guid.Parse(dr["id_rol_usuario"].ToString()!),
                                    Nombre = dr["rol_nombre"].ToString()!
                                }
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}