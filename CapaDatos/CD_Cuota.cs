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
                                IdCuota = dr["id_cuota"] is Guid idC ? idC : Guid.Empty,
                                IdContratoAlquiler = dr["id_contrato_alquiler"] is Guid idCont ? idCont : Guid.Empty,
                                NroCuota = dr["nro_cuota"] is int nc ? nc : 0,
                                Periodo = dr["periodo"]?.ToString() ?? "",
                                FechaVencimiento = dr["fecha_vencimiento"] is DateTime fv ? fv : default,
                                Estado = dr["estado"]?.ToString() ?? "",
                                ValorMoraAplicada = dr["valor_mora_aplicada"] is decimal vm ? vm : 0m,
                                DescuentoAdicionalTotal = dr["descuento_adicional_total"] is decimal da ? da : 0m,
                                OtrosAdicionalesTotal = dr["otros_adicionales_total"] is decimal oa ? oa : 0m,
                                ValorIndiceAplicado = dr["valor_indice_aplicado"] is decimal vi ? vi : 1m,
                                ImporteTotalCalculado = dr["importe_total_calculado"] is decimal itc ? itc : 0m
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
                                IdCuota = dr["id_cuota"] is Guid idCuota ? idCuota : Guid.Empty,
                                IdContratoAlquiler = dr["id_contrato_alquiler"] is Guid idContrato ? idContrato : Guid.Empty,
                                NroCuota = dr["nro_cuota"] is int nc ? nc : 0,
                                Periodo = dr["periodo"]?.ToString() ?? "",
                                FechaVencimiento = dr["fecha_vencimiento"] is DateTime fv ? fv : default,
                                Estado = dr["estado"]?.ToString() ?? "",
                                DescuentoAdicionalTotal = dr["descuento_adicional_total"] is decimal da ? da : 0m
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

        public bool ActualizarDescuento(Guid idCuota, decimal descuento)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = "UPDATE cuota SET descuento_adicional_total = @descuento WHERE id_cuota = @id";
                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", idCuota);
                    cmd.Parameters.AddWithValue("@descuento", descuento);
                    cn.Open();
                    return cmd.ExecuteNonQuery() > 0;
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
                                IdCuota = dr["id_cuota"] is Guid idCta ? idCta : Guid.Empty,
                                IdContratoAlquiler = dr["id_contrato_alquiler"] is Guid idCont ? idCont : Guid.Empty,
                                NroCuota = dr["nro_cuota"] is int nc ? nc : 0,
                                Periodo = dr["periodo"]?.ToString() ?? "",
                                FechaVencimiento = dr["fecha_vencimiento"] is DateTime fv ? fv : default,
                                Estado = dr["estado"]?.ToString() ?? "",
                                ValorMoraAplicada = dr["valor_mora_aplicada"] is decimal vm ? vm : 0m,
                                DescuentoAdicionalTotal = dr["descuento_adicional_total"] is decimal da ? da : 0m,
                                OtrosAdicionalesTotal = dr["otros_adicionales_total"] is decimal oa ? oa : 0m,
                                ValorIndiceAplicado = dr["valor_indice_aplicado"] is decimal vi ? vi : 1m,
                                ImporteTotalCalculado = dr["importe_total_calculado"] is decimal itc ? itc : 0m
                            };
                        }
                    }
                }
            }

            return cuota;
        }
    }
}