namespace IrrigationApp;

using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;

public partial class Modifier : UserControl
{
    public event EventHandler? BackRequested;
	private Data? _editingData;

    public Modifier()
	{
		InitializeComponent();
		RefreshData();
	}

	public void RefreshData()
	{
		var items = (AppState.DataList ?? new System.Collections.Generic.List<Data>())
			.OrderByDescending(d => d.Id)
			.Take(10)
			.ToArray();

		RecentItemsControl.ItemsSource = items;
	}

	private void StartEdit(object? sender, RoutedEventArgs e)
	{
		if (sender is not Button button || button.Tag is not int id)
		{
			return;
		}

		_editingData = AppState.DataList?.FirstOrDefault(d => d.Id == id);
		if (_editingData is null)
		{
			return;
		}

		EditTitle.Text = $"Edition de l'element #{_editingData.Id}";
		editParcelle.Text = _editingData.Parcelle;
		editM3d.Text = _editingData.M3d.ToString();
		editM3a.Text = _editingData.M3a.ToString();
		editCommentaire.Text = _editingData.Commentaire;

		if (DateTime.TryParseExact(_editingData.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
		{
			editDatePicker.SelectedDate = parsedDate;
		}
		else
		{
			editDatePicker.SelectedDate = DateTime.Now;
		}

		SelectComboValue(editAppareil, _editingData.Appareil);
		SelectComboValue(editReseau, _editingData.Reseau);

		EditPanel.IsVisible = true;
	}

	private void SaveEdit(object? sender, RoutedEventArgs e)
	{
		if (_editingData is null)
		{
			return;
		}

		if (!int.TryParse(editM3d.Text, out var depart))
		{
			return;
		}

		if (!int.TryParse(editM3a.Text, out var arrivee))
		{
			return;
		}

		if (arrivee < depart)
		{
			return;
		}

		var selectedDate = editDatePicker.SelectedDate ?? DateTime.Now;
		_editingData.Date = selectedDate.ToString("dd/MM/yyyy");
		_editingData.Parcelle = editParcelle.Text ?? string.Empty;
		_editingData.Appareil = editAppareil.SelectedItem?.ToString() ?? string.Empty;
		_editingData.M3d = depart;
		_editingData.M3a = arrivee;
		_editingData.Consomation = arrivee - depart;
		_editingData.Reseau = editReseau.SelectedItem?.ToString() ?? string.Empty;
		_editingData.Commentaire = editCommentaire.Text ?? string.Empty;

		var requestUrl = "http://localhost:8080/" +
			$"?id={_editingData.Id}" +
			$"&date={Uri.EscapeDataString(_editingData.Date)}" +
			$"&parcelle={Uri.EscapeDataString(_editingData.Parcelle)}" +
			$"&appareil={Uri.EscapeDataString(_editingData.Appareil)}" +
			$"&m3d={_editingData.M3d}" +
			$"&m3a={_editingData.M3a}" +
			$"&consomation={_editingData.Consomation}" +
			$"&reseau={Uri.EscapeDataString(_editingData.Reseau)}" +
			$"&commentaire={Uri.EscapeDataString(_editingData.Commentaire)}";

		try
		{
			using var client = new HttpClient();
			using var request = new HttpRequestMessage(HttpMethod.Put, requestUrl);
			using var response = client.SendAsync(request).GetAwaiter().GetResult();
			using var reader = new StreamReader(response.Content.ReadAsStream());
			_ = reader.ReadToEnd();
		}
		catch (HttpRequestException ex)
		{
			Console.WriteLine($"Failed to update data on server: {ex.Message}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Unexpected error while updating data on server: {ex.Message}");
		}

		_editingData = null;
		EditPanel.IsVisible = false;
		RefreshData();
	}

	private void CancelEdit(object? sender, RoutedEventArgs e)
	{
		_editingData = null;
		EditPanel.IsVisible = false;
	}

	private void DeleteItem(object? sender, RoutedEventArgs e)
	{
		if (sender is not Button button || button.Tag is not int id)
		{
			return;
		}

		var data = AppState.DataList?.FirstOrDefault(d => d.Id == id);
		if (data is null)
		{
			return;
		}

		try
		{
			var requestUrl = $"http://localhost:8080/?id={id}";
			using var client = new HttpClient();
			using var request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
			using var response = client.SendAsync(request).GetAwaiter().GetResult();
			using var reader = new StreamReader(response.Content.ReadAsStream());
			_ = reader.ReadToEnd();
		}
		catch (HttpRequestException ex)
		{
			Console.WriteLine($"Failed to delete data on server: {ex.Message}");
			return;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Unexpected error while deleting data on server: {ex.Message}");
			return;
		}

		AppState.DataList?.Remove(data);

		if (_editingData?.Id == id)
		{
			_editingData = null;
			EditPanel.IsVisible = false;
		}

		RefreshData();
	}

	private static void SelectComboValue(ComboBox comboBox, string value)
	{
		if (comboBox.Items is null)
		{
			comboBox.SelectedIndex = 0;
			return;
		}

		foreach (var item in comboBox.Items)
		{
			if (string.Equals(item?.ToString(), value, StringComparison.Ordinal))
			{
				comboBox.SelectedItem = item;
				return;
			}
		}

		comboBox.SelectedIndex = 0;
	}

    private void RetourMenu(object? sender, RoutedEventArgs e)
	{
		BackRequested?.Invoke(this, EventArgs.Empty);
	}
}
