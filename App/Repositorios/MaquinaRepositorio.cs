using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using LavanderiaApp.Modelos;

namespace LavanderiaApp.Repositorios;

public class MaquinaRepositorio
{
    public List<Maquina> ObtenerTodas()
    {
        var lista = new List<Maquina>();
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "SELECT IdMaquina, Nombre, Status, CiclosOperados, ProxMantenimientoCiclos, Observacion FROM Maquinas";
        using var cmd = new SqliteCommand(query, conexion);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new Maquina
            {
                IdMaquina = reader.GetInt32(0),
                Nombre = reader.IsDBNull(1) ? "" : reader.GetString(1),
                Status = reader.GetString(2),
                CiclosOperados = reader.GetInt32(3),
                ProxMantenimientoCiclos = reader.GetInt32(4),
                Observacion = reader.IsDBNull(5) ? "" : reader.GetString(5)
            });
        }
        return lista;
    }

    public void Guardar(Maquina maquina)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"
            UPDATE Maquinas SET 
            Nombre = @Nombre,
            Status = @Status, 
            CiclosOperados = @CiclosOperados, 
            ProxMantenimientoCiclos = @ProxMantenimientoCiclos, 
            Observacion = @Observacion
            WHERE IdMaquina = @IdMaquina";
            
        using var cmd = new SqliteCommand(query, conexion);
        cmd.Parameters.AddWithValue("@Nombre", maquina.Nombre);
        cmd.Parameters.AddWithValue("@Status", maquina.Status);
        cmd.Parameters.AddWithValue("@CiclosOperados", maquina.CiclosOperados);
        cmd.Parameters.AddWithValue("@ProxMantenimientoCiclos", maquina.ProxMantenimientoCiclos);
        cmd.Parameters.AddWithValue("@Observacion", maquina.Observacion ?? "");
        cmd.Parameters.AddWithValue("@IdMaquina", maquina.IdMaquina);
        
        cmd.ExecuteNonQuery();
    }

    public void ActualizarEstado(int idMaquina, string status, string observacion)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"
            UPDATE Maquinas SET 
            Status = @Status, 
            Observacion = @Observacion
            WHERE IdMaquina = @IdMaquina";
            
        using var cmd = new SqliteCommand(query, conexion);
        cmd.Parameters.AddWithValue("@Status", status);
        cmd.Parameters.AddWithValue("@Observacion", observacion ?? "");
        cmd.Parameters.AddWithValue("@IdMaquina", idMaquina);
        
        cmd.ExecuteNonQuery();
    }

    public void Insertar(Maquina maquina)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = @"
            INSERT INTO Maquinas (Nombre, Status, CiclosOperados, ProxMantenimientoCiclos, Observacion)
            VALUES (@Nombre, @Status, @CiclosOperados, @ProxMantenimientoCiclos, @Observacion);
            SELECT last_insert_rowid();";
            
        using var cmd = new SqliteCommand(query, conexion);
        cmd.Parameters.AddWithValue("@Nombre", maquina.Nombre);
        cmd.Parameters.AddWithValue("@Status", maquina.Status ?? "INACTIVA");
        cmd.Parameters.AddWithValue("@CiclosOperados", maquina.CiclosOperados);
        cmd.Parameters.AddWithValue("@ProxMantenimientoCiclos", maquina.ProxMantenimientoCiclos);
        cmd.Parameters.AddWithValue("@Observacion", maquina.Observacion ?? "");
        
        var result = cmd.ExecuteScalar();
        if (result != null && int.TryParse(result.ToString(), out int newId))
        {
            maquina.IdMaquina = newId;
        }
    }

    public void Eliminar(int idMaquina)
    {
        using var conexion = new SqliteConnection(Config.ConnectionString);
        conexion.Open();

        string query = "DELETE FROM Maquinas WHERE IdMaquina = @IdMaquina";
        using var cmd = new SqliteCommand(query, conexion);
        cmd.Parameters.AddWithValue("@IdMaquina", idMaquina);
        cmd.ExecuteNonQuery();
    }
}
