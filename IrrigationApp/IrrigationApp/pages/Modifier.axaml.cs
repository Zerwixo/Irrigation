namespace IrrigationApp;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;

public partial class Modifier : UserControl
{
    public event EventHandler? BackRequested;
	private Data? _editingData;
	private Border? _activeEditPanel;
	private DatePicker? _activeDatePicker;
	private TextBox? _activeParcelle;
	private ComboBox? _activeAppareil;
	private TextBox? _activeM3d;
	private TextBox? _activeM3a;
	private ComboBox? _activeReseau;
	private TextBox? _activeCommentaire;
	private TextBlock? _activeEditTitle;

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
		if (sender is not Button button || button.Tag is not Data selectedData)
		{
			return;
		}

		_editingData = selectedData;
		if (_editingData is null)
		{
			return;
		}

		var itemContainer = button.FindAncestorOfType<Border>();
		if (itemContainer is null)
		{
			return;
		}

		var inlineEditPanel = FindDescendantByName<Border>(itemContainer, "InlineEditPanel");
		if (inlineEditPanel is null)
		{
			return;
		}

		var inlineEditTitle = FindDescendantByName<TextBlock>(inlineEditPanel, "InlineEditTitle");
		var inlineEditDatePicker = FindDescendantByName<DatePicker>(inlineEditPanel, "InlineEditDatePicker");
		var inlineEditParcelle = FindDescendantByName<TextBox>(inlineEditPanel, "InlineEditParcelle");
		var inlineEditAppareil = FindDescendantByName<ComboBox>(inlineEditPanel, "InlineEditAppareil");
		var inlineEditM3d = FindDescendantByName<TextBox>(inlineEditPanel, "InlineEditM3d");
		var inlineEditM3a = FindDescendantByName<TextBox>(inlineEditPanel, "InlineEditM3a");
		var inlineEditReseau = FindDescendantByName<ComboBox>(inlineEditPanel, "InlineEditReseau");
		var inlineEditCommentaire = FindDescendantByName<TextBox>(inlineEditPanel, "InlineEditCommentaire");

		if (inlineEditTitle is null || inlineEditDatePicker is null || inlineEditParcelle is null || inlineEditAppareil is null || inlineEditM3d is null || inlineEditM3a is null || inlineEditReseau is null || inlineEditCommentaire is null)
		{
			return;
		}

		if (_activeEditPanel is not null && _activeEditPanel != inlineEditPanel)
		{
			_activeEditPanel.IsVisible = false;
		}

		_activeEditPanel = inlineEditPanel;
		_activeEditTitle = inlineEditTitle;
		_activeDatePicker = inlineEditDatePicker;
		_activeParcelle = inlineEditParcelle;
		_activeAppareil = inlineEditAppareil;
		_activeM3d = inlineEditM3d;
		_activeM3a = inlineEditM3a;
		_activeReseau = inlineEditReseau;
		_activeCommentaire = inlineEditCommentaire;

		_activeEditTitle.Text = $"Edition de l'element #{_editingData.Id}";
		_activeParcelle.Text = _editingData.Parcelle;
		_activeM3d.Text = _editingData.M3d.ToString();
		_activeM3a.Text = _editingData.M3a.ToString();
		_activeCommentaire.Text = _editingData.Commentaire;

		if (DateTime.TryParseExact(_editingData.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
		{
			_activeDatePicker.SelectedDate = parsedDate;
		}
		else
		{
			_activeDatePicker.SelectedDate = DateTime.Now;
		}

		SelectComboValue(_activeAppareil, _editingData.Appareil);
		SelectComboValue(_activeReseau, _editingData.Reseau);

		_activeEditPanel.IsVisible = true;
	}

	private void SaveEdit(object? sender, RoutedEventArgs e)
	{
		if (_editingData is null || _activeDatePicker is null || _activeParcelle is null || _activeAppareil is null || _activeM3d is null || _activeM3a is null || _activeReseau is null || _activeCommentaire is null)
		{
			return;
		}

		if (!int.TryParse(_activeM3d.Text, out var depart))
		{
			return;
		}

		int arrivee;
		if (string.IsNullOrWhiteSpace(_activeM3a.Text))
		{
			arrivee = 0;
		}
		else if (!int.TryParse(_activeM3a.Text, out arrivee))
		{
			return;
		}

		if (arrivee != 0 && arrivee < depart)
		{
			return;
		}

		var selectedDate = _activeDatePicker.SelectedDate ?? DateTime.Now;
		var updatedDate = selectedDate.ToString("dd/MM/yyyy");
		var updatedParcelle = _activeParcelle.Text ?? string.Empty;
		var updatedAppareil = _activeAppareil.SelectedItem?.ToString() ?? string.Empty;
		var updatedConsomation = arrivee >= depart ? arrivee - depart : 0;
		var updatedReseau = _activeReseau.SelectedItem?.ToString() ?? string.Empty;
		var updatedCommentaire = _activeCommentaire.Text ?? string.Empty;

		var requestUrl = AppState.ServerBaseUrl +
			$"?id={_editingData.Id}" +
			$"&date={Uri.EscapeDataString(updatedDate)}" +
			$"&parcelle={Uri.EscapeDataString(updatedParcelle)}" +
			$"&appareil={Uri.EscapeDataString(updatedAppareil)}" +
			$"&m3d={depart}" +
			$"&m3a={arrivee}" +
			$"&consomation={updatedConsomation}" +
			$"&reseau={Uri.EscapeDataString(updatedReseau)}" +
			$"&commentaire={Uri.EscapeDataString(updatedCommentaire)}";

		if (!ServerAccess.TrySend(HttpMethod.Put, requestUrl, "update data on server", out _))
		{
			ReturnToMenuAfterConnectionError();
			return;
		}

		_editingData.Date = updatedDate;
		_editingData.Parcelle = updatedParcelle;
		_editingData.Appareil = updatedAppareil;
		_editingData.M3d = depart;
		_editingData.M3a = arrivee;
		_editingData.Consomation = updatedConsomation;
		_editingData.Reseau = updatedReseau;
		_editingData.Commentaire = updatedCommentaire;

		CloseActiveEditPanel();
		RefreshData();
	}

	private void CancelEdit(object? sender, RoutedEventArgs e)
	{
		CloseActiveEditPanel();
	}

	private void DeleteItem(object? sender, RoutedEventArgs e)
	{
		if (sender is not Button button || button.Tag is not Data data)
		{
			return;
		}
		var id = data.Id;

		try
		{
			var requestUrl = $"{AppState.ServerBaseUrl}?id={id}";
			if (!ServerAccess.TrySend(HttpMethod.Delete, requestUrl, "delete data on server", out _))
			{
				ReturnToMenuAfterConnectionError();
				return;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Unexpected error while preparing data deletion: {ex.Message}");
			ReturnToMenuAfterConnectionError();
			return;
		}

		AppState.DataList?.Remove(data);

		if (_editingData?.Id == id)
		{
			CloseActiveEditPanel();
		}

		RefreshData();
	}

	private void CloseActiveEditPanel()
	{
		_editingData = null;

		if (_activeEditPanel is not null)
		{
			_activeEditPanel.IsVisible = false;
		}

		_activeEditPanel = null;
		_activeDatePicker = null;
		_activeParcelle = null;
		_activeAppareil = null;
		_activeM3d = null;
		_activeM3a = null;
		_activeReseau = null;
		_activeCommentaire = null;
		_activeEditTitle = null;
	}

	private static T? FindDescendantByName<T>(Control root, string name) where T : Control
	{
		return root
			.GetVisualDescendants()
			.OfType<T>()
			.FirstOrDefault(control => string.Equals(control.Name, name, StringComparison.Ordinal));
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

	private void ReturnToMenuAfterConnectionError()
	{
		ServerAccess.HandleConnectionFailure(this, OnConnectionErrorDismissed);
	}

	private void OnConnectionErrorDismissed()
	{
		BackRequested?.Invoke(this, EventArgs.Empty);
	}
}
