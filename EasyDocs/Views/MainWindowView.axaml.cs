using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using EasyDocs.ViewModels;
using EasyDocs.Helpers;
using System.IO;
using Avalonia.VisualTree;

namespace EasyDocs.Views;

public partial class MainWindowView : Window
{
    private bool windowLostFocus;

    public MainWindowView()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(this);
        FilePopup.Opened += OnFilePopupOpened;
        ExportPopup.Opened += OnExportPopupOpened;
        windowLostFocus = false;
        this.LostFocus += OnWindowLostFocus;
        this.PointerEntered += OnWindowPointerEntered;
    }

    private void OnMenuButtonPointerOver(object? sender, RoutedEventArgs e)
    {
        if(FilePopup.IsOpen && ExportPopup.IsOpen)
        {
            ExportPopup.Close();
            FilePopup.Focus();
        }
    }

    private void OnExportPopupOpened(object? sender, EventArgs e)
    {
        ExportPopup.Focus();
    }

    private void OnWindowPointerEntered(object? sender, PointerEventArgs e)
    {
        if(!windowLostFocus) { return; }
        if(FilePopup.IsOpen && windowLostFocus)
        {
            FilePopup.Close();
            this.Focus();
        }
    }

    private void OnWindowLostFocus(object? sender, RoutedEventArgs e)
    {
        windowLostFocus = true;
    }

    private void OnFilePopupOpened(object? sender, EventArgs e)
    {
        FilePopup.Focus();
    }

    private void OnFileButtonClicked(object? sender, RoutedEventArgs e)
    {
        if(FilePopup.IsOpen)
        {
            FilePopup.Close();
        }else
        {
            FilePopup.Open();
        }        
    }

    private void OnExportButtonClicked(object? sender, RoutedEventArgs e)
    {
        ExportPopup.Open();
    }

    private void OnPopupButtonClick(object? sender, RoutedEventArgs e)
    {
        ExportPopup.Close();
        FilePopup.Close();
    }


}