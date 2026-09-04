using System;
using System.Linq;
namespace IrrigationApp;

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
	}

	public void RefreshData()
	{
		try
		{
			if (!ServerAccess.TryGetString(AppState.ServerBaseUrl, "load data from server", out var content))
			{
				AppState.DataList ??= [];
				ReturnToMenuAfterConnectionError();
				return;
			}

			Console.WriteLine("Response from server: " + content);
			AppState.DataList = JsonSerializer.Deserialize<List<Data>>(content) ?? [];
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Unexpected error while loading data from server: {ex.Message}");
			AppState.DataList ??= [];
			ReturnToMenuAfterConnectionError();
			return;
		}
		
		DataItemsControl.ItemsSource = AppState.DataList?.OrderByDescending(d => d.Id).ToArray() ?? Array.Empty<Data>();
	}

	private void RetourMenu(object? sender, RoutedEventArgs e)
	{
		BackRequested?.Invoke(this, EventArgs.Empty);
	}

	private void ReturnToMenuAfterConnectionError()
	{
		ServerAccess.HandleConnectionFailure(this, OnConnectionErrorDismissed);
	}

	private void OnConnectionErrorDismissed()
	{
		BackRequested?.Invoke(this, EventArgs.Empty);
	}
}
