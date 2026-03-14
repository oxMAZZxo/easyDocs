using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Media;
using EasyDocs.Models;
using EasyDocs.Models.DocumentInfo;
using EasyDocs.Views.SettingsViews;

namespace EasyDocs.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private UserControl currentUserControl;
    private SettingsTab selectedTab;
    private UCGeneralSettings UC_GeneralSettings;
    private UCPdfSettings UC_PdfSettings;

    public Color ClassDeclarationColour
    {
        get { return SettingsModel.Instance.ClassDeclarationColour; }
        set
        {
            SettingsModel.Instance.SetMigraDocClassDeclarationColour(value.R, value.G, value.B);
            SettingsModel.Instance.ClassDeclarationColour = value;
            OnPropertyChanged(nameof(ClassDeclarationColour));
        }
    }

    public Color PrimitiveDeclarationColour
    {
        get { return SettingsModel.Instance.PrimitiveDeclarationColour; }
        set
        {
            SettingsModel.Instance.SetMigraDocPrimitiveDeclarationColour(value.R, value.G, value.B);
            SettingsModel.Instance.PrimitiveDeclarationColour = value;
            OnPropertyChanged(nameof(PrimitiveDeclarationColour));
        }
    }

    public Color EnumDeclarationColour
    {
        get { return SettingsModel.Instance.EnumDeclarationColour; }
        set
        {
            SettingsModel.Instance.SetMigraDocEnumDeclarationColour(value.R, value.G, value.B);
            SettingsModel.Instance.EnumDeclarationColour = value;
            OnPropertyChanged(nameof(EnumDeclarationColour));
        }
    }

    public Color InterfaceDeclarationColour
    {
        get { return SettingsModel.Instance.InterfaceDeclarationColour; }
        set
        {
            SettingsModel.Instance.SetMigraDocInterfaceDeclarationColour(value.R, value.G, value.B);
            SettingsModel.Instance.InterfaceDeclarationColour = value;
            OnPropertyChanged(nameof(InterfaceDeclarationColour));
        }
    }

    public Color StructDeclarationColour
    {
        get { return SettingsModel.Instance.StructDeclarationColour; }
        set
        {
            SettingsModel.Instance.SetMigraDocStructDeclarationColour(value.R, value.G, value.B);
            SettingsModel.Instance.StructDeclarationColour = value;
            OnPropertyChanged(nameof(StructDeclarationColour));
        }
    }

    public UserControl CurrentUserControl
    {
        get
        {
            return currentUserControl;
        }
        set
        {
            currentUserControl = value;
            OnPropertyChanged(nameof(CurrentUserControl));
        }
    }

    public SettingsTab SelectedTab
    {
        get => selectedTab;
        set
        {
            selectedTab = value;
            OnPropertyChanged(nameof(SelectedTab));
            ChangeUC();
        }
    }

    public ObservableCollection<string> Fonts { get => SettingsModel.Instance.Fonts; }

    public string SelectedFont
    {
        get => SettingsModel.Instance.SelectedFont;
        set
        {
            SettingsModel.Instance.SelectedFont = value;
            OnPropertyChanged(nameof(SelectedFont));
        }
    }

    public FontDeclarationStyle ObjectDeclarationStyle
    {
        get => SettingsModel.Instance.ObjectDeclarationStyle;
        set
        {
            SettingsModel.Instance.ObjectDeclarationStyle = value;
            OnPropertyChanged(nameof(ObjectDeclarationStyle));
        }
    }

    public FontDeclarationStyle ObjectDefinitionStyle
    {
        get => SettingsModel.Instance.ObjectDefinitionStyle;
        set
        {
            SettingsModel.Instance.ObjectDefinitionStyle = value;
            OnPropertyChanged(nameof(ObjectDefinitionStyle));
        }
    }

    public FontDeclarationStyle MemberHeadingStyle
    {
        get => SettingsModel.Instance.MemberHeadingStyle;
        set
        {
            SettingsModel.Instance.MemberHeadingStyle = value;
            OnPropertyChanged(nameof(MemberHeadingStyle));
        }
    }

    public FontDeclarationStyle MemberStyle
    {
        get => SettingsModel.Instance.MemberStyle;
        set
        {
            SettingsModel.Instance.MemberStyle = value;
            OnPropertyChanged(nameof(MemberStyle));
        }
    }

    public FontDeclarationStyle MemberTypeStyle
    {
        get => SettingsModel.Instance.MemberTypeStyle;
        set
        {
            SettingsModel.Instance.MemberTypeStyle = value;
            OnPropertyChanged(nameof(MemberTypeStyle));
        }
    }

    public FontDeclarationStyle MemberDefinitionStyle
    {
        get => SettingsModel.Instance.MemberDefinitionStyle;
        set
        {
            SettingsModel.Instance.MemberDefinitionStyle = value;
            OnPropertyChanged(nameof(MemberDefinitionStyle));
        }
    }

    public bool GenerateTableOfContents
    {
        get => SettingsModel.Instance.GenerateTableOfContents;
        set
        {
            SettingsModel.Instance.GenerateTableOfContents = value;
            OnPropertyChanged(nameof(GenerateTableOfContents));
        }
    }

    public bool GeneratePageNumbers
    {
        get => SettingsModel.Instance.GeneratePageNumbers;
        set
        {
            SettingsModel.Instance.GeneratePageNumbers = value;
            OnPropertyChanged(nameof(GeneratePageNumbers));
        }
    }

    public bool AddDocumentRelationshipGraph
    {
        get => SettingsModel.Instance.AddDocumentRelationshipGraph;

        set
        {
            SettingsModel.Instance.AddDocumentRelationshipGraph = value;
            OnPropertyChanged(nameof(AddDocumentRelationshipGraph));
        }
    }

    public bool PrintBaseTypesToDocument
    {
        get => SettingsModel.Instance.PrintBaseTypesToDocument;
        set
        {
            SettingsModel.Instance.PrintBaseTypesToDocument = value;
            OnPropertyChanged(nameof(PrintBaseTypesToDocument));
        }
    }

    public bool KeepGraphFilesPostPDFGeneration
    {
        get => SettingsModel.Instance.KeepGraphFilesPostPDFGeneration;
        set
        {
            SettingsModel.Instance.KeepGraphFilesPostPDFGeneration = value;
            OnPropertyChanged(nameof(KeepGraphFilesPostPDFGeneration));
        }
    }


    public SettingsViewModel(Window owner) : base(owner)
    {
        new SettingsModel();

        UC_GeneralSettings = new UCGeneralSettings();
        UC_PdfSettings = new UCPdfSettings();

        SelectedTab = SettingsTab.General;

        ObjectDeclarationStyle.PropertyChanged += DeclarationStylePropertyChanged;
        ObjectDefinitionStyle.PropertyChanged += DeclarationStylePropertyChanged;
        MemberHeadingStyle.PropertyChanged += DeclarationStylePropertyChanged;
        MemberStyle.PropertyChanged += DeclarationStylePropertyChanged;
        MemberTypeStyle.PropertyChanged += DeclarationStylePropertyChanged;
        MemberDefinitionStyle.PropertyChanged += DeclarationStylePropertyChanged;
    }

    private void DeclarationStylePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged();
    }

    private void ChangeUC()
    {
        switch (selectedTab)
        {
            case SettingsTab.General:
                CurrentUserControl = UC_GeneralSettings;
                break;
            case SettingsTab.PDF:
                CurrentUserControl = UC_PdfSettings;
                break;

        }
    }
}

public enum SettingsTab
{
    General,
    PDF,
}