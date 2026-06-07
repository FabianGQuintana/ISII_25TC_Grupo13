using CapaEntidades;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_CuotaAdicional
    {
        public bool InsertarCuotaAdicional(CuotaAdicional adicional)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                INSERT INTO cuota_adicional
                (
                    id_cuota_adicional,
                    id_cuota,
                    id_tipo_adicionales,
                    monto_aplicado,
                    descripcion_manual,
                    fecha_creacion
                )
                VALUES
                (
                    @id,
                    @idCuota,
                    @idTipo,
                    @monto,
                    @descripcion,
                    GETDATE()
                )";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", adicional.IdCuotaAdicional);

                    cmd.Parameters.AddWithValue("@idCuota", adicional.IdCuota);

                    cmd.Parameters.AddWithValue("@idTipo", adicional.IdTipoAdicionales);

                    cmd.Parameters.AddWithValue("@monto", adicional.MontoAplicado);

                    cmd.Parameters.AddWithValue("@descripcion",
                        (object?)adicional.DescripcionManual ?? DBNull.Value);

                    cn.Open();

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public decimal ObtenerTotalAdicionales(Guid idCuota)
        {
            decimal total = 0;

            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
            SELECT ISNULL(SUM(monto_aplicado), 0)
            FROM cuota_adicional
            WHERE id_cuota = @id";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", idCuota);

                    cn.Open();

                    total = Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }

            return total;
        }

        public List<CuotaAdicional> ObtenerDetalleAdicionalesPorCuota(Guid idCuota)
        {
            var lista = new List<CuotaAdicional>();

            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT 
                        ca.id_cuota_adicional,
                        ca.id_cuota,
                        ca.id_tipo_adicionales,
                        ca.monto_aplicado,
                        ca.descripcion_manual,
                        ca.fecha_creacion,
                        ta.descripcion AS tipo_adicional_descripcion,
                        ta.monto_base
                    FROM cuota_adicional ca
                    INNER JOIN tipo_adicionales ta 
                        ON ca.id_tipo_adicionales = ta.id_tipo_adicionales
                    WHERE ca.id_cuota = @idCuota
                    ORDER BY ca.fecha_creacion ASC";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@idCuota", idCuota);

                    cn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new CuotaAdicional
                            {
                                IdCuotaAdicional = dr["id_cuota_adicional"] is Guid id ? id : Guid.Empty,
                                IdCuota = dr["id_cuota"] is Guid idC ? idC : Guid.Empty,
                                IdTipoAdicionales = dr["id_tipo_adicionales"] is Guid idT ? idT : Guid.Empty,
                                MontoAplicado = dr["monto_aplicado"] is decimal m ? m : 0m,
                                DescripcionManual = dr["descripcion_manual"]?.ToString(),
                                FechaCreacion = dr["fecha_creacion"] is DateTime f ? f : default,
                                OTipoAdicional = new TipoAdicional
                                {
                                    IdTipoAdicionales = dr["id_tipo_adicionales"] is Guid idTA ? idTA : Guid.Empty,
                                    Descripcion = dr["tipo_adicional_descripcion"]?.ToString() ?? "",
                                    MontoBase = dr["monto_base"] is decimal mb ? mb : 0m
                                }
                            });
                        }
                    }
                }
            }

            return lista;
        }
    }
}
