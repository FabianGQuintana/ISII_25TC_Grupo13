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
                cn.Open();
                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        var idDireccion = Guid.NewGuid();

                        string insertDireccion = @"
                            INSERT INTO direccion (id_direccion, calle, altura, id_localidad)
                            VALUES (@idDireccion, @calle, @altura, @idLocalidad)";

                        using (var cmd = new SqlCommand(insertDireccion, cn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@idDireccion", idDireccion);
                            cmd.Parameters.AddWithValue("@calle", obj.ODireccion?.Calle ?? "");
                            cmd.Parameters.AddWithValue("@altura", obj.ODireccion?.Altura ?? "");
                            cmd.Parameters.AddWithValue("@idLocalidad",
                                obj.ODireccion?.IdLocalidad.HasValue == true
                                    ? (object)obj.ODireccion.IdLocalidad.Value
                                    : DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        string insertInmueble = @"
                            INSERT INTO inmueble (id_inmueble, id_direccion, descripcion, estado, fecha_creacion, id_persona_propietario, disponibilidad, id_tipo_inmueble)
                            VALUES (@id, @idDireccion, @descripcion, 'Activo', GETDATE(), @idPropietario, @disponibilidad, @idTipo)";

                        using (var cmd = new SqlCommand(insertInmueble, cn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                            cmd.Parameters.AddWithValue("@idDireccion", idDireccion);
                            cmd.Parameters.AddWithValue("@descripcion", (object?)obj.Descripcion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@idPropietario", obj.IdPersonaPropietario);
                            cmd.Parameters.AddWithValue("@disponibilidad", obj.Disponibilidad);
                            cmd.Parameters.AddWithValue("@idTipo", (object?)(obj.IdTipoInmueble) ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public bool Editar(Inmueble obj)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                cn.Open();
                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        string updateDireccion = @"
                            UPDATE direccion SET
                                calle = @calle,
                                altura = @altura,
                                id_localidad = @idLocalidad
                            WHERE id_direccion = (
                                SELECT id_direccion FROM inmueble WHERE id_inmueble = @idInmueble
                            )";

                        using (var cmd = new SqlCommand(updateDireccion, cn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@idInmueble", obj.IdInmueble);
                            cmd.Parameters.AddWithValue("@calle", obj.ODireccion?.Calle ?? "");
                            cmd.Parameters.AddWithValue("@altura", obj.ODireccion?.Altura ?? "");
                            cmd.Parameters.AddWithValue("@idLocalidad",
                                obj.ODireccion?.IdLocalidad.HasValue == true
                                    ? (object)obj.ODireccion.IdLocalidad.Value
                                    : DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        string updateInmueble = @"
                            UPDATE inmueble SET
                                descripcion = @descripcion,
                                id_persona_propietario = @idPropietario,
                                disponibilidad = @disponibilidad,
                                id_tipo_inmueble = @idTipo
                            WHERE id_inmueble = @id";

                        using (var cmd = new SqlCommand(updateInmueble, cn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", obj.IdInmueble);
                            cmd.Parameters.AddWithValue("@descripcion", (object?)obj.Descripcion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@idPropietario", obj.IdPersonaPropietario);
                            cmd.Parameters.AddWithValue("@disponibilidad", obj.Disponibilidad);
                            cmd.Parameters.AddWithValue("@idTipo", (object?)(obj.IdTipoInmueble) ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
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

        public bool LiberarPorContrato(Guid idContrato)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    UPDATE inmueble 
                    SET disponibilidad = 1 
                    WHERE id_inmueble = (
                        SELECT TOP 1 id_inmueble 
                        FROM contrato_alquiler 
                        WHERE id_contrato_alquiler = @idContrato
                    )";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@idContrato", idContrato);
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
                        d.id_direccion,
                        d.calle,
                        d.altura,
                        d.id_localidad,
                        l.nombre AS localidad_nombre,
                        l.id_provincia,
                        pr.nombre AS provincia_nombre,
                        p.nombre AS propietario_nombre,
                        p.apellido AS propietario_apellido
                    FROM inmueble i
                    INNER JOIN direccion d ON i.id_direccion = d.id_direccion
                    INNER JOIN persona p ON i.id_persona_propietario = p.id_persona
                    LEFT JOIN localidad l ON d.id_localidad = l.id_localidad
                    LEFT JOIN provincia pr ON l.id_provincia = pr.id_provincia
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
                            lista.Add(MapFromReader(dr));
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
                        d.id_direccion,
                        d.calle,
                        d.altura,
                        d.id_localidad,
                        l.nombre AS localidad_nombre,
                        l.id_provincia,
                        pr.nombre AS provincia_nombre,
                        p.nombre AS propietario_nombre,
                        p.apellido AS propietario_apellido
                    FROM inmueble i
                    INNER JOIN direccion d ON i.id_direccion = d.id_direccion
                    INNER JOIN persona p ON i.id_persona_propietario = p.id_persona
                    LEFT JOIN localidad l ON d.id_localidad = l.id_localidad
                    LEFT JOIN provincia pr ON l.id_provincia = pr.id_provincia
                    WHERE i.id_inmueble = @id AND i.estado = 'Activo'";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                            return MapFromReader(dr);
                    }
                }
            }

            return null;
        }

        private static Inmueble MapFromReader(SqlDataReader dr)
        {
            Guid? idLocalidad = dr["id_localidad"] is Guid idLoc ? idLoc : null;
            Guid? idProvincia = dr["id_provincia"] is Guid idProv ? idProv : null;

            Localidad? localidad = null;
            if (idLocalidad.HasValue)
            {
                localidad = new Localidad
                {
                    IdLocalidad = idLocalidad.Value,
                    Nombre = dr["localidad_nombre"]?.ToString() ?? "",
                    IdProvincia = idProvincia ?? Guid.Empty,
                    OProvincia = idProvincia.HasValue ? new Provincia
                    {
                        IdProvincia = idProvincia.Value,
                        Nombre = dr["provincia_nombre"]?.ToString() ?? ""
                    } : null
                };
            }

            return new Inmueble
            {
                IdInmueble = dr["id_inmueble"] is Guid idInm ? idInm : Guid.Empty,
                Descripcion = dr["descripcion"]?.ToString(),
                Estado = dr["estado"]?.ToString() ?? "Activo",
                FechaCreacion = dr["fecha_creacion"] is DateTime fc ? fc : default,
                Disponibilidad = dr["disponibilidad"] is bool disp && disp,
                IdTipoInmueble = dr["id_tipo_inmueble"] is Guid idTipo ? idTipo : null,
                ODireccion = new Direccion
                {
                    IdDireccion = dr["id_direccion"] is Guid idDir ? idDir : Guid.Empty,
                    Calle = dr["calle"]?.ToString() ?? "",
                    Altura = dr["altura"]?.ToString() ?? "",
                    IdLocalidad = idLocalidad,
                    OLocalidad = localidad
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
