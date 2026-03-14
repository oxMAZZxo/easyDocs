using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using EasyDocs.ViewModels;

namespace EasyDocs.Views;

public partial class SettingsWindowView : Window
{
    public SettingsWindowView()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel(this);
        Closing += OnWindowClosing;
    }

    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = false;
        if (App.Instance == null)
        {
            return;
        }

        if(!App.Instance.IsShuttingDown)
        {
            e.Cancel = true;
            this.Hide();              
        }
    }

    private void OnCloseButtonClicked(object? sender, RoutedEventArgs e)
    {
        this.Hide();
    }

    private void OnChromeBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        this.BeginMoveDrag(e);
    }

    private void OnChromeBarDoubleTap(object? sender, TappedEventArgs e)
    {
        if (WindowState == WindowState.Normal)
        {
            WindowState = WindowState.Maximized;
            
        }else if(WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
        }
    }
}