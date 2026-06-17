using System;
using System.Linq;
using System.IO;
using System.Net.Http;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace IrrigationApp;


public partial class Ajout : UserControl
{
	public event EventHandler? BackRequested;

	public Ajout()
	{
		InitializeComponent();
		datePicker.SelectedDate = DateTime.Now;
	}

	private void Valide(object? sender, RoutedEventArgs e)
	{
		if (!int.TryParse(m3d.Text, out var depart))
		{
			if (AppState.DataList != null && AppState.DataList.Where(item => item.Appareil == appareil.SelectedItem?.ToString()).ToList().Count > 0)
			{
				depart = AppState.DataList.Where(item => item.Appareil == appareil.SelectedItem?.ToString()).ToList()[^1].M3a;
			}
			else
			{
				depart = 0;
			}
		}

		if (!int.TryParse(m3a.Text, out var arrivee))
		{
			return;
		}

		if (arrivee < depart)
		{
			return;
		}

		AppState.DataList ??= new System.Collections.Generic.List<Data>();

		var selectedDate = datePicker.SelectedDate ?? DateTime.Now;
		var appareilValue = appareil.SelectedItem?.ToString() ?? string.Empty;
		var reseauValue = reseau.SelectedItem?.ToString() ?? string.Empty;
		var parcelleValue = parcelle.Text ?? string.Empty;
		var commentaireValue = commentaire.Text ?? string.Empty;

		var data = new Data
		{
			Id = AppState.DataList.Count + 1,
			Date = selectedDate.ToString("dd/MM/yyyy"),
			Parcelle = parcelleValue,
			Appareil = appareilValue,
			M3d = depart,
			M3a = arrivee,
			Consomation = arrivee - depart,
			Reseau = reseauValue,
			Commentaire = commentaireValue
		};

		AppState.DataList.Add(data);

		parcelle.Text = string.Empty;
		m3d.Text = string.Empty;
		m3a.Text = string.Empty;
		datePicker.SelectedDate = DateTime.Now;
		appareil.SelectedIndex = 0;
		reseau.SelectedIndex = 0;
		commentaire.Text = string.Empty;

		try
		{
			using var client = new HttpClient();
			using var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8080/" + $"?date={Uri.EscapeDataString(data.Date)}&parcelle={Uri.EscapeDataString(data.Parcelle)}&appareil={Uri.EscapeDataString(data.Appareil)}&m3d={data.M3d}&m3a={data.M3a}&consomation={data.Consomation}&reseau={Uri.EscapeDataString(data.Reseau)}&commentaire={Uri.EscapeDataString(data.Commentaire)}");
			using var response = client.SendAsync(request).GetAwaiter().GetResult();
			using var reader = new StreamReader(response.Content.ReadAsStream());
			{
	    		string content = reader.ReadToEnd();
				Console.WriteLine("Response from server: " + content);
			}
		}
		catch (HttpRequestException ex)
		{
			Console.WriteLine($"Failed to send new data to server: {ex.Message}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Unexpected error while sending new data to server: {ex.Message}");
		}


		//Console.WriteLine($"Data added: Id={data.Id}, Date={data.Date}, Parcelle={data.Parcelle}, Appareil={data.Appareil}, M3d={data.M3d}, M3a={data.M3a}, Consomation={data.Consomation}, Reseau={data.Reseau}");
		BackRequested?.Invoke(this, EventArgs.Empty);
	}

	private void RetourMenu(object? sender, RoutedEventArgs e)
	{
		BackRequested?.Invoke(this, EventArgs.Empty);
	}
}
