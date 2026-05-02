using CapaDatos;
using CapaEntidades;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

public class CD_Pago
{
    // 1. Insertar el pago (Estado depende del rol)
    public void Insertar(Pago pago)
    {
        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            string query = @"INSERT INTO pago (id_pago, id_cuota, id_metodo_pago, id_usuario_creador, monto_total, estado, fecha_creacion)
                             VALUES (@id, @idCuota, @idMetodo, @idUser, @monto, @estado, GETDATE())";

            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@id", pago.IdPago);
                cmd.Parameters.AddWithValue("@idCuota", pago.IdCuota);
                cmd.Parameters.AddWithValue("@idMetodo", pago.IdMetodoPago);
                cmd.Parameters.AddWithValue("@idUser", pago.IdUsuarioCreador);
                cmd.Parameters.AddWithValue("@monto", pago.MontoTotal);
                cmd.Parameters.AddWithValue("@estado", pago.Estado);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
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
                    // Cambiar estado a Aprobado
                    string q1 = "UPDATE pago SET estado = 'Aprobado' WHERE id_pago = @id";
                    // Crear Recibo
                    string q2 = "INSERT INTO recibo (id_recibo, id_pago, fecha_emision, nro_comprobante) VALUES (NEWID(), @id, GETDATE(), 'REC-' + CAST(NEXT VALUE FOR SecuenciaRecibo AS VARCHAR))";
                    // Marcar Cuota como Pagada
                    string q3 = "UPDATE cuota SET estado = 'Pagada' WHERE id_cuota = @idCuota";

                    new SqlCommand(q1, cn, transaction) { Parameters = { new SqlParameter("@id", idPago) } }.ExecuteNonQuery();
                    new SqlCommand(q2, cn, transaction) { Parameters = { new SqlParameter("@id", idPago) } }.ExecuteNonQuery();
                    new SqlCommand(q3, cn, transaction) { Parameters = { new SqlParameter("@idCuota", idCuota) } }.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch { transaction.Rollback(); return false; }
            }
        }
    }

    public List<Pago> Listar(int? estado)
    {
        var lista = new List<Pago>();
        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            string query = @"SELECT id_pago, id_cuota, id_metodo_pago, id_usuario_creador, 
                         monto_total, periodo, estado, fecha_pago, mora_cobrada
                         FROM pago";

            if (estado.HasValue)
            {
                query += " WHERE estado = @estado";
            }

            using (var cmd = new SqlCommand(query, cn))
            {
                if (estado.HasValue)
                    cmd.Parameters.AddWithValue("@estado", estado.Value);

                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Pago
                        {
                            IdPago = Guid.Parse(dr["id_pago"].ToString()!),
                            IdCuota = Guid.Parse(dr["id_cuota"].ToString()!),
                            IdMetodoPago = Guid.Parse(dr["id_metodo_pago"].ToString()!),
                            IdUsuarioCreador = Guid.Parse(dr["id_usuario_creador"].ToString()!),
                            MontoTotal = decimal.Parse(dr["monto_total"].ToString()!),
                            Periodo = dr["periodo"]?.ToString() ?? "",
                            Estado = dr["estado"].ToString()!, 
                            FechaPago = DateTime.Parse(dr["fecha_pago"].ToString()!)
                        });
                    }
                }
            }
        }
        return lista;
    }

    public List<Pago> ListarPorContrato(Guid contratoId)
    {
        var lista = new List<Pago>();
        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            string query = @"SELECT p.id_pago, p.id_cuota, p.id_metodo_pago, p.id_usuario_creador, 
                         p.monto_total, p.periodo, p.estado, p.fecha_pago, p.mora_cobrada
                         FROM pago p
                         INNER JOIN cuota c ON p.id_cuota = c.id_cuota
                         WHERE c.id_contrato_alquiler = @contratoId";

            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@contratoId", contratoId);
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Pago
                        {
                            IdPago = Guid.Parse(dr["id_pago"].ToString()!),
                            IdCuota = Guid.Parse(dr["id_cuota"].ToString()!),
                            IdMetodoPago = Guid.Parse(dr["id_metodo_pago"].ToString()!),
                            IdUsuarioCreador = Guid.Parse(dr["id_usuario_creador"].ToString()!),
                            MontoTotal = decimal.Parse(dr["monto_total"].ToString()!),
                            Periodo = dr["periodo"]?.ToString() ?? "",
                            Estado = dr["estado"].ToString()!,
                            FechaPago = DateTime.Parse(dr["fecha_pago"].ToString()!)
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
            string query = "UPDATE pago SET estado = 'Rechazado' WHERE id_pago = @id";
            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@id", idPago);
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }

    public bool Anular(Guid idPago)
    {
        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            string query = "UPDATE pago SET estado = 'Anulado' WHERE id_pago = @id";
            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@id", idPago);
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
    public Pago? ObtenerPorId(Guid id)
    {
        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            string query = @"SELECT id_pago, id_cuota, id_metodo_pago, id_usuario_creador, 
                         monto_total, periodo, estado, fecha_pago, mora_cobrada
                         FROM pago WHERE id_pago = @id";

            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        return new Pago
                        {
                            IdPago = Guid.Parse(dr["id_pago"].ToString()!),
                            IdCuota = Guid.Parse(dr["id_cuota"].ToString()!),
                            IdMetodoPago = Guid.Parse(dr["id_metodo_pago"].ToString()!),
                            IdUsuarioCreador = Guid.Parse(dr["id_usuario_creador"].ToString()!),
                            MontoTotal = decimal.Parse(dr["monto_total"].ToString()!),
                            Periodo = dr["periodo"]?.ToString() ?? "",
                            Estado = dr["estado"].ToString()!,
                            FechaPago = DateTime.Parse(dr["fecha_pago"].ToString()!)
                        };
                    }
                }
            }
        }
        return null;
    }



}