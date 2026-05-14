using CapaDatos;
using CapaEntidades;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

public class CD_Pago
{
    public void Insertar(Pago pago)
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
                // CORRECCIÓN: Se envía el string directamente
                cmd.Parameters.AddWithValue("@estado", pago.Estado ?? "Pendiente");

                cmd.Parameters.AddWithValue("@periodo", (object)pago.Periodo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@mora", pago.MoraCobrada);

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
                    // CORRECCIÓN: Query usa valor literal string o parámetro string
                    string q1 = "UPDATE pago SET estado = 'Aprobado' WHERE id_pago = @id";
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

    public List<Pago> Listar(string? estado) // CORRECCIÓN: Parámetro string
    {
        var lista = new List<Pago>();
        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            string query = @"
                SELECT p.id_pago, p.id_cuota, p.id_metodo_pago, p.id_usuario_creador, p.monto_total, p.periodo, p.estado, p.fecha_pago, p.mora_cobrada,
                       c.nro_cuota, c.fecha_vencimiento, c.id_contrato_alquiler,
                       per.nombre + ' ' + per.apellido AS inquilino,
                       ISNULL(d.calle, '') + ' ' + ISNULL(d.altura, '') AS inmueble
                FROM pago p
                INNER JOIN cuota c ON p.id_cuota = c.id_cuota
                INNER JOIN contrato_alquiler ca ON c.id_contrato_alquiler = ca.id_contrato_alquiler
                INNER JOIN persona per ON ca.id_persona_inquilino = per.id_persona
                INNER JOIN inmueble i ON ca.id_inmueble = i.id_inmueble
                LEFT JOIN direccion d ON i.id_direccion = d.id_direccion";

            if (!string.IsNullOrEmpty(estado))
                query += " WHERE p.estado = @estado";

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
                SELECT p.id_pago, p.id_cuota, p.id_metodo_pago, p.id_usuario_creador, p.monto_total, p.periodo, p.estado, p.fecha_pago, p.mora_cobrada,
                       c.nro_cuota, c.fecha_vencimiento, c.id_contrato_alquiler,
                       per.nombre + ' ' + per.apellido AS inquilino,
                       ISNULL(d.calle, '') + ' ' + ISNULL(d.altura, '') AS inmueble
                FROM pago p
                INNER JOIN cuota c ON p.id_cuota = c.id_cuota
                INNER JOIN contrato_alquiler ca ON c.id_contrato_alquiler = ca.id_contrato_alquiler
                INNER JOIN persona per ON ca.id_persona_inquilino = per.id_persona
                INNER JOIN inmueble i ON ca.id_inmueble = i.id_inmueble
                LEFT JOIN direccion d ON i.id_direccion = d.id_direccion
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
                SELECT p.id_pago, p.id_cuota, p.id_metodo_pago, p.id_usuario_creador, p.monto_total, p.periodo, p.estado, p.fecha_pago, p.mora_cobrada,
                       c.nro_cuota, c.fecha_vencimiento, c.id_contrato_alquiler,
                       per.nombre + ' ' + per.apellido AS inquilino,
                       ISNULL(d.calle, '') + ' ' + ISNULL(d.altura, '') AS inmueble
                FROM pago p
                INNER JOIN cuota c ON p.id_cuota = c.id_cuota
                INNER JOIN contrato_alquiler ca ON c.id_contrato_alquiler = ca.id_contrato_alquiler
                INNER JOIN persona per ON ca.id_persona_inquilino = per.id_persona
                INNER JOIN inmueble i ON ca.id_inmueble = i.id_inmueble
                LEFT JOIN direccion d ON i.id_direccion = d.id_direccion
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

    public List<ContratoAlquiler> ListarActivosPorInquilino(Guid idInquilino)
    {
        var lista = new List<ContratoAlquiler>();

        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            string query = @"
            SELECT 
                c.id_contrato_alquiler,
                c.fecha_fin,
                c.condiciones,
                c.cantidad_cuotas,
                c.precio_cuota,
                c.fecha_creacion,
                c.id_inmueble,
                c.id_persona_inquilino,
                c.id_rol_cliente_inquilino,
                c.tasa_mora_mensual,
                c.estado,
                c.id_usuario_creador,
                c.frecuencia_ajuste,
                c.id_tipo_indice,
                c.valor_indice_inicio,
                i.descripcion AS inmueble_descripcion,
                d.calle + ' ' + d.altura AS direccion,
                p.nombre AS inquilino_nombre,
                p.apellido AS inquilino_apellido,
                p.dni AS inquilino_dni
            FROM contrato_alquiler c
            INNER JOIN inmueble i ON c.id_inmueble = i.id_inmueble
            INNER JOIN direccion d ON i.id_direccion = d.id_direccion
            INNER JOIN persona p ON c.id_persona_inquilino = p.id_persona
            WHERE c.estado = 'Activo'
            AND c.id_persona_inquilino = @idInquilino
            ORDER BY c.fecha_creacion DESC";

            using (var cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@idInquilino", idInquilino);

                cn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new ContratoAlquiler
                        {
                            IdContratoAlquiler = Guid.Parse(dr["id_contrato_alquiler"].ToString()!),
                            FechaFin = DateTime.Parse(dr["fecha_fin"].ToString()!),
                            Condiciones = dr["condiciones"]?.ToString(),
                            CantidadCuotas = int.Parse(dr["cantidad_cuotas"].ToString()!),
                            PrecioCuota = decimal.Parse(dr["precio_cuota"].ToString()!),
                            FechaInicio = DateTime.Parse(dr["fecha_creacion"].ToString()!),
                            IdInmueble = Guid.Parse(dr["id_inmueble"].ToString()!),
                            IdPersonaInquilino = Guid.Parse(dr["id_persona_inquilino"].ToString()!),
                            IdRolClienteInquilino = Guid.Parse(dr["id_rol_cliente_inquilino"].ToString()!),
                            TasaMoraMensual = dr["tasa_mora_mensual"] != DBNull.Value
                                ? decimal.Parse(dr["tasa_mora_mensual"].ToString()!)
                                : 0m,
                            Estado = dr["estado"]?.ToString() ?? "Activo",
                            IdUsuarioCreador = Guid.Parse(dr["id_usuario_creador"].ToString()!),
                            FrecuenciaAjuste = dr["frecuencia_ajuste"]?.ToString(),
                            IdTipoIndice = dr["id_tipo_indice"] != DBNull.Value
                                ? Guid.Parse(dr["id_tipo_indice"].ToString()!)
                                : null,
                            ValorIndiceInicio = dr["valor_indice_inicio"] != DBNull.Value
                                ? decimal.Parse(dr["valor_indice_inicio"].ToString()!)
                                : null,
                            OInmueble = new Inmueble
                            {
                                IdInmueble = Guid.Parse(dr["id_inmueble"].ToString()!),
                                Descripcion = dr["inmueble_descripcion"]?.ToString(),
                                ODireccion = new Direccion
                                {
                                    Calle = dr["direccion"]?.ToString() ?? ""
                                }
                            },
                            OInquilino = new Persona
                            {
                                IdPersona = Guid.Parse(dr["id_persona_inquilino"].ToString()!),
                                Nombre = dr["inquilino_nombre"]?.ToString() ?? "",
                                Apellido = dr["inquilino_apellido"]?.ToString() ?? "",
                                Dni = dr["inquilino_dni"]?.ToString()
                            }
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

    public bool Anular(Pago pago, Guid idUsuario, string? motivo)
    {
        using (var cn = new SqlConnection(Conexion.Cadena))
        {
            cn.Open();
            using (var transaction = cn.BeginTransaction())
            {
                try
                {
                    string q1 = "UPDATE pago SET estado = 'Anulado' WHERE id_pago = @idPago";
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
}