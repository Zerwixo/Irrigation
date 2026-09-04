using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace IrrigationApp;

public sealed class ConsomationRow
{
    public string Appareil { get; set; } = string.Empty;
    public int Consomation { get; set; }
}

public partial class Consomation : UserControl
{
    public event EventHandler? BackRequested;

    public Consomation()
    {
        InitializeComponent();
        RefreshData();
    }

    public void RefreshData()
    {
        var rows = (AppState.DataList ?? [])
            .GroupBy(item => string.IsNullOrWhiteSpace(item.Appareil) ? "(Sans appareil)" : item.Appareil)
            .Select(group => new ConsomationRow
            {
                Appareil = group.Key,
                Consomation = group.Sum(item => item.Consomation)
            })
            .OrderBy(row => row.Appareil)
            .ToList();

        ConsomationItemsControl.ItemsSource = rows;

        var total = rows.Sum(row => row.Consomation);
        TotalConsomationText.Text = total.ToString();
    }

    private void RetourMenu(object? sender, RoutedEventArgs e)
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }
}
