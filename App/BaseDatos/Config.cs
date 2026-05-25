using System;
using System.IO;

namespace LavanderiaApp;

public static class Config
{
    // Esto asegura que la base de datos se cree en la carpeta donde se ejecuta el programa
    public static string ConnectionString => $"Data Source={DbPath}";
    
    public static string DbPath 
    {
        get 
        {
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(folder, "lavanderia.db");
        }
    }
}
