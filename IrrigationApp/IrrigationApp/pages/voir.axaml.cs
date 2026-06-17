
using System;
using System.Linq;
namespace IrrigationApp;

using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;

public partial class Voir : UserControl
{
	public event EventHandler? BackRequested;

	public Voir()
	{
		InitializeComponent();
		RefreshData();
	}

	public void RefreshData()
	{
		try
		{
			using var client = new HttpClient();
			var content = client.GetStringAsync("http://localhost:8080/").GetAwaiter().GetResult();
			Console.WriteLine("Response from server: " + content);
			AppState.DataList = JsonSerializer.Deserialize<List<Data>>(content) ?? [];
		}
		catch (HttpRequestException ex)
		{
			Console.WriteLine($"Failed to load data from server: {ex.Message}");
			AppState.DataList ??= [];
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Unexpected error while loading data from server: {ex.Message}");
			AppState.DataList ??= [];
		}
		
		DataItemsControl.ItemsSource = AppState.DataList?.OrderByDescending(d => d.Id).ToArray() ?? Array.Empty<Data>();
	}

	private void RetourMenu(object? sender, RoutedEventArgs e)
	{
		BackRequested?.Invoke(this, EventArgs.Empty);
	}
}
