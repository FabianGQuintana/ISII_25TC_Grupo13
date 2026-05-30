using System;
using Microsoft.Data.SqlClient;
using CapaEntidades;

namespace CapaDatos
{
    public class CD_Direccion
    {
        public bool AgregarDireccion(Direccion obj)
        {
            using var cn = new SqlConnection(Conexion.Cadena);
            cn.Open();
            AgregarDireccion(Guid.NewGuid(), obj, cn, null!);
            return true;
        }

        public bool EditarDireccion(Guid idInmueble, Direccion obj)
        {
            using var cn = new SqlConnection(Conexion.Cadena);
            cn.Open();
            EditarDireccion(idInmueble, obj, cn, null!);
            return true;
        }

        public bool EliminarDireccion(Guid idDireccion)
        {
            using var cn = new SqlConnection(Conexion.Cadena);
            string query = "DELETE FROM direccion WHERE id_direccion = @id";
            using var cmd = new SqlCommand(query, cn);
            cmd.Parameters.AddWithValue("@id", idDireccion);
            cn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public void AgregarDireccion(Guid idDireccion, Direccion obj, SqlConnection cn, SqlTransaction tx)
        {
            string query = @"
                INSERT INTO direccion (id_direccion, calle, altura, id_localidad)
                VALUES (@idDireccion, @calle, @altura, @idLocalidad)";

            using var cmd = new SqlCommand(query, cn, tx);
            cmd.Parameters.AddWithValue("@idDireccion", idDireccion);
            cmd.Parameters.AddWithValue("@calle", obj.Calle ?? "");
            cmd.Parameters.AddWithValue("@altura", obj.Altura ?? "");
            cmd.Parameters.AddWithValue("@idLocalidad",
                obj.IdLocalidad.HasValue ? (object)obj.IdLocalidad.Value : DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void EditarDireccion(Guid idInmueble, Direccion obj, SqlConnection cn, SqlTransaction tx)
        {
            string query = @"
                UPDATE direccion SET
                    calle = @calle,
                    altura = @altura,
                    id_localidad = @idLocalidad
                WHERE id_direccion = (
                    SELECT id_direccion FROM inmueble WHERE id_inmueble = @idInmueble
                )";

            using var cmd = new SqlCommand(query, cn, tx);
            cmd.Parameters.AddWithValue("@idInmueble", idInmueble);
            cmd.Parameters.AddWithValue("@calle", obj.Calle ?? "");
            cmd.Parameters.AddWithValue("@altura", obj.Altura ?? "");
            cmd.Parameters.AddWithValue("@idLocalidad",
                obj.IdLocalidad.HasValue ? (object)obj.IdLocalidad.Value : DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }
}
