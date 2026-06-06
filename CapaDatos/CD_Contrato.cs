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
                        p.dni AS inquilino_dni,
                        rc.id_rol_cliente AS inquilino_rol_id,
                        rc.nombre AS inquilino_rol_nombre
                    FROM contrato_alquiler c
                    INNER JOIN inmueble i ON c.id_inmueble = i.id_inmueble
                    INNER JOIN direccion d ON i.id_direccion = d.id_direccion
                    INNER JOIN persona p ON c.id_persona_inquilino = p.id_persona
                    INNER JOIN rol_cliente rc ON c.id_rol_cliente_inquilino = rc.id_rol_cliente
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
                                IdContratoAlquiler = dr["id_contrato_alquiler"] is Guid idContrato ? idContrato : Guid.Empty,
                                FechaFin = dr["fecha_fin"] is DateTime ff ? ff : default,
                                Condiciones = dr["condiciones"]?.ToString(),
                                CantidadCuotas = dr["cantidad_cuotas"] is int cantCuotas ? cantCuotas : 0,
                                PrecioCuota = dr["precio_cuota"] is decimal pc ? pc : 0m,
                                FechaCreacion = dr["fecha_creacion"] is DateTime fc ? fc : default,
                                IdInmueble = dr["id_inmueble"] is Guid idInm ? idInm : Guid.Empty,
                                IdPersonaInquilino = dr["id_persona_inquilino"] is Guid idPers ? idPers : Guid.Empty,
                                IdRolClienteInquilino = dr["id_rol_cliente_inquilino"] is Guid idRol ? idRol : Guid.Empty,
                                TasaMoraMensual = dr["tasa_mora_mensual"] is decimal tm ? tm : 0m,
                                Estado = dr["estado"]?.ToString() ?? "Activo",
                                IdUsuarioCreador = dr["id_usuario_creador"] is Guid idUser ? idUser : Guid.Empty,
                                FrecuenciaAjuste = dr["frecuencia_ajuste"]?.ToString(),
                                IdTipoIndice = dr["id_tipo_indice"] is Guid idTipo ? idTipo : null,
                                ValorIndiceInicio = dr["valor_indice_inicio"] is decimal vi ? vi : null,
                                OInmueble = new Inmueble
                                {
                                    IdInmueble = dr["id_inmueble"] is Guid idInmueble ? idInmueble : Guid.Empty,
                                    Descripcion = dr["inmueble_descripcion"]?.ToString(),
                                    ODireccion = new Direccion
                                    {
                                        Calle = dr["direccion"]?.ToString() ?? ""
                                    }
                                },
                                OInquilino = new Inquilino
                                {
                                    IdPersona = Guid.Parse(dr["id_persona_inquilino"].ToString()!),
                                    IdRolCliente = Guid.Parse(dr["inquilino_rol_id"].ToString()!),
                                    OPersona = new Persona
                                    {
                                        IdPersona = Guid.Parse(dr["id_persona_inquilino"].ToString()!),
                                        Nombre = dr["inquilino_nombre"]?.ToString() ?? "",
                                        Apellido = dr["inquilino_apellido"]?.ToString() ?? "",
                                        Dni = dr["inquilino_dni"]?.ToString()
                                    },
                                    ORolCliente = new RolCliente
                                    {
                                        IdRolCliente = Guid.Parse(dr["inquilino_rol_id"].ToString()!),
                                        Nombre = dr["inquilino_rol_nombre"]?.ToString() ?? ""
                                    }
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
                        p.dni AS inquilino_dni,
                        rc.id_rol_cliente AS inquilino_rol_id,
                        rc.nombre AS inquilino_rol_nombre
                    FROM contrato_alquiler c
                    INNER JOIN inmueble i ON c.id_inmueble = i.id_inmueble
                    INNER JOIN direccion d ON i.id_direccion = d.id_direccion
                    INNER JOIN persona p ON c.id_persona_inquilino = p.id_persona
                    INNER JOIN rol_cliente rc ON c.id_rol_cliente_inquilino = rc.id_rol_cliente
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
                                IdContratoAlquiler = dr["id_contrato_alquiler"] is Guid idContrato ? idContrato : Guid.Empty,
                                FechaFin = dr["fecha_fin"] is DateTime ff ? ff : default,
                                Condiciones = dr["condiciones"]?.ToString(),
                                CantidadCuotas = dr["cantidad_cuotas"] is int cantCuotas ? cantCuotas : 0,
                                PrecioCuota = dr["precio_cuota"] is decimal pc ? pc : 0m,
                                FechaCreacion = dr["fecha_creacion"] is DateTime fc ? fc : default,
                                IdInmueble = dr["id_inmueble"] is Guid idInm ? idInm : Guid.Empty,
                                IdPersonaInquilino = dr["id_persona_inquilino"] is Guid idPers ? idPers : Guid.Empty,
                                IdRolClienteInquilino = dr["id_rol_cliente_inquilino"] is Guid idRol ? idRol : Guid.Empty,
                                TasaMoraMensual = dr["tasa_mora_mensual"] is decimal tm ? tm : 0m,
                                Estado = dr["estado"]?.ToString() ?? "Activo",
                                IdUsuarioCreador = dr["id_usuario_creador"] is Guid idUser ? idUser : Guid.Empty,
                                FrecuenciaAjuste = dr["frecuencia_ajuste"]?.ToString(),
                                IdTipoIndice = dr["id_tipo_indice"] is Guid idTipo ? idTipo : null,
                                ValorIndiceInicio = dr["valor_indice_inicio"] is decimal vi ? vi : null,
                                OInmueble = new Inmueble
                                {
                                    IdInmueble = dr["id_inmueble"] is Guid idInmueble ? idInmueble : Guid.Empty,
                                    Descripcion = dr["inmueble_descripcion"]?.ToString(),
                                    ODireccion = new Direccion
                                    {
                                        Calle = dr["direccion"]?.ToString() ?? ""
                                    }
                                },
                                OInquilino = new Inquilino
                                {
                                    IdPersona = Guid.Parse(dr["id_persona_inquilino"].ToString()!),
                                    IdRolCliente = Guid.Parse(dr["inquilino_rol_id"].ToString()!),
                                    OPersona = new Persona
                                    {
                                        IdPersona = Guid.Parse(dr["id_persona_inquilino"].ToString()!),
                                        Nombre = dr["inquilino_nombre"]?.ToString() ?? "",
                                        Apellido = dr["inquilino_apellido"]?.ToString() ?? "",
                                        Dni = dr["inquilino_dni"]?.ToString()
                                    },
                                    ORolCliente = new RolCliente
                                    {
                                        IdRolCliente = Guid.Parse(dr["inquilino_rol_id"].ToString()!),
                                        Nombre = dr["inquilino_rol_nombre"]?.ToString() ?? ""
                                    }
                                }
                            };
                        }
                    }
                }
            }

            return null;
        }

        public Guid? ObtenerIdRolInquilino(Guid idPersona)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    SELECT TOP 1 prc.id_rol_cliente
                    FROM persona_rol_cliente prc
                    INNER JOIN rol_cliente rc ON prc.id_rol_cliente = rc.id_rol_cliente
                    WHERE prc.id_persona = @idPersona AND rc.nombre = 'Inquilino'";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@idPersona", idPersona);
                    cn.Open();
                    var result = cmd.ExecuteScalar();
                    return result != null ? (Guid)result : (Guid?)null;
                }
            }
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
                        var idContrato = contrato.IdContratoAlquiler != Guid.Empty
                            ? contrato.IdContratoAlquiler
                            : Guid.NewGuid();

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
                                    @fechaInicio, @idInmueble, @idPersonaInquilino, @idRolClienteInquilino,
                                    @tasaMoraMensual, 'Activo', @idUsuarioCreador, @frecuenciaAjuste, @idTipoIndice, @valorIndiceInicio)";

                        using (var cmdContrato = new SqlCommand(queryContrato, cn, transaction))
                        {
                            cmdContrato.Parameters.AddWithValue("@id", idContrato);
                            cmdContrato.Parameters.AddWithValue("@fechaInicio", contrato.FechaCreacion);
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

                        string queryCuota = @"
                                    INSERT INTO cuota
                                    (id_cuota, id_contrato_alquiler, nro_cuota, periodo, fecha_vencimiento,
                                     estado, valor_mora_aplicada, descuento_adicional_total,
                                     otros_adicionales_total, valor_indice_aplicado, importe_total_calculado)
                                    VALUES (@id, @idContrato, @nroCuota, @periodo, @fechaVencimiento,
                                            'Pendiente', 0, 0, 0, 0, @importeTotal)";

                        if (contrato.Cuotas.Count > 0)
                        {
                            foreach (var cuota in contrato.Cuotas)
                            {
                                using (var cmdCuota = new SqlCommand(queryCuota, cn, transaction))
                                {
                                    cmdCuota.Parameters.AddWithValue("@id", cuota.IdCuota);
                                    cmdCuota.Parameters.AddWithValue("@idContrato", idContrato);
                                    cmdCuota.Parameters.AddWithValue("@nroCuota", cuota.NroCuota);
                                    cmdCuota.Parameters.AddWithValue("@periodo", cuota.Periodo);
                                    cmdCuota.Parameters.AddWithValue("@fechaVencimiento", cuota.FechaVencimiento);
                                    cmdCuota.Parameters.AddWithValue("@importeTotal", cuota.ImporteTotalCalculado);
                                    cmdCuota.ExecuteNonQuery();
                                }
                            }
                        }
                        else
                        {
                            var fechaVencimiento = contrato.FechaCreacion;
                            for (int i = 1; i <= contrato.CantidadCuotas; i++)
                            {
                                fechaVencimiento = fechaVencimiento.AddMonths(1);
                                using (var cmdCuota = new SqlCommand(queryCuota, cn, transaction))
                                {
                                    cmdCuota.Parameters.AddWithValue("@id", Guid.NewGuid());
                                    cmdCuota.Parameters.AddWithValue("@idContrato", idContrato);
                                    cmdCuota.Parameters.AddWithValue("@nroCuota", i);
                                    cmdCuota.Parameters.AddWithValue("@periodo", fechaVencimiento.ToString("yyyyMM"));
                                    cmdCuota.Parameters.AddWithValue("@fechaVencimiento", fechaVencimiento);
                                    cmdCuota.Parameters.AddWithValue("@importeTotal", contrato.PrecioCuota);
                                    cmdCuota.ExecuteNonQuery();
                                }
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

        public (bool success, string message) RescindirEstado(Guid idContrato)
        {
            using (var cn = new SqlConnection(Conexion.Cadena))
            {
                string query = @"
                    UPDATE contrato_alquiler 
                    SET estado = 'Rescindido' 
                    WHERE id_contrato_alquiler = @id AND estado = 'Activo'";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@id", idContrato);
                    cn.Open();
                    var filas = cmd.ExecuteNonQuery();
                    if (filas == 0)
                        return (false, "Contrato no encontrado o ya rescindido");
                }
            }

            return (true, "");
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
                    p.dni AS inquilino_dni,
                    rc.id_rol_cliente AS inquilino_rol_id,
                    rc.nombre AS inquilino_rol_nombre
                FROM contrato_alquiler c
                INNER JOIN inmueble i ON c.id_inmueble = i.id_inmueble
                INNER JOIN direccion d ON i.id_direccion = d.id_direccion
                INNER JOIN persona p ON c.id_persona_inquilino = p.id_persona
                INNER JOIN rol_cliente rc ON c.id_rol_cliente_inquilino = rc.id_rol_cliente
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
                                IdContratoAlquiler = dr["id_contrato_alquiler"] is Guid idContrato ? idContrato : Guid.Empty,
                                FechaFin = dr["fecha_fin"] is DateTime ff ? ff : default,
                                Condiciones = dr["condiciones"]?.ToString(),
                                CantidadCuotas = dr["cantidad_cuotas"] is int cantCuotas ? cantCuotas : 0,
                                PrecioCuota = dr["precio_cuota"] is decimal pc ? pc : 0m,
                                FechaCreacion = dr["fecha_creacion"] is DateTime fc ? fc : default,
                                IdInmueble = dr["id_inmueble"] is Guid idInm ? idInm : Guid.Empty,
                                IdPersonaInquilino = dr["id_persona_inquilino"] is Guid idPers ? idPers : Guid.Empty,
                                IdRolClienteInquilino = dr["id_rol_cliente_inquilino"] is Guid idRol ? idRol : Guid.Empty,
                                TasaMoraMensual = dr["tasa_mora_mensual"] is decimal tm ? tm : 0m,
                                Estado = dr["estado"]?.ToString() ?? "Activo",
                                IdUsuarioCreador = dr["id_usuario_creador"] is Guid idUser ? idUser : Guid.Empty,
                                FrecuenciaAjuste = dr["frecuencia_ajuste"]?.ToString(),
                                IdTipoIndice = dr["id_tipo_indice"] is Guid idTipo ? idTipo : null,
                                ValorIndiceInicio = dr["valor_indice_inicio"] is decimal vi ? vi : null,
                                OInmueble = new Inmueble
                                {
                                    IdInmueble = dr["id_inmueble"] is Guid idInmueble ? idInmueble : Guid.Empty,
                                    Descripcion = dr["inmueble_descripcion"]?.ToString(),
                                    ODireccion = new Direccion
                                    {
                                        Calle = dr["direccion"]?.ToString() ?? ""
                                    }
                                },
                                OInquilino = new Inquilino
                                {
                                    IdPersona = dr["id_persona_inquilino"] is Guid idPersI ? idPersI : Guid.Empty,
                                    IdRolCliente = dr["inquilino_rol_id"] is Guid idRolI ? idRolI : Guid.Empty,
                                    OPersona = new Persona
                                    {
                                        IdPersona = dr["id_persona_inquilino"] is Guid idPersP ? idPersP : Guid.Empty,
                                        Nombre = dr["inquilino_nombre"]?.ToString() ?? "",
                                        Apellido = dr["inquilino_apellido"]?.ToString() ?? "",
                                        Dni = dr["inquilino_dni"]?.ToString()
                                    },
                                    ORolCliente = new RolCliente
                                    {
                                        IdRolCliente = dr["inquilino_rol_id"] is Guid idRolRC ? idRolRC : Guid.Empty,
                                        Nombre = dr["inquilino_rol_nombre"]?.ToString() ?? ""
                                    }
                                }
                            });
                        }
                    }
                }
            }

            return lista;
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
                    var count = cmd.ExecuteScalar() is int n ? n : 0;
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