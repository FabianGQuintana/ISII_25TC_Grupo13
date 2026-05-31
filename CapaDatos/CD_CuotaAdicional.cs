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
    }
}
