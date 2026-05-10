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
                    SELECT id_cuota, id_contrato_alquiler, nro_cuota, periodo, fecha_vencimiento, 
                           importe_base, estado, valor_mora_aplicada, descuento_adicional_total, 
                           otros_adicionales_total, valor_indice_aplicado, importe_total_calculado
                    FROM cuota 
                    WHERE id_contrato_alquiler = @idContrato 
                      AND estado IN ('Pendiente', 'Vencida')
                    ORDER BY nro_cuota ASC";

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
                                ImporteBase = Convert.ToDecimal(dr["importe_base"]),
                                Estado = dr["estado"].ToString()!,
                                ValorMoraAplicada = dr["valor_mora_aplicada"] != DBNull.Value ? Convert.ToDecimal(dr["valor_mora_aplicada"]) : 0m,
                                DescuentoAdicionalTotal = dr["descuento_adicional_total"] != DBNull.Value ? Convert.ToDecimal(dr["descuento_adicional_total"]) : 0m,
                                OtrosAdicionalesTotal = dr["otros_adicionales_total"] != DBNull.Value ? Convert.ToDecimal(dr["otros_adicionales_total"]) : 0m,
                                ValorIndiceAplicado = dr["valor_indice_aplicado"] != DBNull.Value ? Convert.ToDecimal(dr["valor_indice_aplicado"]) : 1m,
                                ImporteTotalCalculado = dr["importe_total_calculado"] != DBNull.Value ? Convert.ToDecimal(dr["importe_total_calculado"]) : 0m
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}
