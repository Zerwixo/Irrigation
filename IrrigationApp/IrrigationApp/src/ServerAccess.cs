using System;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls.Shapes;

namespace IrrigationApp;

internal static class ServerAccess
{
	private static bool _isShowingConnectionError;

	public static bool TrySend(HttpMethod method, string url, string operation, out string responseContent)
	{
		try
		{
			using var client = new HttpClient();
			using var request = new HttpRequestMessage(method, url);
			using var response = client.SendAsync(request).GetAwaiter().GetResult();
			response.EnsureSuccessStatusCode();
			responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			return true;
		}
		catch (HttpRequestException ex)
		{
			Console.WriteLine($"Failed to {operation}: {ex.Message}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Unexpected error while trying to {operation}: {ex.Message}");
		}

		responseContent = string.Empty;
		return false;
	}

	public static bool TryGetString(string url, string operation, out string responseContent)
	{
		return TrySend(HttpMethod.Get, url, operation, out responseContent);
	}

	public static void HandleConnectionFailure(Control owner, Action? onDismissed)
	{
		_ = HandleConnectionFailureAsync(owner, onDismissed);
	}

	private static async Task HandleConnectionFailureAsync(Control owner, Action? onDismissed)
	{
		if (_isShowingConnectionError)
		{
			return;
		}

		_isShowingConnectionError = true;

		await ShowConnectionErrorAsync(owner);
		try
		{
			onDismissed?.Invoke();
		}
		finally
		{
			_isShowingConnectionError = false;
		}
	}

	private static async Task ShowConnectionErrorAsync(Control owner)
	{
		var dialog = CreateConnectionErrorDialog();

		if (TopLevel.GetTopLevel(owner) is Window parentWindow)
		{
			await dialog.ShowDialog(parentWindow);
			return;
		}

		dialog.Show();
	}

	private static Window CreateConnectionErrorDialog()
	{
		var dialog = new Window
		{
			Title = "Erreur de connexion",
			Width = 440,
			CanResize = false,
			SizeToContent = SizeToContent.Height,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			Background = Brushes.White,
			FontFamily = new FontFamily("Segoe UI")
		};

		var okButton = new Button
		{
			Content = "OK",
			MinWidth = 100,
			HorizontalAlignment = HorizontalAlignment.Right,
			Padding = new Thickness(18, 6),
			Background = new SolidColorBrush(Color.Parse("#F3F3F3")),
			BorderBrush = new SolidColorBrush(Color.Parse("#ADADAD")),
			BorderThickness = new Thickness(1)
		};

		okButton.Click += (_, _) => dialog.Close();

		dialog.Content = new DockPanel
		{
			Children =
			{
				new Border
				{
					Background = new SolidColorBrush(Color.Parse("#F0F0F0")),
					BorderBrush = new SolidColorBrush(Color.Parse("#D4D4D4")),
					BorderThickness = new Thickness(0, 1, 0, 0),
					Padding = new Thickness(16, 12),
					[DockPanel.DockProperty] = Dock.Bottom,
					Child = new Grid
					{
						ColumnDefinitions = new ColumnDefinitions("*,Auto"),
						Children =
						{
							okButton
						}
					}
				},
				new Grid
				{
					Margin = new Thickness(20, 18, 20, 18),
					ColumnDefinitions = new ColumnDefinitions("Auto,*"),
					Children =
					{
						new Border
						{
							Width = 32,
							Height = 32,
							CornerRadius = new CornerRadius(16),
							Background = new SolidColorBrush(Color.Parse("#FFF4CE")),
							BorderBrush = new SolidColorBrush(Color.Parse("#D6B656")),
							BorderThickness = new Thickness(1),
							VerticalAlignment = VerticalAlignment.Top,
							Child = new TextBlock
							{
								Text = "!",
								FontSize = 20,
								FontWeight = FontWeight.Bold,
								HorizontalAlignment = HorizontalAlignment.Center,
								VerticalAlignment = VerticalAlignment.Center,
								Foreground = new SolidColorBrush(Color.Parse("#7A5C00"))
							}
						},
						new StackPanel
						{
							Margin = new Thickness(16, 0, 0, 0),
							Spacing = 8,
							[Grid.ColumnProperty] = 1,
							Children =
							{
								new TextBlock
								{
									Text = "Connexion au serveur impossible",
									FontSize = 16,
									FontWeight = FontWeight.SemiBold,
									Foreground = Brushes.Black
								},
								new TextBlock
								{
									Text = "Les donnees n'ont pas ete enregistrees. Verifiez que le serveur est lance, puis recommencez. Vous allez revenir au menu.",
									TextWrapping = TextWrapping.Wrap,
									Foreground = new SolidColorBrush(Color.Parse("#222222"))
								}
							}
						}
					}
				}
			}
		};

		return dialog;
	}
}