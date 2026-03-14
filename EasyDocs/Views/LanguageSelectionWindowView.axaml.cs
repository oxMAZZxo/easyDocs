using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EasyDocs.Models;

namespace EasyDocs.Views;

public partial class LanguageSelectionWindowView : Window
{
    public ProgLanguage SelectedProgrammingLanguage { get; private set; }
    public bool ValidSelection { get; private set; }

    public LanguageSelectionWindowView()
    {
        InitializeComponent();
        Closing += OnWindowClosing;
        ProgLangOptions.ItemsSource = Enum.GetValues(typeof(ProgLanguage));
        ValidSelection = false;
    }

    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = false;
        if(App.Instance == null){return;}

        if(!App.Instance.IsShuttingDown)
        {
            e.Cancel = true;
            Hide();
        }
    }

    public void OnOkayButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (ProgLangOptions.SelectedIndex > -1)
        {
            ValidSelection = true;
        }
        SelectedProgrammingLanguage = (ProgLanguage)ProgLangOptions.SelectedIndex;
        Hide();
    }

    public void OnCancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        ValidSelection = false;
        Hide();
    }
}