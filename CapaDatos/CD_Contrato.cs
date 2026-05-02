using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_Contrato
    {
        public List<ContratoAlquiler> Listar(int? estado = null)
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
                    WHERE 1=1";

                if (estado.HasValue)
                {
                    if (estado.Value == 1)
                        query += " AND c.estado = 'Activo'";
                    else if (estado.Value == 0)
                        query += " AND c.estado = 'Rescindido'";
                }

                query += " ORDER BY c.fecha_creacion DESC";

                using (var cmd = new SqlCommand(query, cn))
                {
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
                                FechaCreacion = DateTime.Parse(dr["fecha_creacion"].ToString()!),
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

        public ContratoAlquiler? ObtenerPorId(Guid id)
        {
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
                    WHERE c.id_contrato_alquiler = @id";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new ContratoAlquiler
                            {
                                IdContratoAlquiler = Guid.Parse(dr["id_contrato_alquiler"].ToString()!),
                                FechaFin = DateTime.Parse(dr["fecha_fin"].ToString()!),
                                Condiciones = dr["condiciones"]?.ToString(),
                                CantidadCuotas = int.Parse(dr["cantidad_cuotas"].ToString()!),
                                PrecioCuota = decimal.Parse(dr["precio_cuota"].ToString()!),
                                FechaCreacion = DateTime.Parse(dr["fecha_creacion"].ToString()!),
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
                            };
                        }
                    }
                }
            }

            return null;
        }

        public (bool success, string message, Guid? contratoId) Insertar(ContratoAlquiler contrato)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                cn.Open();
                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        var idContrato = Guid.NewGuid();

                        if (contrato.IdRolClienteInquilino == Guid.Empty)
                        {
                            string queryRol = @"
                                SELECT TOP 1 prc.id_rol_cliente 
                                FROM persona_rol_cliente prc
                                INNER JOIN rol_cliente rc ON prc.id_rol_cliente = rc.id_rol_cliente
                                WHERE prc.id_persona = @idPersonaParam AND rc.nombre = 'Inquilino'";
                            
                            using (var cmdRol = new SqlCommand(queryRol, cn, transaction))
                            {
                                cmdRol.Parameters.AddWithValue("@idPersonaParam", contrato.IdPersonaInquilino);
                                var rolIdObj = cmdRol.ExecuteScalar();
                                if (rolIdObj != null)
                                {
                                    contrato.IdRolClienteInquilino = (Guid)rolIdObj;
                                }
                                else
                                {
                                    throw new Exception("El inquilino seleccionado no tiene un rol 'Inquilino' válido asignado en el sistema.");
                                }
                            }
                        }

                        string queryContrato = @"
                            INSERT INTO contrato_alquiler 
                            (id_contrato_alquiler, fecha_fin, condiciones, cantidad_cuotas, precio_cuota, 
                             fecha_creacion, id_inmueble, id_persona_inquilino, id_rol_cliente_inquilino, 
                             tasa_mora_mensual, estado, id_usuario_creador, frecuencia_ajuste, id_tipo_indice, valor_indice_inicio)
                            VALUES (@id, @fechaFin, @condiciones, @cantidadCuotas, @precioCuota, 
                                    GETDATE(), @idInmueble, @idPersonaInquilino, @idRolClienteInquilino, 
                                    @tasaMoraMensual, 'Activo', @idUsuarioCreador, @frecuenciaAjuste, @idTipoIndice, @valorIndiceInicio)";

                        using (var cmdContrato = new SqlCommand(queryContrato, cn, transaction))
                        {
                            cmdContrato.Parameters.AddWithValue("@id", idContrato);
                            cmdContrato.Parameters.AddWithValue("@fechaFin", contrato.FechaFin);
                            cmdContrato.Parameters.AddWithValue("@condiciones", (object?)contrato.Condiciones ?? DBNull.Value);
                            cmdContrato.Parameters.AddWithValue("@cantidadCuotas", contrato.CantidadCuotas);
                            cmdContrato.Parameters.AddWithValue("@precioCuota", contrato.PrecioCuota);
                            cmdContrato.Parameters.AddWithValue("@idInmueble", contrato.IdInmueble);
                            cmdContrato.Parameters.AddWithValue("@idPersonaInquilino", contrato.IdPersonaInquilino);
                            cmdContrato.Parameters.AddWithValue("@idRolClienteInquilino", contrato.IdRolClienteInquilino);
                            cmdContrato.Parameters.AddWithValue("@tasaMoraMensual", contrato.TasaMoraMensual);
                            cmdContrato.Parameters.AddWithValue("@idUsuarioCreador", contrato.IdUsuarioCreador);
                            cmdContrato.Parameters.AddWithValue("@frecuenciaAjuste", (object?)contrato.FrecuenciaAjuste ?? DBNull.Value);
                            cmdContrato.Parameters.AddWithValue("@idTipoIndice", (object?)contrato.IdTipoIndice ?? DBNull.Value);
                            cmdContrato.Parameters.AddWithValue("@valorIndiceInicio", (object?)contrato.ValorIndiceInicio ?? DBNull.Value);

                            cmdContrato.ExecuteNonQuery();
                        }

                        var fechaVencimiento = contrato.FechaCreacion;
                        for (int i = 1; i <= contrato.CantidadCuotas; i++)
                        {
                            fechaVencimiento = fechaVencimiento.AddMonths(1);
                            var periodo = fechaVencimiento.ToString("yyyyMM");
                            var nroCuota = i;

                            string queryCuota = @"
                                INSERT INTO cuota 
                                (id_cuota, id_contrato_alquiler, nro_cuota, periodo, fecha_vencimiento, 
                                 importe_base, estado, valor_mora_aplicada, descuento_adicional_total, 
                                 otros_adicionales_total, valor_indice_aplicado, importe_total_calculado)
                                VALUES (@id, @idContrato, @nroCuota, @periodo, @fechaVencimiento, 
                                        @importeBase, 'Pendiente', 0, 0, 0, 0, @importeTotal)";

                            using (var cmdCuota = new SqlCommand(queryCuota, cn, transaction))
                            {
                                cmdCuota.Parameters.AddWithValue("@id", Guid.NewGuid());
                                cmdCuota.Parameters.AddWithValue("@idContrato", idContrato);
                                cmdCuota.Parameters.AddWithValue("@nroCuota", nroCuota);
                                cmdCuota.Parameters.AddWithValue("@periodo", periodo);
                                cmdCuota.Parameters.AddWithValue("@fechaVencimiento", fechaVencimiento);
                                cmdCuota.Parameters.AddWithValue("@importeBase", contrato.PrecioCuota);
                                cmdCuota.Parameters.AddWithValue("@importeTotal", contrato.PrecioCuota);

                                cmdCuota.ExecuteNonQuery();
                            }
                        }

                        string queryInmueble = @"
                            UPDATE inmueble 
                            SET disponibilidad = 0 
                            WHERE id_inmueble = @idInmueble";
                        using (var cmdInmueble = new SqlCommand(queryInmueble, cn, transaction))
                        {
                            cmdInmueble.Parameters.AddWithValue("@idInmueble", contrato.IdInmueble);
                            cmdInmueble.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return (true, "Contrato creado exitosamente", idContrato);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return (false, $"Error al crear contrato: {ex.Message}", null);
                    }
                }
            }
        }

        public (bool success, string message) Rescindir(Guid id)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                cn.Open();
                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        string queryContrato = @"
                            UPDATE contrato_alquiler 
                            SET estado = 'Rescindido' 
                            WHERE id_contrato_alquiler = @id AND estado = 'Activo'";

                        using (var cmdContrato = new SqlCommand(queryContrato, cn, transaction))
                        {
                            cmdContrato.Parameters.AddWithValue("@id", id);
                            var filas = cmdContrato.ExecuteNonQuery();
                            if (filas == 0)
                            {
                                transaction.Rollback();
                                return (false, "Contrato no encontrado o ya rescindido");
                            }
                        }

                        string queryCuotas = @"
                            UPDATE cuota 
                            SET estado = 'Cancelada' 
                            WHERE id_contrato_alquiler = @id AND estado = 'Pendiente'";

                        using (var cmdCuotas = new SqlCommand(queryCuotas, cn, transaction))
                        {
                            cmdCuotas.Parameters.AddWithValue("@id", id);
                            cmdCuotas.ExecuteNonQuery();
                        }

                        string queryInmueble = @"
                            UPDATE inmueble 
                            SET disponibilidad = 1 
                            WHERE id_inmueble = (SELECT TOP 1 id_inmueble FROM contrato_alquiler WHERE id_contrato_alquiler = @id)";
                        using (var cmdInmueble = new SqlCommand(queryInmueble, cn, transaction))
                        {
                            cmdInmueble.Parameters.AddWithValue("@id", id);
                            cmdInmueble.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return (true, "Contrato rescindido exitosamente");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return (false, $"Error: {ex.Message}");
                    }
                }
            }
        }

        public (bool success, string message) ValidarInmuebleDisponible(Guid idInmueble, Guid? idContratoExcluir = null)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT COUNT(*) FROM contrato_alquiler 
                    WHERE id_inmueble = @idInmueble AND estado = 'Activo'";

                if (idContratoExcluir.HasValue)
                {
                    query += " AND id_contrato_alquiler != @idExcluir";
                }

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@idInmueble", idInmueble);
                    if (idContratoExcluir.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@idExcluir", idContratoExcluir.Value);
                    }

                    cn.Open();
                    var count = (int)cmd.ExecuteScalar();
                    if (count > 0)
                    {
                        return (false, "El inmueble ya tiene un contrato activo");
                    }
                }
            }

            return (true, "");
        }
    }
}