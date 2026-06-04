using CapaDatos;
using CapaEntidades;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

public class CD_Pago
{
    private static int EstadoToInt(string estado) => estado switch
    {
        "Pendiente" or "1" => 1,
        "Aprobado" or "2" => 2,
        "Rechazado" or "3" => 3,
        "Anulado" or "4" => 4,
        _ => 1
    };

    public void Insertar(Pago pago)
    {
        try
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
            INSERT INTO pago (id_pago, id_cuota, id_metodo_pago, id_usuario_creador, monto_total, estado, fecha_pago, periodo, mora_cobrada)
            VALUES (@id, @idCuota, @idMetodo, @idUser, @monto, @estado, GETDATE(), @periodo, @mora)";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", pago.IdPago);
                    cmd.Parameters.AddWithValue("@idCuota", pago.IdCuota);
                    cmd.Parameters.AddWithValue("@idMetodo", pago.IdMetodoPago);
                    cmd.Parameters.AddWithValue("@idUser", pago.IdUsuarioCreador);
                    cmd.Parameters.AddWithValue("@monto", pago.MontoTotal);
                    cmd.Parameters.AddWithValue("@estado", EstadoToInt(pago.Estado ?? "Pendiente"));

                    cmd.Parameters.AddWithValue("@periodo", (object)pago.Periodo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@mora", pago.MoraCobrada);

                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (SqlException ex)
        {
            throw new Exception($"Error al insertar el pago: {ex.Message}", ex);
        }
    }

    public bool AprobarPago(Guid idPago, Guid idCuota)
    {
        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            cn.Open();
            using (var transaction = cn.BeginTransaction())
            {
                try
                {
                    // CORRECCIÓN: Query usa valor literal string o parámetro string
                    string q1 = "UPDATE pago SET estado = 2 WHERE id_pago = @id";
                    string q2 = "INSERT INTO recibo (id_recibo, id_pago, fecha_emision, nro_comprobante) VALUES (NEWID(), @id, GETDATE(), 'REC-' + CAST(NEXT VALUE FOR SecuenciaRecibo AS VARCHAR))";
                    string q3 = "UPDATE cuota SET estado = 'Pagada' WHERE id_cuota = @idCuota";

                    using (var cmd1 = new SqlCommand(q1, cn, transaction))
                    {
                        cmd1.Parameters.AddWithValue("@id", idPago);
                        cmd1.ExecuteNonQuery();
                    }

                    new SqlCommand(q2, cn, transaction) { Parameters = { new SqlParameter("@id", idPago) } }.ExecuteNonQuery();
                    new SqlCommand(q3, cn, transaction) { Parameters = { new SqlParameter("@idCuota", idCuota) } }.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch { transaction.Rollback(); return false; }
            }
        }
    }

    public List<Pago> Listar(string? estado)
    {
        var lista = new List<Pago>();
        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            string query = @"
                SELECT p.id_pago, p.id_cuota, p.id_metodo_pago, p.id_usuario_creador, p.monto_total, p.periodo,
                       CASE WHEN pa.id_pago IS NOT NULL THEN 'Anulado' ELSE 'Activo' END AS estado,
                       p.fecha_pago, p.mora_cobrada,
                       c.nro_cuota, c.fecha_vencimiento, c.id_contrato_alquiler,
                       per.nombre + ' ' + per.apellido AS inquilino,
                       ISNULL(d.calle, '') + ' ' + ISNULL(d.altura, '') AS inmueble
                FROM pago p
                INNER JOIN cuota c ON p.id_cuota = c.id_cuota
                INNER JOIN contrato_alquiler ca ON c.id_contrato_alquiler = ca.id_contrato_alquiler
                INNER JOIN persona per ON ca.id_persona_inquilino = per.id_persona
                INNER JOIN inmueble i ON ca.id_inmueble = i.id_inmueble
                LEFT JOIN direccion d ON i.id_direccion = d.id_direccion
                LEFT JOIN pago_anulado pa ON p.id_pago = pa.id_pago";

            if (!string.IsNullOrEmpty(estado))
                query += " WHERE CASE WHEN pa.id_pago IS NOT NULL THEN 'Anulado' ELSE 'Activo' END = @estado";

            using (var cmd = new SqlCommand(query, cn))
            {
                if (!string.IsNullOrEmpty(estado))
                    cmd.Parameters.AddWithValue("@estado", estado);

                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(MapFromReader(dr));
                    }
                }
            }
        }
        return lista;
    }

    private bool HasColumn(SqlDataReader dr, string columnName)
    {
        for (int i = 0; i < dr.FieldCount; i++)
        {
            if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                return true;
        }
        return false;
    }

    // Helper para centralizar el mapeo y evitar errores de lectura
    private Pago MapFromReader(SqlDataReader dr)
    {
        return new Pago
        {
            IdPago = (Guid)dr["id_pago"],
            IdCuota = (Guid)dr["id_cuota"],
            IdMetodoPago = (Guid)dr["id_metodo_pago"],
            IdUsuarioCreador = (Guid)dr["id_usuario_creador"],
            MontoTotal = (decimal)dr["monto_total"],
            Periodo = dr["periodo"]?.ToString() ?? "",
            Estado = dr["estado"]?.ToString() ?? "Pendiente",
            FechaPago = dr["fecha_pago"] != DBNull.Value ? (DateTime)dr["fecha_pago"] : DateTime.MinValue,
            MoraCobrada = (decimal)dr["mora_cobrada"],
            
            NroCuota = HasColumn(dr, "nro_cuota") && dr["nro_cuota"] != DBNull.Value ? (int)dr["nro_cuota"] : 0,
            FechaVencimiento = HasColumn(dr, "fecha_vencimiento") && dr["fecha_vencimiento"] != DBNull.Value ? (DateTime)dr["fecha_vencimiento"] : null,
            IdContrato = HasColumn(dr, "id_contrato_alquiler") && dr["id_contrato_alquiler"] != DBNull.Value ? (Guid)dr["id_contrato_alquiler"] : Guid.Empty,
            Inquilino = HasColumn(dr, "inquilino") && dr["inquilino"] != DBNull.Value ? dr["inquilino"]?.ToString() ?? "" : "",
            Inmueble = HasColumn(dr, "inmueble") && dr["inmueble"] != DBNull.Value ? dr["inmueble"]?.ToString() ?? "" : ""
        };
    }

    public Pago? ObtenerPorId(Guid id)
    {
        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            string query = @"
                SELECT p.id_pago, p.id_cuota, p.id_metodo_pago, p.id_usuario_creador, p.monto_total, p.periodo,
                       CASE WHEN pa.id_pago IS NOT NULL THEN 'Anulado' ELSE 'Activo' END AS estado,
                       p.fecha_pago, p.mora_cobrada,
                       c.nro_cuota, c.fecha_vencimiento, c.id_contrato_alquiler,
                       per.nombre + ' ' + per.apellido AS inquilino,
                       ISNULL(d.calle, '') + ' ' + ISNULL(d.altura, '') AS inmueble
                FROM pago p
                INNER JOIN cuota c ON p.id_cuota = c.id_cuota
                INNER JOIN contrato_alquiler ca ON c.id_contrato_alquiler = ca.id_contrato_alquiler
                INNER JOIN persona per ON ca.id_persona_inquilino = per.id_persona
                INNER JOIN inmueble i ON ca.id_inmueble = i.id_inmueble
                LEFT JOIN direccion d ON i.id_direccion = d.id_direccion
                LEFT JOIN pago_anulado pa ON p.id_pago = pa.id_pago
                WHERE p.id_pago = @id";

            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return MapFromReader(dr);
                    }
                }
            }
        }
        return null;
    }

    public List<Pago> ListarPorContrato(Guid contratoId)
    {
        var lista = new List<Pago>();
        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            string query = @"
                SELECT p.id_pago, p.id_cuota, p.id_metodo_pago, p.id_usuario_creador, p.monto_total, p.periodo,
                       CASE WHEN pa.id_pago IS NOT NULL THEN 'Anulado' ELSE 'Activo' END AS estado,
                       p.fecha_pago, p.mora_cobrada,
                       c.nro_cuota, c.fecha_vencimiento, c.id_contrato_alquiler,
                       per.nombre + ' ' + per.apellido AS inquilino,
                       ISNULL(d.calle, '') + ' ' + ISNULL(d.altura, '') AS inmueble
                FROM pago p
                INNER JOIN cuota c ON p.id_cuota = c.id_cuota
                INNER JOIN contrato_alquiler ca ON c.id_contrato_alquiler = ca.id_contrato_alquiler
                INNER JOIN persona per ON ca.id_persona_inquilino = per.id_persona
                INNER JOIN inmueble i ON ca.id_inmueble = i.id_inmueble
                LEFT JOIN direccion d ON i.id_direccion = d.id_direccion
                LEFT JOIN pago_anulado pa ON p.id_pago = pa.id_pago
                WHERE c.id_contrato_alquiler = @contratoId";

            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@contratoId", contratoId);
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(MapFromReader(dr));
                    }
                }
            }
        }
        return lista;
    }

    public List<MetodoPago> ListarMetodosPagos()
    {
        var lista = new List<MetodoPago>();

        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            string query = "SELECT id_metodo_pago, tipo_pago, descripcion FROM metodo_pago";

            using (var cmd = new SqlCommand(query, cn))
            {
                cn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new MetodoPago
                        {
                            IdMetodoPago = (Guid)dr["id_metodo_pago"],
                            Nombre = dr["tipo_pago"]?.ToString() ?? "",
                            Descripcion = dr["descripcion"]?.ToString()
                        });
                    }
                }
            }
        }

        return lista;
    }

    public bool Rechazar(Guid idPago, string? motivo)
    {
        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            string query = "UPDATE pago SET estado = 3 WHERE id_pago = @id";
            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@id", idPago);
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }

    public bool Anular(Pago pago, Guid idUsuario, string? motivo)
    {
        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            cn.Open();
            using (var transaction = cn.BeginTransaction())
            {
                try
                {
                    string q1 = "UPDATE pago SET estado = 4 WHERE id_pago = @idPago";
                    using (var cmd1 = new SqlCommand(q1, cn, transaction))
                    {
                        cmd1.Parameters.AddWithValue("@idPago", pago.IdPago);
                        cmd1.ExecuteNonQuery();
                    }

                    string q2 = @"INSERT INTO pago_anulado (id_pago_anulado, id_pago, fecha_anulacion, id_usuario_anulacion, monto_total_anulado, motivo) 
                                  VALUES (NEWID(), @idPago, GETDATE(), @idUsuario, @montoTotal, @motivo)";
                    using (var cmd2 = new SqlCommand(q2, cn, transaction))
                    {
                        cmd2.Parameters.AddWithValue("@idPago", pago.IdPago);
                        cmd2.Parameters.AddWithValue("@idUsuario", idUsuario);
                        cmd2.Parameters.AddWithValue("@montoTotal", pago.MontoTotal);
                        cmd2.Parameters.AddWithValue("@motivo", (object?)motivo ?? DBNull.Value);
                        cmd2.ExecuteNonQuery();
                    }

                    string q3 = "UPDATE cuota SET estado = 'Pendiente' WHERE id_cuota = @idCuota";
                    using (var cmd3 = new SqlCommand(q3, cn, transaction))
                    {
                        cmd3.Parameters.AddWithValue("@idCuota", pago.IdCuota);
                        cmd3.ExecuteNonQuery();
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

    public bool CrearPagoAnulado(PagoAnulado entidad)
    {
        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            cn.Open();
            using (var transaction = cn.BeginTransaction())
            {
                try
                {
                    string q1 = @"INSERT INTO pago_anulado (id_pago_anulado, id_pago, fecha_anulacion, id_usuario_anulacion, monto_total_anulado, motivo) 
                                  VALUES (@idPagoAnulado, @idPago, @fechaAnulacion, @idUsuario, @montoTotal, @motivo)";
                    using (var cmd1 = new SqlCommand(q1, cn, transaction))
                    {
                        cmd1.Parameters.AddWithValue("@idPagoAnulado", entidad.IdPagoAnulado);
                        cmd1.Parameters.AddWithValue("@idPago", entidad.IdPago);
                        cmd1.Parameters.AddWithValue("@fechaAnulacion", entidad.FechaAnulacion);
                        cmd1.Parameters.AddWithValue("@idUsuario", entidad.IdUsuarioAnulacion);
                        cmd1.Parameters.AddWithValue("@montoTotal", entidad.MontoTotalAnulado);
                        cmd1.Parameters.AddWithValue("@motivo", (object?)entidad.Motivo ?? DBNull.Value);
                        cmd1.ExecuteNonQuery();
                    }

                    string q2 = "UPDATE cuota SET estado = 'Pendiente' WHERE id_cuota = (SELECT id_cuota FROM pago WHERE id_pago = @idPago)";
                    using (var cmd2 = new SqlCommand(q2, cn, transaction))
                    {
                        cmd2.Parameters.AddWithValue("@idPago", entidad.IdPago);
                        cmd2.ExecuteNonQuery();
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
}