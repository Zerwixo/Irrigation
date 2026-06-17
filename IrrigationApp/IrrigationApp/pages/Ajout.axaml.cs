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
		var appareilValue = appareil.SelectedItem?.ToString() ?? string.Empty;
		var departProvided = !string.IsNullOrWhiteSpace(m3d.Text);
		var previousForAppareil = AppState.DataList?.LastOrDefault(item => item.Appareil == appareilValue);

		int depart = 0;
		if (string.IsNullOrWhiteSpace(m3d.Text))
		{
			depart = previousForAppareil?.M3a ?? 0;
		}
		else if (!int.TryParse(m3d.Text, out depart))
		{
			return;
		}

		if (departProvided && depart != 0 && previousForAppareil is not null && previousForAppareil.M3a == 0)
		{
			previousForAppareil.M3a = depart;
			previousForAppareil.Consomation = previousForAppareil.M3a >= previousForAppareil.M3d
				? previousForAppareil.M3a - previousForAppareil.M3d
				: 0;

			var updateUrl = "http://localhost:8080/" +
				$"?id={previousForAppareil.Id}" +
				$"&date={Uri.EscapeDataString(previousForAppareil.Date)}" +
				$"&parcelle={Uri.EscapeDataString(previousForAppareil.Parcelle)}" +
				$"&appareil={Uri.EscapeDataString(previousForAppareil.Appareil)}" +
				$"&m3d={previousForAppareil.M3d}" +
				$"&m3a={previousForAppareil.M3a}" +
				$"&consomation={previousForAppareil.Consomation}" +
				$"&reseau={Uri.EscapeDataString(previousForAppareil.Reseau)}" +
				$"&commentaire={Uri.EscapeDataString(previousForAppareil.Commentaire)}";

			try
			{
				using var client = new HttpClient();
				using var request = new HttpRequestMessage(HttpMethod.Put, updateUrl);
				using var response = client.SendAsync(request).GetAwaiter().GetResult();
				using var reader = new StreamReader(response.Content.ReadAsStream());
				_ = reader.ReadToEnd();
			}
			catch (HttpRequestException ex)
			{
				Console.WriteLine($"Failed to update previous data on server: {ex.Message}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Unexpected error while updating previous data on server: {ex.Message}");
			}
		}

		int arrivee = 0;
		var arriveeProvided = !string.IsNullOrWhiteSpace(m3a.Text);
		if (arriveeProvided && !int.TryParse(m3a.Text, out arrivee))
		{
			return;
		}

		if (arriveeProvided && arrivee < depart)
		{
			return;
		}

		AppState.DataList ??= new System.Collections.Generic.List<Data>();

		var selectedDate = datePicker.SelectedDate ?? DateTime.Now;
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
			Consomation = arrivee >= depart ? arrivee - depart : 0,
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
