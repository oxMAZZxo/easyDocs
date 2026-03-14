using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using EasyDocs.Views;
using Avalonia.Controls;
using PdfSharp.Fonts;
using EasyDocs.Helpers;
using System;

namespace EasyDocs;

public partial class App : Application
{
    public static App? Instance { get; private set; }
    public TopLevel? TopLevel { get; private set; }
    public bool IsShuttingDown { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Instance = this;
        GlobalFontSettings.FontResolver = new SimpleFontResolver();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindowView();
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            desktop.MainWindow.Closing += OnMainWindowClosing;
            TopLevel = TopLevel.GetTopLevel(desktop.MainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        IsShuttingDown = true;
        if(sender == null) { return; }

        Window window = (Window)sender;
        window.Closing -= OnMainWindowClosing;
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}