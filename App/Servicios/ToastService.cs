using System;

namespace LavanderiaApp.Servicios;

public static class ToastService
{
    public static event Action<string, string, string>? OnShowToast;

    public static void Show(string title, string message, string type = "success")
    {
        OnShowToast?.Invoke(title, message, type);
    }
}
