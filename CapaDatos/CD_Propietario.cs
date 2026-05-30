using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_Propietario
    {
        public List<Propietario> ListarPropietarios()
        {
            var lista = new List<Propietario>();

            using var cn = new SqlConnection(Conexion.Cadena);
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
                WHERE p.estado = 'Activo' AND rc.nombre = 'Propietario'
                ORDER BY p.apellido, p.nombre";

            using var cmd = new SqlCommand(query, cn);
            cn.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new Propietario
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
                });
            }

            return lista;
        }

        public Propietario? ObtenerPorId(Guid id)
        {
            using var cn = new SqlConnection(Conexion.Cadena);
            string query = @"
                SELECT
                    p.id_persona, p.dni, p.nombre, p.apellido, p.email,
                    p.telefono, p.estado, p.fecha_nacimiento,
                    prc.id_rol_cliente, prc.estado AS rol_estado
                FROM persona p
                INNER JOIN persona_rol_cliente prc ON p.id_persona = prc.id_persona
                INNER JOIN rol_cliente rc ON prc.id_rol_cliente = rc.id_rol_cliente
                WHERE p.id_persona = @id AND rc.nombre = 'Propietario' AND p.estado = 'Activo'";

            using var cmd = new SqlCommand(query, cn);
            cmd.Parameters.AddWithValue("@id", id);
            cn.Open();
            using var dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                return new Propietario
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

            return null;
        }
    }
}
