using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_RolCliente
    {
        public List<RolCliente> ListarRoles()
        {
            var lista = new List<RolCliente>();

            using var cn = new SqlConnection(Conexion.Cadena);
            string query = @"
                SELECT id_rol_cliente, nombre, descripcion
                FROM rol_cliente
                ORDER BY nombre";

            using var cmd = new SqlCommand(query, cn);
            cn.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new RolCliente
                {
                    IdRolCliente = Guid.Parse(dr["id_rol_cliente"].ToString()!),
                    Nombre = dr["nombre"]?.ToString() ?? "",
                    Descripcion = dr["descripcion"]?.ToString() ?? ""
                });
            }

            return lista;
        }
    }
}
