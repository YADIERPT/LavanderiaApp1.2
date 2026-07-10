using System;

namespace LavanderiaApp.Servicios;

public static class CustomMessageBox
{
    public static event Action<string, string, string, Action?>? OnShow;

    public static void Show(string title, string message, string type = "info", Action? onConfirm = null)
    {
        OnShow?.Invoke(title, message, type, onConfirm);
    }
}
