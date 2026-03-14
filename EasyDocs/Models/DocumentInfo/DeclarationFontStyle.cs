using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasyDocs.Models.DocumentInfo;

/// <summary>
/// A Font Declaration Style object represents how a specific declaration will be rendered onto a PDF Document.
/// </summary>
public class FontDeclarationStyle : INotifyPropertyChanged
{
    private string fontSizeString = "";
    private bool isItalic;
    private bool isBold;
    private string spaceAfterString = "";

    /// <summary>
    /// The Font Size (in a string) of this declaration. 
    /// </summary>
    public string FontSizeString
    {
        get => fontSizeString;
        set
        {
            fontSizeString = value;
            SetFontSize();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The font size of this declaration.
    /// </summary>
    public int FontSize { get; private set; }

    public bool IsItalic
    {
        get => isItalic;
        set
        {
            isItalic = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Is this declaration bold.
    /// </summary>
    public bool IsBold
    {
        get => isBold;
        set
        {
            isBold = value;

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The space after this line (in a string).
    /// </summary>
    public string SpaceAfterString
    {
        get => spaceAfterString;
        set
        {
            spaceAfterString = value;
            SetSpaceAfter();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// The space after this line.
    /// </summary>
    public int SpaceAfter { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public FontDeclarationStyle(string fontSizeString, bool isItalic, bool isBold, string spaceAfterString)
    {
        FontSizeString = fontSizeString;
        SpaceAfterString = spaceAfterString;
        IsItalic = isItalic;
        IsBold = isBold;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private void SetSpaceAfter()
    {
        if (string.IsNullOrEmpty(SpaceAfterString) || string.IsNullOrWhiteSpace(SpaceAfterString))
        {
            SpaceAfter = 1;
            return;
        }
        SpaceAfter = Convert.ToInt32(SpaceAfterString);
    }

    private void SetFontSize()
    {
        if (string.IsNullOrEmpty(FontSizeString) || string.IsNullOrWhiteSpace(FontSizeString))
        {
            FontSize = 1;
            return;
        }
        FontSize = Convert.ToInt32(FontSizeString);
    }

    public string GetValuesInStrings()
    {
        return $"{FontSize},{IsItalic},{IsBold},{SpaceAfter}";
    }
}