using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_Inmueble
    {
        private readonly CD_Direccion _cdDireccion = new();

        public bool RegistrarInmueble(Inmueble obj)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                cn.Open();
                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        var idDireccion = Guid.NewGuid();
                        _cdDireccion.AgregarDireccion(idDireccion, obj.ODireccion!, cn, transaction);

                        string insertInmueble = @"
                            INSERT INTO inmueble (id_inmueble, id_direccion, descripcion, estado, fecha_creacion, id_persona_propietario, id_rol_cliente_propietario, disponibilidad, id_tipo_inmueble)
                            VALUES (@id, @idDireccion, @descripcion, 'Activo', GETDATE(), @idPropietario, @idRolPropietario, @disponibilidad, @idTipo)";

                        using (var cmd = new SqlCommand(insertInmueble, cn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                            cmd.Parameters.AddWithValue("@idDireccion", idDireccion);
                            cmd.Parameters.AddWithValue("@descripcion", (object?)obj.Descripcion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@idPropietario", obj.OPropietario!.IdPersona);
                            cmd.Parameters.AddWithValue("@idRolPropietario", obj.OPropietario!.IdRolCliente);
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

        public bool EditarInmueble(Inmueble obj)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                cn.Open();
                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        _cdDireccion.EditarDireccion(obj.IdInmueble, obj.ODireccion!, cn, transaction);

                        string updateInmueble = @"
                            UPDATE inmueble SET
                                descripcion = @descripcion,
                                id_persona_propietario = @idPropietario,
                                id_rol_cliente_propietario = @idRolPropietario,
                                disponibilidad = @disponibilidad,
                                id_tipo_inmueble = @idTipo
                            WHERE id_inmueble = @id";

                        using (var cmd = new SqlCommand(updateInmueble, cn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", obj.IdInmueble);
                            cmd.Parameters.AddWithValue("@descripcion", (object?)obj.Descripcion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@idPropietario", obj.OPropietario!.IdPersona);
                            cmd.Parameters.AddWithValue("@idRolPropietario", obj.OPropietario!.IdRolCliente);
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

        public bool EliminarInmueble(Guid id)
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
                        i.id_persona_propietario,
                        i.id_rol_cliente_propietario,
                        d.id_direccion,
                        d.calle,
                        d.altura,
                        d.id_localidad,
                        l.nombre AS localidad_nombre,
                        l.id_provincia,
                        pr.nombre AS provincia_nombre,
                        p.nombre AS propietario_nombre,
                        p.apellido AS propietario_apellido,
                        rc.nombre AS propietario_rol_nombre
                    FROM inmueble i
                    INNER JOIN direccion d ON i.id_direccion = d.id_direccion
                    INNER JOIN persona p ON i.id_persona_propietario = p.id_persona
                    INNER JOIN rol_cliente rc ON i.id_rol_cliente_propietario = rc.id_rol_cliente
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
                        i.id_persona_propietario,
                        i.id_rol_cliente_propietario,
                        d.id_direccion,
                        d.calle,
                        d.altura,
                        d.id_localidad,
                        l.nombre AS localidad_nombre,
                        l.id_provincia,
                        pr.nombre AS provincia_nombre,
                        p.nombre AS propietario_nombre,
                        p.apellido AS propietario_apellido,
                        rc.nombre AS propietario_rol_nombre
                    FROM inmueble i
                    INNER JOIN direccion d ON i.id_direccion = d.id_direccion
                    INNER JOIN persona p ON i.id_persona_propietario = p.id_persona
                    INNER JOIN rol_cliente rc ON i.id_rol_cliente_propietario = rc.id_rol_cliente
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
            var idLocalidad = dr["id_localidad"] != DBNull.Value
                ? Guid.Parse(dr["id_localidad"].ToString()!)
                : (Guid?)null;

            var idProvincia = dr["id_provincia"] != DBNull.Value
                ? Guid.Parse(dr["id_provincia"].ToString()!)
                : (Guid?)null;

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
                    IdDireccion = Guid.Parse(dr["id_direccion"].ToString()!),
                    Calle = dr["calle"]?.ToString() ?? "",
                    Altura = dr["altura"]?.ToString() ?? "",
                    IdLocalidad = idLocalidad,
                    OLocalidad = localidad
                },
                OPropietario = new Propietario
                {
                    IdPersona = Guid.Parse(dr["id_persona_propietario"].ToString()!),
                    IdRolCliente = Guid.Parse(dr["id_rol_cliente_propietario"].ToString()!),
                    OPersona = new Persona
                    {
                        Nombre = dr["propietario_nombre"]?.ToString() ?? "",
                        Apellido = dr["propietario_apellido"]?.ToString() ?? ""
                    },
                    ORolCliente = new RolCliente
                    {
                        IdRolCliente = Guid.Parse(dr["id_rol_cliente_propietario"].ToString()!),
                        Nombre = dr["propietario_rol_nombre"]?.ToString() ?? ""
                    }
                }
            };
        }
    }
}
