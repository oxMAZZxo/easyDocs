using Avalonia.Controls;
using Avalonia.Input;

namespace EasyDocs.Views.SettingsViews;

public partial class UCPdfSettings : UserControl
{
    private const string numbers = "0123456789";

    public UCPdfSettings()
    {
        InitializeComponent();
    }
    
    private void TextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender == null || e.KeySymbol == null) { return; }
        e.Handled = false;

        TextBox textBox = (TextBox)sender;
        if (e.Key == Key.Space || e.Key == Key.Tab ||
        (e.KeySymbol.ToString() == "0" && (string.IsNullOrEmpty(textBox.Text) || string.IsNullOrWhiteSpace(textBox.Text))))
        {
            e.Handled = true;
            return;
        }

        bool valid = false;
        for (int i = 0; i < numbers.Length; i++)
        {
            if (numbers[i] == e.KeySymbol.ToString()[0])
            {
                valid = true;
                break;
            }
        }

        if (!valid) { e.Handled = true; }
    }
}