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
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (baseDir.Contains("App.Tests", StringComparison.OrdinalIgnoreCase) || 
                AppDomain.CurrentDomain.FriendlyName.Contains("testhost", StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(baseDir, "lavanderia.db");
            }
            
#if DEBUG
            return @"C:\Users\Yadie\RiderProjects\LavanderiaApp0.1\App\BaseDatos\lavanderia.db";
#else
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LavanderiaApp");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, "lavanderia.db");
#endif
        }
    }
}
