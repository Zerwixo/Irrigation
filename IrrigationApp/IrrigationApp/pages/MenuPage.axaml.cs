using System;
using Avalonia.Controls;

namespace IrrigationApp;

public partial class MenuPage : UserControl
{
    public event EventHandler? AjoutRequested;
    public event EventHandler? ModifRequested;
    public event EventHandler? VoirRequested;
    public event EventHandler? ExcelRequested;
    public event EventHandler? QuitRequested;

    public MenuPage()
    {
        InitializeComponent();
    }

    private void AjoutData(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        AjoutRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ModifData(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ModifRequested?.Invoke(this, EventArgs.Empty);
    }

    private void VoirData(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        VoirRequested?.Invoke(this, EventArgs.Empty);
    }

    private void CreationExcel(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ExcelRequested?.Invoke(this, EventArgs.Empty);
    }

    private void QuitApplication(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        QuitRequested?.Invoke(this, EventArgs.Empty);
    }
}

