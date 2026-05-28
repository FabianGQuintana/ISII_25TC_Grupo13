using System;
using System.Collections.Generic;
using CapaEntidades;
using Microsoft.Data.SqlClient;

namespace CapaDatos
{
    public class CD_Cuota
    {
        public List<Cuota> ListarPendientesPorContrato(Guid idContrato)
        {
            var lista = new List<Cuota>();

            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT 
                        c.id_cuota,
                        c.id_contrato_alquiler,
                        c.nro_cuota,
                        c.periodo,
                        c.fecha_vencimiento,
                        ca.precio_cuota,
                        c.estado,
                        c.valor_mora_aplicada,
                        c.descuento_adicional_total,
                        c.otros_adicionales_total,
                        c.valor_indice_aplicado,
                        c.importe_total_calculado
                    FROM cuota c
                    INNER JOIN contrato_alquiler ca 
                        ON c.id_contrato_alquiler = ca.id_contrato_alquiler
                    WHERE c.id_contrato_alquiler = @idContrato
                      AND c.estado IN ('Pendiente', 'Vencida')
                    ORDER BY c.nro_cuota ASC";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@idContrato", idContrato);

                    cn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Cuota
                            {
                                IdCuota = Guid.Parse(dr["id_cuota"].ToString()!),
                                IdContratoAlquiler = Guid.Parse(dr["id_contrato_alquiler"].ToString()!),
                                NroCuota = Convert.ToInt32(dr["nro_cuota"]),
                                Periodo = dr["periodo"].ToString()!,
                                FechaVencimiento = Convert.ToDateTime(dr["fecha_vencimiento"]),

                               

                                Estado = dr["estado"].ToString()!,

                                ValorMoraAplicada = dr["valor_mora_aplicada"] != DBNull.Value
                                    ? Convert.ToDecimal(dr["valor_mora_aplicada"])
                                    : 0m,

                                DescuentoAdicionalTotal = dr["descuento_adicional_total"] != DBNull.Value
                                    ? Convert.ToDecimal(dr["descuento_adicional_total"])
                                    : 0m,

                                OtrosAdicionalesTotal = dr["otros_adicionales_total"] != DBNull.Value
                                    ? Convert.ToDecimal(dr["otros_adicionales_total"])
                                    : 0m,

                                ValorIndiceAplicado = dr["valor_indice_aplicado"] != DBNull.Value
                                    ? Convert.ToDecimal(dr["valor_indice_aplicado"])
                                    : 1m,

                                ImporteTotalCalculado = dr["importe_total_calculado"] != DBNull.Value
                                    ? Convert.ToDecimal(dr["importe_total_calculado"])
                                    : 0m
                            });
                        }
                    }
                }
            }

            return lista;
        }


        public Cuota? ObtenerUltimaPendientePorContrato(Guid contratoId)
        {
            Cuota? cuota = null;

            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT TOP 1
                        c.id_cuota,
                        c.id_contrato_alquiler,
                        c.nro_cuota,
                        c.periodo,
                        c.fecha_vencimiento,
                        c.estado,
                        c.descuento_adicional_total
                    FROM cuota c
                    WHERE c.id_contrato_alquiler = @idContrato
                      AND c.estado IN ('Pendiente', 'Vencida')
                    ORDER BY c.nro_cuota ASC";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@idContrato", contratoId);

                    cn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            cuota = new Cuota
                            {
                                IdCuota = Guid.Parse(dr["id_cuota"].ToString()!),
                                IdContratoAlquiler = Guid.Parse(dr["id_contrato_alquiler"].ToString()!),
                                NroCuota = Convert.ToInt32(dr["nro_cuota"]),
                                Periodo = dr["periodo"].ToString()!,
                                FechaVencimiento = Convert.ToDateTime(dr["fecha_vencimiento"]),
                                Estado = dr["estado"].ToString()!,
                                DescuentoAdicionalTotal = dr["descuento_adicional_total"] != DBNull.Value
                                    ? Convert.ToDecimal(dr["descuento_adicional_total"])
                                    : 0m
                            };
                        }
                    }
                }
            }

            return cuota;
        }

        public int AnularPendientesPorContrato(Guid idContrato)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    UPDATE cuota 
                    SET estado = 'Cancelada' 
                    WHERE id_contrato_alquiler = @idContrato 
                      AND estado IN ('Pendiente', 'Vencida')";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@idContrato", idContrato);
                    cn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public Cuota? ObtenerPorId(Guid idCuota)
        {
            Cuota? cuota = null;

            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
        SELECT 
            c.id_cuota,
            c.id_contrato_alquiler,
            c.nro_cuota,
            c.periodo,
            c.fecha_vencimiento,
            c.estado,
            c.valor_mora_aplicada,
            c.descuento_adicional_total,
            c.otros_adicionales_total,
            c.valor_indice_aplicado,
            c.importe_total_calculado
        FROM cuota c
        WHERE c.id_cuota = @id";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", idCuota);

                    cn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            cuota = new Cuota
                            {
                                IdCuota = Guid.Parse(dr["id_cuota"].ToString()!),
                                IdContratoAlquiler = Guid.Parse(dr["id_contrato_alquiler"].ToString()!),
                                NroCuota = Convert.ToInt32(dr["nro_cuota"]),
                                Periodo = dr["periodo"].ToString()!,
                                FechaVencimiento = Convert.ToDateTime(dr["fecha_vencimiento"]),
                                Estado = dr["estado"].ToString()!,
                                ValorMoraAplicada = dr["valor_mora_aplicada"] != DBNull.Value
                                    ? Convert.ToDecimal(dr["valor_mora_aplicada"])
                                    : 0m,
                                DescuentoAdicionalTotal = dr["descuento_adicional_total"] != DBNull.Value
                                    ? Convert.ToDecimal(dr["descuento_adicional_total"])
                                    : 0m,
                                OtrosAdicionalesTotal = dr["otros_adicionales_total"] != DBNull.Value
                                    ? Convert.ToDecimal(dr["otros_adicionales_total"])
                                    : 0m,
                                ValorIndiceAplicado = dr["valor_indice_aplicado"] != DBNull.Value
                                    ? Convert.ToDecimal(dr["valor_indice_aplicado"])
                                    : 1m,
                                ImporteTotalCalculado = dr["importe_total_calculado"] != DBNull.Value
                                    ? Convert.ToDecimal(dr["importe_total_calculado"])
                                    : 0m
                            };
                        }
                    }
                }
            }

            return cuota;
        }
    }
}