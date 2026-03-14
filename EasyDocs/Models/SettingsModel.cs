using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Avalonia.Media;
using EasyDocs.Models.DocumentInfo;


namespace EasyDocs.Models;

/// <summary>
/// The Settings Model is a Singleton that stores the user's settings data regarding to how Documentation should be generated. The data is attempted to be loaded at the start of the applications lifetime; 
/// if unsuccessful, default values are assigned to all the settings. The Settings Model provides functionality for saving the settings data, and can be called from anywhere in the application.
/// </summary>
public class SettingsModel
{
    /// <summary>
    /// The Settings Model Instance
    /// </summary>
    public static SettingsModel Instance { get; private set; }
    /// <summary>
    /// The Class Declaration Colour which will be used for rendering PDFs with MigraDoc
    /// </summary>
    public MigraDoc.DocumentObjectModel.Color MigraDocClassDeclarationColour { get; private set; }
    /// <summary>
    /// The Primitive Declaration Colour which will be used for rendering PDFs with MigraDoc
    /// </summary>
    public MigraDoc.DocumentObjectModel.Color MigraDocPrimitiveDeclarationColour { get; private set; }
    /// <summary>
    /// The Enum Declaration Colour which will be used for rendering PDFs with MigraDoc
    /// 
    /// </summary>
    public MigraDoc.DocumentObjectModel.Color MigraDocEnumDeclarationColour { get; private set; }
    /// <summary>
    /// The Interface Declaration Colour which will be used for rendering PDFs with MigraDoc
    /// 
    /// </summary>
    public MigraDoc.DocumentObjectModel.Color MigraDocInterfaceDeclarationColour { get; private set; }
    /// <summary>
    /// The Struct Declaration Colour which will be used for rendering PDFs with MigraDoc
    /// 
    /// </summary>
    public MigraDoc.DocumentObjectModel.Color MigraDocStructDeclarationColour { get; private set; }

    /// <summary>
    /// The class declaration colour used saved in the settings.
    /// </summary>
    public Color ClassDeclarationColour { get; set; }
    /// <summary>
    /// The primitive declaration colour used saved in the settings.
    /// 
    /// </summary>
    public Color PrimitiveDeclarationColour { get; set; }
    /// <summary>
    /// The enum declaration colour used saved in the settings.
    /// 
    /// </summary>
    public Color EnumDeclarationColour { get; set; }
    /// <summary>
    /// The interface declaration colour used saved in the settings.
    /// 
    /// </summary>
    public Color InterfaceDeclarationColour { get; set; }
    /// <summary>
    /// The struct declaration colour used saved in the settings.
    /// 
    /// </summary>
    public Color StructDeclarationColour { get; set; }

    /// <summary>
    /// The font style for an objects declaration.
    /// </summary>
    public FontDeclarationStyle ObjectDeclarationStyle { get; set; }
    /// <summary>
    /// the font style for an objects definition.
    /// </summary>
    public FontDeclarationStyle ObjectDefinitionStyle { get; set; }
    /// <summary>
    /// The font style for member headings
    /// </summary>
    public FontDeclarationStyle MemberHeadingStyle { get; set; }
    /// <summary>
    /// THe font style for a members name.
    /// </summary>
    public FontDeclarationStyle MemberStyle { get; set; }
    /// <summary>
    /// The font style for a memebers type
    /// </summary>
    public FontDeclarationStyle MemberTypeStyle { get; set; }
    /// <summary>
    /// The font style for a members definition.
    /// </summary>
    public FontDeclarationStyle MemberDefinitionStyle { get; set; }
    /// <summary>
    /// The currently selected Font. Used for rendering PDFs
    /// </summary>
    public string SelectedFont { get; set; }
    /// <summary>
    /// If true, the PDF Renderer will add a table of contents to the document.
    /// </summary>
    public bool GenerateTableOfContents { get; set; }
    /// <summary>
    /// If true, the PDF Renderer will add page numbers to the document.
    /// 
    /// </summary>
    public bool GeneratePageNumbers { get; set; }
    /// <summary>
    /// If true, the documentation will contain inherititance hierarchy per Class Declaration, as well as a project wide Inheritance Hierarchy.
    /// </summary>
    public bool AddDocumentRelationshipGraph { get; set; }
    /// <summary>
    /// If true, the PDF renderer will print the base types of a class in text. (Doesn't override the functionality of the AddDocumentRelationshipGraph property.)
    /// </summary>
    public bool PrintBaseTypesToDocument { get; set; }
    /// <summary>
    /// If true, after the PDF has been renderer and saved, all the graph images will be deleted.
    /// </summary>
    public bool KeepGraphFilesPostPDFGeneration { get; set; }
    /// <summary>
    /// The available fonts for the PDF generation.
    /// </summary>
    public ObservableCollection<string> Fonts { get; } = new ObservableCollection<string> { "Arial", "Arial Black", "Courier New" };

    /// <summary>
    /// Instantiates a Settings Model Singleton. NOTE! If a singleton has already been instantiated, this constructor will stop the process early. 
    /// For best practice, check whether the Instance is null before performing an operation.
    /// </summary>
    public SettingsModel()
    {
        if (Instance != null)
        {
            return;
        }

        if (Instance == null && Instance != this)
        {
            Instance = this;
        }

        if (TryLoadSettings() == false)
        {
            ClassDeclarationColour = Color.FromRgb(0, 200, 100);
            PrimitiveDeclarationColour = Color.FromRgb(0, 0, 255);
            EnumDeclarationColour = Color.FromRgb(107, 142, 35);
            InterfaceDeclarationColour = Color.FromRgb(0, 128, 128);
            StructDeclarationColour = Color.FromRgb(184, 134, 11);

            SelectedFont = Fonts[0];
            GeneratePageNumbers = true;
            GenerateTableOfContents = true;
            AddDocumentRelationshipGraph = true;
            PrintBaseTypesToDocument = true;
            KeepGraphFilesPostPDFGeneration = false;

            ObjectDeclarationStyle = new FontDeclarationStyle("20", false, true, "5");
            ObjectDefinitionStyle = new FontDeclarationStyle("18", true, false, "20");
            MemberHeadingStyle = new FontDeclarationStyle("16", false, true, "8");
            MemberTypeStyle = new FontDeclarationStyle("14", false, true, "5");
            MemberStyle = new FontDeclarationStyle("14", false, false, "1");
            MemberDefinitionStyle = new FontDeclarationStyle("14", true, false, "1");
        }

        SetMigraDocClassDeclarationColour(ClassDeclarationColour.R, ClassDeclarationColour.G, ClassDeclarationColour.B);
        SetMigraDocInterfaceDeclarationColour(InterfaceDeclarationColour.R, InterfaceDeclarationColour.G, InterfaceDeclarationColour.B);
        SetMigraDocStructDeclarationColour(StructDeclarationColour.R, StructDeclarationColour.G, StructDeclarationColour.B);
        SetMigraDocEnumDeclarationColour(EnumDeclarationColour.R, EnumDeclarationColour.G,EnumDeclarationColour.B);
        SetMigraDocPrimitiveDeclarationColour(PrimitiveDeclarationColour.R, PrimitiveDeclarationColour.G, PrimitiveDeclarationColour.B);

    }

    /// <summary>
    /// Attempts to load settings data stored in a file in the application base directory.
    /// </summary>
    /// <returns>Returns true if successful, otherwise returns false.</returns>
    private bool TryLoadSettings()
    {
        string[] lines;
        try
        {
            lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "settings.txt"));
        }
        catch (DirectoryNotFoundException)
        {
            File.Create(Path.Combine(AppContext.BaseDirectory, "settings.txt")).Close();
            return false;
        }
        catch (FileNotFoundException)
        {
            File.Create(Path.Combine(AppContext.BaseDirectory, "settings.txt")).Close();
            return false;
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"The program was not able load the saved settings preferences due to an IOException.\n Exception Message:{ex.Message}");
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            Debug.WriteLine($"The program could not load the saved settings file because of unauthorised access.");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"The program could not load the saved settings file due to a random error.\n Exception Message:{ex.Message}");
            return false;
        }

        try
        {
            string[] argb = lines[1].Split(",");
            ClassDeclarationColour = Color.FromArgb(Convert.ToByte(argb[0]), Convert.ToByte(argb[1]), Convert.ToByte(argb[2]), Convert.ToByte(argb[3]));

            argb = lines[2].Split(",");
            InterfaceDeclarationColour = Color.FromArgb(Convert.ToByte(argb[0]), Convert.ToByte(argb[1]), Convert.ToByte(argb[2]), Convert.ToByte(argb[3]));

            argb = lines[3].Split(",");
            StructDeclarationColour = Color.FromArgb(Convert.ToByte(argb[0]), Convert.ToByte(argb[1]), Convert.ToByte(argb[2]), Convert.ToByte(argb[3]));

            argb = lines[4].Split(",");
            EnumDeclarationColour = Color.FromArgb(Convert.ToByte(argb[0]), Convert.ToByte(argb[1]), Convert.ToByte(argb[2]), Convert.ToByte(argb[3]));

            argb = lines[5].Split(",");
            PrimitiveDeclarationColour = Color.FromArgb(Convert.ToByte(argb[0]), Convert.ToByte(argb[1]), Convert.ToByte(argb[2]), Convert.ToByte(argb[3]));

            string[] values = lines[7].Split(",");
            ObjectDeclarationStyle = new FontDeclarationStyle(values[0], Convert.ToBoolean(values[1]), Convert.ToBoolean(values[2]), values[3]);

            values = lines[8].Split(",");
            ObjectDefinitionStyle = new FontDeclarationStyle(values[0], Convert.ToBoolean(values[1]), Convert.ToBoolean(values[2]), values[3]);

            values = lines[9].Split(",");
            MemberHeadingStyle = new FontDeclarationStyle(values[0], Convert.ToBoolean(values[1]), Convert.ToBoolean(values[2]), values[3]);

            values = lines[10].Split(",");
            MemberTypeStyle = new FontDeclarationStyle(values[0], Convert.ToBoolean(values[1]), Convert.ToBoolean(values[2]), values[3]);

            values = lines[11].Split(",");
            MemberStyle = new FontDeclarationStyle(values[0], Convert.ToBoolean(values[1]), Convert.ToBoolean(values[2]), values[3]);

            values = lines[12].Split(",");
            MemberDefinitionStyle = new FontDeclarationStyle(values[0], Convert.ToBoolean(values[1]), Convert.ToBoolean(values[2]), values[3]);

            SelectedFont = lines[13];

            GeneratePageNumbers = Convert.ToBoolean(lines[14]);

            GenerateTableOfContents = Convert.ToBoolean(lines[15]);

            AddDocumentRelationshipGraph = Convert.ToBoolean(lines[16]);

            PrintBaseTypesToDocument = Convert.ToBoolean(lines[17]);

            KeepGraphFilesPostPDFGeneration = Convert.ToBoolean(lines[18]);

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Settings file was fully loaded but could not extract data.\nException Message:{ex.Message}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Saves the settings data to a file in the applications base directory.
    /// </summary>
    public void SaveSettings()
    {
        StreamWriter writer = new StreamWriter(Path.Combine(AppContext.BaseDirectory, "settings.txt"), false);

        writer.WriteLine("Colours (C,I,S,E,P):");
        writer.WriteLine($"{ClassDeclarationColour.A},{ClassDeclarationColour.R},{ClassDeclarationColour.G},{ClassDeclarationColour.B}");
        writer.WriteLine($"{InterfaceDeclarationColour.A},{InterfaceDeclarationColour.R},{InterfaceDeclarationColour.G},{InterfaceDeclarationColour.B}");
        writer.WriteLine($"{StructDeclarationColour.A},{StructDeclarationColour.R},{StructDeclarationColour.G},{StructDeclarationColour.B}");
        writer.WriteLine($"{EnumDeclarationColour.A},{EnumDeclarationColour.R},{EnumDeclarationColour.G},{EnumDeclarationColour.B}");
        writer.WriteLine($"{PrimitiveDeclarationColour.A},{PrimitiveDeclarationColour.R},{PrimitiveDeclarationColour.G},{PrimitiveDeclarationColour.B}");

        writer.WriteLine("FontDeclarationStyle (ObjDec,ObjDef,MemberHead,MemberType,MemberStyle,MemberDef):");
        writer.WriteLine(ObjectDeclarationStyle.GetValuesInStrings());
        writer.WriteLine(ObjectDefinitionStyle.GetValuesInStrings());
        writer.WriteLine(MemberHeadingStyle.GetValuesInStrings());
        writer.WriteLine(MemberTypeStyle.GetValuesInStrings());
        writer.WriteLine(MemberStyle.GetValuesInStrings());
        writer.WriteLine(MemberDefinitionStyle.GetValuesInStrings());

        writer.WriteLine(SelectedFont);
        writer.WriteLine(GeneratePageNumbers.ToString());
        writer.WriteLine(GenerateTableOfContents.ToString());
        writer.WriteLine(AddDocumentRelationshipGraph.ToString());
        writer.WriteLine(PrintBaseTypesToDocument.ToString());
        writer.WriteLine(KeepGraphFilesPostPDFGeneration.ToString());

        writer.Close();
        writer.Dispose();
    }

    public void SetMigraDocClassDeclarationColour(byte r, byte g, byte b)
    {
        MigraDocClassDeclarationColour = new MigraDoc.DocumentObjectModel.Color(255, r, g, b);
    }

    public void SetMigraDocPrimitiveDeclarationColour(byte r, byte g, byte b)
    {
        MigraDocPrimitiveDeclarationColour = new MigraDoc.DocumentObjectModel.Color(255, r, g, b);
    }

    public void SetMigraDocInterfaceDeclarationColour(byte r, byte g, byte b)
    {
        MigraDocInterfaceDeclarationColour = new MigraDoc.DocumentObjectModel.Color(255, r, g, b);
    }

    public void SetMigraDocEnumDeclarationColour(byte r, byte g, byte b)
    {
        MigraDocEnumDeclarationColour = new MigraDoc.DocumentObjectModel.Color(255, r, g, b);
    }

    public void SetMigraDocStructDeclarationColour(byte r, byte g, byte b)
    {
        MigraDocStructDeclarationColour = new MigraDoc.DocumentObjectModel.Color(255, r, g, b);
    }
}