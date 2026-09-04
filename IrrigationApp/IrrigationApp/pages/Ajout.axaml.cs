using System;
using System.Linq;
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
		var shouldUpdatePrevious = departProvided && previousForAppareil is not null && previousForAppareil.M3a == 0;
		var previousArrivee = 0;
		var previousConsomation = 0;

		int depart = 0;
		if (string.IsNullOrWhiteSpace(m3d.Text))
		{
			depart = previousForAppareil?.M3a ?? 0;
		}
		else if (!int.TryParse(m3d.Text, out depart))
		{
			return;
		}

		if (shouldUpdatePrevious && previousForAppareil is Data previousData && depart != 0)
		{
			previousArrivee = depart;
			previousConsomation = previousArrivee >= previousData.M3d
				? previousArrivee - previousData.M3d
				: 0;

			var updateUrl = AppState.ServerBaseUrl +
				$"?id={previousData.Id}" +
				$"&date={Uri.EscapeDataString(previousData.Date)}" +
				$"&parcelle={Uri.EscapeDataString(previousData.Parcelle)}" +
				$"&appareil={Uri.EscapeDataString(previousData.Appareil)}" +
				$"&m3d={previousData.M3d}" +
				$"&m3a={previousArrivee}" +
				$"&consomation={previousConsomation}" +
				$"&reseau={Uri.EscapeDataString(previousData.Reseau)}" +
				$"&commentaire={Uri.EscapeDataString(previousData.Commentaire)}";

			if (!ServerAccess.TrySend(HttpMethod.Put, updateUrl, "update previous data on server", out _))
			{
				ReturnToMenuAfterConnectionError();
				return;
			}

			previousForAppareil.M3a = previousArrivee;
			previousForAppareil.Consomation = previousConsomation;
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

		var selectedDate = datePicker.SelectedDate ?? DateTime.Now;
		var reseauValue = reseau.SelectedItem?.ToString() ?? string.Empty;
		var parcelleValue = parcelle.Text ?? string.Empty;
		var commentaireValue = commentaire.Text ?? string.Empty;
		var nextId = (AppState.DataList?.Count ?? 0) + 1;

		var data = new Data
		{
			Id = nextId,
			Date = selectedDate.ToString("dd/MM/yyyy"),
			Parcelle = parcelleValue,
			Appareil = appareilValue,
			M3d = depart,
			M3a = arrivee,
			Consomation = arrivee >= depart ? arrivee - depart : 0,
			Reseau = reseauValue,
			Commentaire = commentaireValue
		};

		var createUrl = AppState.ServerBaseUrl + $"?date={Uri.EscapeDataString(data.Date)}&parcelle={Uri.EscapeDataString(data.Parcelle)}&appareil={Uri.EscapeDataString(data.Appareil)}&m3d={data.M3d}&m3a={data.M3a}&consomation={data.Consomation}&reseau={Uri.EscapeDataString(data.Reseau)}&commentaire={Uri.EscapeDataString(data.Commentaire)}";
		if (!ServerAccess.TrySend(HttpMethod.Post, createUrl, "send new data to server", out var responseContent))
		{
			ReturnToMenuAfterConnectionError();
			return;
		}

		Console.WriteLine("Response from server: " + responseContent);

		if (shouldUpdatePrevious && previousForAppareil is not null)
		{
			previousForAppareil.M3a = previousArrivee;
			previousForAppareil.Consomation = previousConsomation;
		}

		AppState.DataList ??= new System.Collections.Generic.List<Data>();
		AppState.DataList.Add(data);

		parcelle.Text = string.Empty;
		m3d.Text = string.Empty;
		m3a.Text = string.Empty;
		datePicker.SelectedDate = DateTime.Now;
		appareil.SelectedIndex = 0;
		reseau.SelectedIndex = 0;
		commentaire.Text = string.Empty;


		//Console.WriteLine($"Data added: Id={data.Id}, Date={data.Date}, Parcelle={data.Parcelle}, Appareil={data.Appareil}, M3d={data.M3d}, M3a={data.M3a}, Consomation={data.Consomation}, Reseau={data.Reseau}");
		BackRequested?.Invoke(this, EventArgs.Empty);
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
