using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_Inmueble
    {
        public bool Registrar(Inmueble obj)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"INSERT INTO inmueble (id_inmueble, id_direccion, descripcion, estado, fecha_creacion, id_persona_propietario, disponibilidad, id_tipo_inmueble) 
                         VALUES (@id, @idDireccion, @descripcion, 'Activo', GETDATE(), @idPropietario, @disponibilidad, @idTipo)";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("@idDireccion", obj.IdDireccion);
                    cmd.Parameters.AddWithValue("@descripcion", (object?)obj.Descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@idPropietario", obj.IdPersonaPropietario);
                    cmd.Parameters.AddWithValue("@disponibilidad", obj.Disponibilidad);
                    cmd.Parameters.AddWithValue("@idTipo", (object)(obj.IdTipoInmueble ?? (object)DBNull.Value));

                    cn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Editar(Inmueble obj)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"UPDATE inmueble SET id_direccion = @idDireccion, descripcion = @descripcion,
                         id_persona_propietario = @idPropietario, disponibilidad = @disponibilidad, id_tipo_inmueble = @idTipo
                         WHERE id_inmueble = @id";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", obj.IdInmueble);
                    cmd.Parameters.AddWithValue("@idDireccion", obj.IdDireccion);
                    cmd.Parameters.AddWithValue("@descripcion", (object?)obj.Descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@idPropietario", obj.IdPersonaPropietario);
                    cmd.Parameters.AddWithValue("@disponibilidad", obj.Disponibilidad);
                    cmd.Parameters.AddWithValue("@idTipo", (object)(obj.IdTipoInmueble ?? (object)DBNull.Value));

                    cn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Eliminar(Guid id)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = "UPDATE inmueble SET estado = 'Inactivo' WHERE id_inmueble = @id";
                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }




        public List<Inmueble> ListarInmuebles(bool soloDisponibles = false)
        {
            var lista = new List<Inmueble>();

            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT 
                        i.id_inmueble,
                        i.descripcion,
                        i.estado,
                        i.fecha_creacion,
                        i.disponibilidad,
                        i.id_tipo_inmueble,
                        d.calle + ' ' + d.altura AS direccion,
                        p.nombre AS propietario_nombre,
                        p.apellido AS propietario_apellido,
                        ti.descripcion AS tipo_descripcion
                    FROM inmueble i
                    INNER JOIN direccion d ON i.id_direccion = d.id_direccion
                    INNER JOIN persona p ON i.id_persona_propietario = p.id_persona
                    LEFT JOIN tipo_inmueble ti ON i.id_tipo_inmueble = ti.id_tipo_inmueble
                    WHERE i.estado = 'Activo'";

                if (soloDisponibles)
                {
                    query += " AND i.disponibilidad = 1";
                    query += " AND NOT EXISTS (SELECT 1 FROM contrato_alquiler c WHERE c.id_inmueble = i.id_inmueble AND c.estado = 'Activo')";
                }

                query += " ORDER BY d.calle, d.altura";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Inmueble
                            {
                                IdInmueble = Guid.Parse(dr["id_inmueble"].ToString()!),
                                Descripcion = dr["descripcion"]?.ToString(),
                                Estado = dr["estado"]?.ToString() ?? "Activo",
                                FechaCreacion = DateTime.Parse(dr["fecha_creacion"].ToString()!),
                                Disponibilidad = dr["disponibilidad"] != DBNull.Value && Convert.ToBoolean(dr["disponibilidad"]),
                                IdTipoInmueble = dr["id_tipo_inmueble"] != DBNull.Value
                                    ? Guid.Parse(dr["id_tipo_inmueble"].ToString()!)
                                    : null,
                                ODireccion = new Direccion
                                {
                                    Calle = dr["direccion"]?.ToString() ?? ""
                                },
                                OPropietario = new Persona
                                {
                                    Nombre = dr["propietario_nombre"]?.ToString() ?? "",
                                    Apellido = dr["propietario_apellido"]?.ToString() ?? ""
                                }
                            });
                        }
                    }
                }
            }

            return lista;
        }

        public Inmueble? ObtenerPorId(Guid id)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT 
                        i.id_inmueble,
                        i.descripcion,
                        i.estado,
                        i.fecha_creacion,
                        i.disponibilidad,
                        i.id_tipo_inmueble,
                        i.id_direccion,
                        d.calle + ' ' + d.altura AS direccion,
                        p.nombre AS propietario_nombre,
                        p.apellido AS propietario_apellido,
                        ti.descripcion AS tipo_descripcion
                    FROM inmueble i
                    INNER JOIN direccion d ON i.id_direccion = d.id_direccion
                    INNER JOIN persona p ON i.id_persona_propietario = p.id_persona
                    LEFT JOIN tipo_inmueble ti ON i.id_tipo_inmueble = ti.id_tipo_inmueble
                    WHERE i.id_inmueble = @id AND i.estado = 'Activo'";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new Inmueble
                            {
                                IdInmueble = Guid.Parse(dr["id_inmueble"].ToString()!),
                                Descripcion = dr["descripcion"]?.ToString(),
                                Estado = dr["estado"]?.ToString() ?? "Activo",
                                FechaCreacion = DateTime.Parse(dr["fecha_creacion"].ToString()!),
                                Disponibilidad = dr["disponibilidad"] != DBNull.Value && Convert.ToBoolean(dr["disponibilidad"]),
                                IdTipoInmueble = dr["id_tipo_inmueble"] != DBNull.Value
                                    ? Guid.Parse(dr["id_tipo_inmueble"].ToString()!)
                                    : null,
                                ODireccion = new Direccion
                                {
                                    Calle = dr["direccion"]?.ToString() ?? ""
                                },
                                OPropietario = new Persona
                                {
                                    Nombre = dr["propietario_nombre"]?.ToString() ?? "",
                                    Apellido = dr["propietario_apellido"]?.ToString() ?? ""
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