using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using EasyDocs.Helpers;
using EasyDocs.Models;
using EasyDocs.Models.DocumentInfo;
using EasyDocs.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;

namespace EasyDocs.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private string fileName;
    private FilePickerOpenOptions filePickerOpenOptions;
    private FolderPickerOpenOptions directoryPickerOpenOptions;
    private FilePickerSaveOptions filePickerSaveOptions;
    private SourceFileReader sourceFileReader;
    private DocumentationWriter documentationWriter;
    private SettingsWindowView settingsView;
    private string output;
    private LanguageSelectionWindowView languageSelectionWindowView;

    public string FileName
    {
        get { return fileName; }
        set
        {
            fileName = value;
            OnPropertyChanged();
        }
    }

    public string Output
    {
        get => output;
        set
        {
            output = value;
            OnPropertyChanged(nameof(Output));
        }
    }

    public string ProjectName { get; set; }
    public string ProjectDescription { get; set; }

    public ICommand LoadFileCommand { get; set; }
    public ICommand LoadDirectoryCommand { get; set; }
    public ICommand ExportPDFDocumentationCommand { get; set; }
    public ICommand ExportHTMLDocumentationCommand { get; set; }
    public ICommand ClearDocsCommand { get; set; }
    public ICommand SettingsButtonCommand { get; set; }
    public ICommand ExitAppCommand { get; set; }

    public MainWindowViewModel(Window owner) : base(owner)
    {
        owner.Closing += OnAppShuttingDown;
        settingsView = new SettingsWindowView();
        LoadFileCommand = new RelayCommand(LoadFile);
        LoadDirectoryCommand = new RelayCommand(LoadDirectory);
        ExportPDFDocumentationCommand = new RelayCommand(ExportToPDF);
        ExportHTMLDocumentationCommand = new RelayCommand(ExportToHTML);
        ClearDocsCommand = new RelayCommand(ClearDocs);
        ExitAppCommand = new RelayCommand(Exit);
        SettingsButtonCommand = new RelayCommand(OnSettingsButtonClicked);

        sourceFileReader = new SourceFileReader();
        documentationWriter = new DocumentationWriter();

        filePickerOpenOptions = new FilePickerOpenOptions();
        directoryPickerOpenOptions = new FolderPickerOpenOptions();
        filePickerSaveOptions = new FilePickerSaveOptions();

        InitDialogs();

        ShowDefaultUI();

        languageSelectionWindowView = new LanguageSelectionWindowView();
    }

    private void OnAppShuttingDown(object? sender, WindowClosingEventArgs e)
    {
        SettingsModel.Instance.SaveSettings();
    }

    private void InitDialogs()
    {
        filePickerOpenOptions.AllowMultiple = true;
        filePickerOpenOptions.Title = "Select a file(s) to load.";

        filePickerOpenOptions.FileTypeFilter = [new FilePickerFileType(@"C#"){ Patterns = ["*.cs",]}, new FilePickerFileType("Visual Basic"){ Patterns = ["*.vb"]}, new FilePickerFileType("C++ Files"){Patterns = ["*.cpp", "*.h"]}];

        directoryPickerOpenOptions.Title = "Select a folder which contains source files to load."; ;
        directoryPickerOpenOptions.AllowMultiple = false;

        filePickerSaveOptions.Title = "Save the Documentation as a PDF at the desired Location";
        filePickerSaveOptions.SuggestedFileName = "Documentation";
        filePickerSaveOptions.DefaultExtension = "pdf";
        filePickerSaveOptions.ShowOverwritePrompt = true;
        filePickerSaveOptions.FileTypeChoices = [new FilePickerFileType("PDF") { Patterns = ["*.pdf"] }];
    }

    private void Exit()
    {
        Environment.Exit(0);
    }

    private async void ClearDocs()
    {
        if (!sourceFileReader.HasData) { return; }

        IMsBox<ButtonResult> box = MessageBoxManager.GetMessageBoxStandard("Warning!", "Are you sure you want to clear all the source data that has been loaded.", ButtonEnum.YesNo, Icon.Warning, null, WindowStartupLocation.CenterOwner);

        ButtonResult result = await box.ShowWindowDialogAsync(Owner);
        if (result == ButtonResult.Yes)
        {
            sourceFileReader.Clear();
            ShowDefaultUI();
        }
    }

    private void ShowDefaultUI()
    {
        FileName = "Loaded File(s) / Directory name will be displayed here.";
        Output = "Preview will be displayed here.";
        ProjectName = "";
        ProjectDescription = "";
    }

    private async void ExportToHTML()
    {
        bool noData = await CheckNoData();
        bool hasProjectName = await CheckProjectNameAsync();
        if (noData || !hasProjectName) { return; }

        TopLevel? topLevel = TopLevel.GetTopLevel(Owner);
        if (topLevel == null) { return; }
        IReadOnlyList<IStorageFolder> folders = await topLevel.StorageProvider.OpenFolderPickerAsync(directoryPickerOpenOptions);
        if (folders.Count < 1) { return; }

        FileName = "Please wait while the documentation is being generated....";


        DeclarationColours declarationColours = new DeclarationColours(SettingsModel.Instance.MigraDocClassDeclarationColour,
                   SettingsModel.Instance.MigraDocEnumDeclarationColour, SettingsModel.Instance.MigraDocPrimitiveDeclarationColour,
                   SettingsModel.Instance.MigraDocInterfaceDeclarationColour, SettingsModel.Instance.MigraDocStructDeclarationColour);

        DeclarationFontStyles declarationFontStyles = new DeclarationFontStyles(SettingsModel.Instance.SelectedFont, SettingsModel.Instance.ObjectDeclarationStyle,
            SettingsModel.Instance.ObjectDefinitionStyle, SettingsModel.Instance.MemberHeadingStyle, SettingsModel.Instance.MemberTypeStyle,
            SettingsModel.Instance.MemberStyle, SettingsModel.Instance.MemberDefinitionStyle);

        DocumentInformation docInfo = new DocumentInformation(folders[0], declarationColours, declarationFontStyles,
            SettingsModel.Instance.GenerateTableOfContents, SettingsModel.Instance.GeneratePageNumbers,
            SettingsModel.Instance.AddDocumentRelationshipGraph, SettingsModel.Instance.PrintBaseTypesToDocument, ProjectName, ProjectDescription);

        await documentationWriter.WriteDocumentationAsync(DocumentationType.HTML, sourceFileReader.Classes.ToArray(),
                sourceFileReader.Enums.ToArray(), sourceFileReader.Interfaces.ToArray(), sourceFileReader.Structs.ToArray(), docInfo);

        FileName = "The documentation was generated successfully.";

    }

    private async void ExportToPDF()
    {
        bool noData = await CheckNoData();
        bool hasProjectName = await CheckProjectNameAsync();
        if (noData || !hasProjectName) { return; }

        TopLevel? topLevel = TopLevel.GetTopLevel(Owner);
        if (topLevel == null) { return; }
        IStorageFile? file = await topLevel.StorageProvider.SaveFilePickerAsync(filePickerSaveOptions);
        if (file == null) { return; }
        FileName = "Please wait while the documentation is being generated.....";

        DeclarationColours declarationColours = new DeclarationColours(SettingsModel.Instance.MigraDocClassDeclarationColour,
                   SettingsModel.Instance.MigraDocEnumDeclarationColour, SettingsModel.Instance.MigraDocPrimitiveDeclarationColour,
                   SettingsModel.Instance.MigraDocInterfaceDeclarationColour, SettingsModel.Instance.MigraDocStructDeclarationColour);

        DeclarationFontStyles declarationFontStyles = new DeclarationFontStyles(SettingsModel.Instance.SelectedFont, SettingsModel.Instance.ObjectDeclarationStyle,
            SettingsModel.Instance.ObjectDefinitionStyle, SettingsModel.Instance.MemberHeadingStyle, SettingsModel.Instance.MemberTypeStyle,
            SettingsModel.Instance.MemberStyle, SettingsModel.Instance.MemberDefinitionStyle);

        DocumentInformation docInfo = new DocumentInformation(file, declarationColours, declarationFontStyles,
            SettingsModel.Instance.GenerateTableOfContents, SettingsModel.Instance.GeneratePageNumbers,
            SettingsModel.Instance.AddDocumentRelationshipGraph, SettingsModel.Instance.PrintBaseTypesToDocument, ProjectName, ProjectDescription);

        await documentationWriter.WriteDocumentationAsync(DocumentationType.PDF, sourceFileReader.Classes.ToArray(),
                sourceFileReader.Enums.ToArray(), sourceFileReader.Interfaces.ToArray(), sourceFileReader.Structs.ToArray(), docInfo);
        FileName = "The documentation was generated successfully.";
    }

    private async Task<bool> CheckProjectNameAsync()
    {
        if (string.IsNullOrEmpty(ProjectName) || string.IsNullOrWhiteSpace(ProjectName))
        {
            IMsBox<ButtonResult> box = MessageBoxManager.GetMessageBoxStandard("Error!", "Enter a project name in the 'Project Name' input field.", ButtonEnum.Ok, Icon.Error, WindowStartupLocation.CenterOwner);
            await box.ShowAsPopupAsync(Owner);
            return false;
        }
        return true;
    }

    private async Task<bool> CheckHasData()
    {
        if (sourceFileReader.HasData)
        {
            IMsBox<ButtonResult> box =
            MessageBoxManager.GetMessageBoxStandard("Warning!",
            "You already have loaded some data. By clicking 'OK', you will load data on top of the existing data. Click 'Cancel' to cancel the operation.",
            ButtonEnum.OkCancel, Icon.Warning, null, WindowStartupLocation.CenterOwner);

            ButtonResult result = await box.ShowAsPopupAsync(Owner);
            if (result == ButtonResult.Cancel)
            {
                return false;
            }
        }

        return true;
    }


    /// <returns>True if there's no data, false if otherwise</returns>
    private async Task<bool> CheckNoData()
    {
        if (!sourceFileReader.HasData)
        {
            IMsBox<ButtonResult> result = MessageBoxManager.GetMessageBoxStandard("Error!",
            "You are trying to Export but you haven't loaded any data! Try loading some data by loading a directory or specific files by clickin one of the options on the 'File' menu.",
            ButtonEnum.Ok, Icon.Error, null, WindowStartupLocation.CenterOwner);

            await result.ShowAsPopupAsync(Owner);
            return true;
        }

        return false;
    }

    private async void LoadDirectory()
    {
        bool validOp = await CheckHasData();

        if (!validOp) { return; }


        if (App.Instance == null || App.Instance.TopLevel == null) { return; }

        await languageSelectionWindowView.ShowDialog(Owner);
        
        if (!languageSelectionWindowView.ValidSelection) { return; }

        IReadOnlyList<IStorageFolder> folders = await App.Instance.TopLevel.StorageProvider.OpenFolderPickerAsync(directoryPickerOpenOptions);

        if (folders.Count < 1 || folders == null) { return; }

        await sourceFileReader.ReadSourceDirectory(folders[0], languageSelectionWindowView.SelectedProgrammingLanguage);
        FileName = folders[0].Name;
        Output = sourceFileReader.GetAllDeclarations();


        
    }

    private async void LoadFile()
    {
        bool validOp = await CheckHasData();

        if (!validOp) { return; }


        if (App.Instance == null || App.Instance.TopLevel == null) { return; }
        IReadOnlyList<IStorageFile>? files = await App.Instance.TopLevel.StorageProvider.OpenFilePickerAsync(filePickerOpenOptions);
        
        if (files == null || files.Count < 1) { return; }

        await sourceFileReader.ReadSourceFilesAsync(files.ToList(), ProgLanguage.CSharp);

        FileName = files[0].Name;
        Output = sourceFileReader.GetAllDeclarations();
    }

    private void OnSettingsButtonClicked()
    {
        settingsView.ShowDialog(Owner);
    }

}
