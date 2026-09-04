using System.Collections.Generic;
using Avalonia.Controls;
using IrrigationApp;

namespace IrrigationApp.Views;

public partial class MainView : UserControl
{
    private readonly MenuPage _menuPage;
    private readonly Ajout _ajoutPage;
    private readonly Voir _voirPage;
    private readonly Modifier _modifierPage;
    private readonly Consomation _consomationPage;

    public MainView()
    {
        InitializeComponent();

        AppState.DataList ??= new List<Data>();

        _menuPage = new MenuPage();
        _ajoutPage = new Ajout();
        _voirPage = new Voir();
        _modifierPage = new Modifier();
        _consomationPage = new Consomation();

        _menuPage.AjoutRequested += (_, _) => ShowAjoutPage();
        _menuPage.ModifRequested += (_, _) => ShowModifierPage();
        _menuPage.VoirRequested += (_, _) => ShowVoirPage();
        _menuPage.ConsomationRequested += (_, _) => ShowConsomationPage();
        _menuPage.ExcelRequested += async (_, _) =>
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null)
            {
                return;
            }

            await Excel.ExportAsync(topLevel, AppState.DataList ?? new List<Data>());
        };
        _menuPage.QuitRequested += (_, _) =>
        {
            if (TopLevel.GetTopLevel(this) is Window window)
            {
                window.Close();
            }
        };

        _ajoutPage.BackRequested += (_, _) => ShowMenuPage();
        _voirPage.BackRequested += (_, _) => ShowMenuPage();
        _modifierPage.BackRequested += (_, _) => ShowMenuPage();
        _consomationPage.BackRequested += (_, _) => ShowMenuPage();

        ShowMenuPage();
    }

    private void ShowMenuPage() => PageHost.Content = _menuPage;
    private void ShowAjoutPage() => PageHost.Content = _ajoutPage;

    private void ShowVoirPage()
    {
        _voirPage.RefreshData();
        PageHost.Content = _voirPage;
    }

    private void ShowModifierPage()
    {
        _modifierPage.RefreshData();
        PageHost.Content = _modifierPage;
    }

    private void ShowConsomationPage()
    {
        _consomationPage.RefreshData();
        PageHost.Content = _consomationPage;
    }
}