using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_Inquilino
    {
        public List<Inquilino> ListarInquilinos()
        {
            var lista = new List<Inquilino>();

            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT DISTINCT
                        p.id_persona,
                        p.dni,
                        p.nombre,
                        p.apellido,
                        p.email,
                        p.telefono,
                        p.estado,
                        p.fecha_nacimiento,
                        prc.id_rol_cliente,
                        prc.estado AS rol_estado
                    FROM persona p
                    INNER JOIN persona_rol_cliente prc ON p.id_persona = prc.id_persona
                    INNER JOIN rol_cliente rc ON prc.id_rol_cliente = rc.id_rol_cliente
                    WHERE p.estado = 'Activo' AND rc.nombre = 'Inquilino'
                    ORDER BY p.apellido, p.nombre";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                            lista.Add(MapInquilino(dr));
                    }
                }
            }

            return lista;
        }

        public List<Inquilino> ListarConContratosActivos()
        {
            var lista = new List<Inquilino>();

            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT DISTINCT
                        p.id_persona,
                        p.dni,
                        p.nombre,
                        p.apellido,
                        p.email,
                        p.telefono,
                        p.estado,
                        p.fecha_nacimiento,
                        prc.id_rol_cliente,
                        prc.estado AS rol_estado
                    FROM persona p
                    INNER JOIN persona_rol_cliente prc ON p.id_persona = prc.id_persona
                    INNER JOIN rol_cliente rc ON prc.id_rol_cliente = rc.id_rol_cliente
                    INNER JOIN contrato_alquiler c ON p.id_persona = c.id_persona_inquilino
                    WHERE p.estado = 'Activo' AND c.estado = 'Activo' AND rc.nombre = 'Inquilino'
                    ORDER BY p.apellido, p.nombre";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                            lista.Add(MapInquilino(dr));
                    }
                }
            }

            return lista;
        }

        private static Inquilino MapInquilino(SqlDataReader dr)
        {
            return new Inquilino
            {
                IdPersona = Guid.Parse(dr["id_persona"].ToString()!),
                IdRolCliente = Guid.Parse(dr["id_rol_cliente"].ToString()!),
                Estado = Convert.ToBoolean(dr["rol_estado"]),
                OPersona = new Persona
                {
                    IdPersona = Guid.Parse(dr["id_persona"].ToString()!),
                    Dni = dr["dni"]?.ToString(),
                    Nombre = dr["nombre"]?.ToString() ?? "",
                    Apellido = dr["apellido"]?.ToString() ?? "",
                    Email = dr["email"]?.ToString() ?? "",
                    Telefono = dr["telefono"]?.ToString(),
                    Estado = dr["estado"]?.ToString() ?? "Activo",
                    FechaNacimiento = dr["fecha_nacimiento"] != DBNull.Value
                        ? DateTime.Parse(dr["fecha_nacimiento"].ToString()!)
                        : null
                }
            };
        }

        public Persona? ObtenerPorDni(string dni)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT 
                        id_persona,
                        dni,
                        nombre,
                        apellido,
                        email,
                        telefono,
                        estado,
                        fecha_nacimiento
                    FROM persona 
                    WHERE dni = @dni AND estado = 'Activo'";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@dni", dni);
                    cn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new Persona
                            {
                                IdPersona = Guid.Parse(dr["id_persona"].ToString()!),
                                Dni = dr["dni"]?.ToString(),
                                Nombre = dr["nombre"]?.ToString() ?? "",
                                Apellido = dr["apellido"]?.ToString() ?? "",
                                Email = dr["email"]?.ToString() ?? "",
                                Telefono = dr["telefono"]?.ToString(),
                                Estado = dr["estado"]?.ToString() ?? "Activo",
                                FechaNacimiento = dr["fecha_nacimiento"] != DBNull.Value
                                    ? DateTime.Parse(dr["fecha_nacimiento"].ToString()!)
                                    : null
                            };
                        }
                    }
                }
            }

            return null;
        }

        public Persona? ObtenerPorId(Guid id)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT 
                        id_persona,
                        dni,
                        nombre,
                        apellido,
                        email,
                        telefono,
                        estado,
                        fecha_nacimiento
                    FROM persona 
                    WHERE id_persona = @id AND estado = 'Activo'";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new Persona
                            {
                                IdPersona = Guid.Parse(dr["id_persona"].ToString()!),
                                Dni = dr["dni"]?.ToString(),
                                Nombre = dr["nombre"]?.ToString() ?? "",
                                Apellido = dr["apellido"]?.ToString() ?? "",
                                Email = dr["email"]?.ToString() ?? "",
                                Telefono = dr["telefono"]?.ToString(),
                                Estado = dr["estado"]?.ToString() ?? "Activo",
                                FechaNacimiento = dr["fecha_nacimiento"] != DBNull.Value
                                    ? DateTime.Parse(dr["fecha_nacimiento"].ToString()!)
                                    : null
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}