using System;
using System.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_Contrato
    {
        public bool CrearContrato(ContratoAlquiler contrato)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    INSERT INTO contrato_alquiler (
                        id_contrato, fecha_inicio, fecha_fin, cantidad_cuotas, 
                        precio_cuota, frecuencia_ajuste, tasa_mora_mensual, estado,
                        fecha_creacion, id_inmueble, id_inquilino, id_usuario_creador, id_tipo_indice
                    ) VALUES (
                        @id, @fecha_inicio, @fecha_fin, @cantidad_cuotas,
                        @precio_cuota, @frecuencia_ajuste, @tasa_mora_mensual, @estado,
                        @fecha_creacion, @id_inmueble, @id_inquilino, @id_usuario_creador, @id_tipo_indice
                    )";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", contrato.IdContrato);
                    cmd.Parameters.AddWithValue("@fecha_inicio", contrato.FechaInicio);
                    cmd.Parameters.AddWithValue("@fecha_fin", contrato.FechaFin);
                    cmd.Parameters.AddWithValue("@cantidad_cuotas", contrato.CantidadCuotas);
                    cmd.Parameters.AddWithValue("@precio_cuota", contrato.PrecioCuota);
                    cmd.Parameters.AddWithValue("@frecuencia_ajuste", contrato.FrecuenciaAjuste);
                    cmd.Parameters.AddWithValue("@tasa_mora_mensual", contrato.TasaMoraMensual);
                    cmd.Parameters.AddWithValue("@estado", contrato.Estado);
                    cmd.Parameters.AddWithValue("@fecha_creacion", contrato.FechaCreacion);
                    cmd.Parameters.AddWithValue("@id_inmueble", contrato.IdInmueble);
                    cmd.Parameters.AddWithValue("@id_inquilino", contrato.IdInquilino);
                    cmd.Parameters.AddWithValue("@id_usuario_creador", contrato.IdUsuarioCreador);
                    cmd.Parameters.AddWithValue("@id_tipo_indice", contrato.IdTipoIndice);

                    cn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool ValidarDisponibilidadInmueble(Guid idInmueble, DateTime fechaInicio, DateTime fechaFin)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT COUNT(1) 
                    FROM contrato_alquiler 
                    WHERE id_inmueble = @id_inmueble 
                        AND estado = 'Activo'
                        AND (
                            (fecha_inicio <= @fecha_fin AND fecha_fin >= @fecha_inicio)
                        )";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id_inmueble", idInmueble);
                    cmd.Parameters.AddWithValue("@fecha_inicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@fecha_fin", fechaFin);

                    cn.Open();
                    var count = (int)cmd.ExecuteScalar();
                    return count == 0;
                }
            }
        }

        public decimal? ObtenerValorIndiceActual(Guid idTipoIndice)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT TOP 1 valor 
                    FROM indice 
                    WHERE id_tipo_indice = @id_tipo_indice 
                    ORDER BY fecha DESC";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id_tipo_indice", idTipoIndice);

                    cn.Open();
                    var result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? null : Convert.ToDecimal(result);
                }
            }
        }

        public ContratoAlquiler? ObtenerPorId(Guid idContrato)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT 
                        c.id_contrato, c.fecha_inicio, c.fecha_fin, c.cantidad_cuotas,
                        c.precio_cuota, c.frecuencia_ajuste, c.tasa_mora_mensual, c.estado, c.fecha_creacion,
                        c.id_inmueble, c.id_inquilino, c.id_usuario_creador, c.id_tipo_indice,
                        i.direccion, p.nombre, p.apellido, p.email,
                        u.id_persona, ti.nombre AS indice_nombre
                    FROM contrato_alquiler c
                    INNER JOIN inmueble i ON c.id_inmueble = i.id_inmueble
                    INNER JOIN persona p ON c.id_inquilino = p.id_persona
                    INNER JOIN usuario u ON c.id_usuario_creador = u.id_usuario
                    INNER JOIN tipo_indice ti ON c.id_tipo_indice = ti.id_tipo_indice
                    WHERE c.id_contrato = @id";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", idContrato);

                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new ContratoAlquiler
                            {
                                IdContrato = Guid.Parse(dr["id_contrato"].ToString()!),
                                FechaInicio = DateTime.Parse(dr["fecha_inicio"].ToString()!),
                                FechaFin = DateTime.Parse(dr["fecha_fin"].ToString()!),
                                CantidadCuotas = (int)dr["cantidad_cuotas"],
                                PrecioCuota = (decimal)dr["precio_cuota"],
                                FrecuenciaAjuste = dr["frecuencia_ajuste"].ToString()!,
                                TasaMoraMensual = (decimal)dr["tasa_mora_mensual"],
                                Estado = dr["estado"].ToString()!,
                                FechaCreacion = DateTime.Parse(dr["fecha_creacion"].ToString()!),
                                IdInmueble = Guid.Parse(dr["id_inmueble"].ToString()!),
                                IdInquilino = Guid.Parse(dr["id_inquilino"].ToString()!),
                                IdUsuarioCreador = Guid.Parse(dr["id_usuario_creador"].ToString()!),
                                IdTipoIndice = Guid.Parse(dr["id_tipo_indice"].ToString()!),
                                OInmueble = new Inmueble
                                {
                                    IdInmueble = Guid.Parse(dr["id_inmueble"].ToString()!),
                                    Direccion = dr["direccion"].ToString()!
                                },
                                OInquilino = new Persona
                                {
                                    IdPersona = Guid.Parse(dr["id_inquilino"].ToString()!),
                                    Nombre = dr["nombre"].ToString()!,
                                    Apellido = dr["apellido"].ToString()!,
                                    Email = dr["email"].ToString()!
                                },
                                OTipoIndice = new TipoIndice
                                {
                                    IdTipoIndice = Guid.Parse(dr["id_tipo_indice"].ToString()!),
                                    Nombre = dr["indice_nombre"].ToString()!
                                }
                            };
                        }
                    }
                }
            }
            return null;
        }
    }
}