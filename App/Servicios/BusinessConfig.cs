using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LavanderiaApp.Servicios;

public class BusinessConfigData
{
    public string NombreNegocio { get; set; } = "Lavanderias villas del sur";
    public string Telefono { get; set; } 
    public string Direccion { get; set; } 
    public string MensajeTicket { get; set; } = "¡Gracias por su preferencia!";
    public double Iva { get; set; } = 16.0;
    public bool IvaActivo { get; set; } = false; // por predeterminado en apagado
    public string Moneda { get; set; } = "$";
    public string Tema { get; set; } = "Claro";
    public decimal FondoCaja { get; set; } 
    public decimal CostoAplazarEntrega { get; set; } = 25.00m;
    public List<string> Sucursales { get; set; } = new List<string> { "Sucursal Norte", "Sucursal Sur" };
    public bool InventarioActivo { get; set; } = true;
}

public static class BusinessConfig
{
    private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "business-config.json");
    
    public static BusinessConfigData Current { get; private set; } = new();
    public static bool IvaActivo => Current?.IvaActivo ?? false;
    public static double Iva => Current?.Iva ?? 16.0;
    public static decimal CostoAplazarEntrega => Current?.CostoAplazarEntrega ?? 25.00m;

    static BusinessConfig()
    {
        Cargar();
    }

    public static void Cargar()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                string json = File.ReadAllText(ConfigPath);
                var data = JsonSerializer.Deserialize<BusinessConfigData>(json);
                if (data != null)
                {
                    Current = data;
                }
            }
        }
        catch
        {
            // En caso de error, mantener la configuración por defecto
        }
    }

    public static event Action? OnConfigUpdated;

    public static void TriggerConfigUpdated()
    {
        OnConfigUpdated?.Invoke();
    }

    public static void Guardar()
    {
        try
        {
            string json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
            OnConfigUpdated?.Invoke();
        }
        catch
        {
            // Ignorar excepción en caso de que el archivo esté en uso
        }
    }
}
