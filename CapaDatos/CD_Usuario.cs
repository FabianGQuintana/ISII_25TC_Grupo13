using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_Usuario
    {
        public List<RolUsuario> ListarRoles()
        {
            List<RolUsuario> lista = new List<RolUsuario>();
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = "SELECT id_rol_usuario, nombre FROM rol_usuario";
                using (var cmd = new SqlCommand(query, cn))
                {
                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new RolUsuario
                            {
                                IdRolUsuario = Guid.Parse(dr["id_rol_usuario"].ToString()!),
                                Nombre = dr["nombre"].ToString()!
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public bool TieneUsuarios()
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = "SELECT COUNT(*) FROM usuario";
                using (var cmd = new SqlCommand(query, cn))
                {
                    cn.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public (Usuario? usuario, string? errorCode) ValidarUsuario(string dni, string password)
        {
            try
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
                        WHERE p.dni = @dni";

                    using (var cmd = new SqlCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@dni", dni);
                        
                        cn.Open();
                        
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                var estado = dr["estado"]?.ToString() ?? "Activo";
                                var claveHash = dr["clave"]?.ToString();
                                
                                if (string.IsNullOrEmpty(claveHash))
                                {
                                    return (null, "INVALID_PASSWORD");
                                }
                                
                                if (estado != "Activo")
                                {
                                    return (null, "INACTIVE");
                                }
                                
                                if (!BCrypt.Net.BCrypt.Verify(password, claveHash))
                                {
                                    return (null, "INVALID_PASSWORD");
                                }
                                
                                return (new Usuario
                                {
                                    IdUsuario = Guid.Parse(dr["id_usuario"].ToString()!),
                                    Clave = claveHash!,
                                    Estado = estado,
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
                                }, null);
                            }
                        }
                    }
                }
                return (null, "NOT_FOUND");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[SQL Error]: {ex.Message}");
                return (null, "DB_OFFLINE");
            }
            catch (Exception)
            {
                return (null, "ERROR_CONEXION");
            }
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

        public (bool success, string message) Registrar(Persona persona, string password, string rolNombre)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                cn.Open();
                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(persona.Dni))
                        {
                            transaction.Rollback();
                            return (false, "El DNI es requerido");
                        }
                        
                        string checkDniQuery = "SELECT COUNT(*) FROM persona WHERE dni = @dni";
                        using (var cmdCheck = new SqlCommand(checkDniQuery, cn, transaction))
                        {
                            cmdCheck.Parameters.AddWithValue("@dni", persona.Dni);
                            var countDni = (int)cmdCheck.ExecuteScalar();
                            if (countDni > 0)
                            {
                                transaction.Rollback();
                                return (false, "El DNI ya está registrado");
                            }
                        }

                        string checkEmailQuery = "SELECT COUNT(*) FROM persona WHERE email = @email";
                        using (var cmdCheckEmail = new SqlCommand(checkEmailQuery, cn, transaction))
                        {
                            cmdCheckEmail.Parameters.AddWithValue("@email", persona.Email);
                            var countEmail = (int)cmdCheckEmail.ExecuteScalar();
                            if (countEmail > 0)
                            {
                                transaction.Rollback();
                                return (false, "El email ya está registrado");
                            }
                        }

                        var idPersona = Guid.NewGuid();
                        string insertPersona = @"
                            INSERT INTO persona (id_persona, dni, nombre, apellido, email, estado)
                            VALUES (@id, @dni, @nombre, @apellido, @email, 'Activo')";
                        using (var cmdPersona = new SqlCommand(insertPersona, cn, transaction))
                        {
                            cmdPersona.Parameters.AddWithValue("@id", idPersona);
                            cmdPersona.Parameters.AddWithValue("@dni", persona.Dni ?? (object)DBNull.Value);
                            cmdPersona.Parameters.AddWithValue("@nombre", persona.Nombre);
                            cmdPersona.Parameters.AddWithValue("@apellido", persona.Apellido);
                            cmdPersona.Parameters.AddWithValue("@email", persona.Email);
                            cmdPersona.ExecuteNonQuery();
                        }

                        string getRolQuery = "SELECT id_rol_usuario FROM rol_usuario WHERE nombre = @rol";
                        Guid idRolUsuario;
                        using (var cmdRol = new SqlCommand(getRolQuery, cn, transaction))
                        {
                            cmdRol.Parameters.AddWithValue("@rol", rolNombre);
                            var rolResult = cmdRol.ExecuteScalar();
                            if (rolResult == null)
                            {
                                transaction.Rollback();
                                return (false, "El rol especificado no existe");
                            }
                            idRolUsuario = Guid.Parse(rolResult.ToString()!);
                        }

                        var idUsuario = Guid.NewGuid();
                        var claveHash = BCrypt.Net.BCrypt.HashPassword(password);
                        string insertUsuario = @"
                            INSERT INTO usuario (id_usuario, clave, estado, fecha_creacion, id_persona, id_rol_usuario)
                            VALUES (@id, @clave, 'Activo', GETDATE(), @idPersona, @idRol)";
                        using (var cmdUsuario = new SqlCommand(insertUsuario, cn, transaction))
                        {
                            cmdUsuario.Parameters.AddWithValue("@id", idUsuario);
                            cmdUsuario.Parameters.AddWithValue("@clave", claveHash);
                            cmdUsuario.Parameters.AddWithValue("@idPersona", idPersona);
                            cmdUsuario.Parameters.AddWithValue("@idRol", idRolUsuario);
                            cmdUsuario.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return (true, "Usuario registrado correctamente");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return (false, $"Error al registrar: {ex.Message}");
                    }
                }
            }
        }
    }
}