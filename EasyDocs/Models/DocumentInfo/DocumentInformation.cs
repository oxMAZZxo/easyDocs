using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace EasyDocs.Models.DocumentInfo;

/// <summary>
/// The Document Information class represents/stores data regarding to how the documentation will be rendered/what will be rendered. 
/// It is used in both the HTML Docs creation and PDF Docs creation process, however for each process different data would be needed, therefore this class is a way of storing that data in one place and accessing it from one place.
/// That said, in the future different classes might be created to be used for the different documentation creation processes. 
/// </summary>
public class DocumentInformation : IDisposable
{
    public DeclarationColours DeclarationColours { get; set; }
    public DeclarationFontStyles DeclarationFonts { get; set; }
    public bool GenerateTableOfContents { get; set; }
    public bool GeneratePageNumbers { get; set; }
    public bool GenerateInheritanceGraphs { get; set; }
    public bool PrintBaseTypes { get; set; }
    public string ProjectName { get; set; }
    public string ProjectDescription { get; set; }
    public Bitmap? GlobalInheritanceGraph { get; set; }
    public Dictionary<string, Bitmap>? IndividualObjsGraphs { get; set; }
    public IStorageItem SavePath { get; }
    public IStorageFolder? GraphFolder { get; set; }

    public
    DocumentInformation(IStorageItem savePath, DeclarationColours declarationColours, DeclarationFontStyles declarationFonts, bool generateTableOfContents, bool generatePageNumbers, bool generateRelationshipGraph, bool printBaseTypes, string projectName, string projectDescription)
    {
        SavePath = savePath;
        DeclarationColours = declarationColours;
        DeclarationFonts = declarationFonts;
        GenerateTableOfContents = generateTableOfContents;
        GeneratePageNumbers = generatePageNumbers;
        GenerateInheritanceGraphs = generateRelationshipGraph;
        PrintBaseTypes = printBaseTypes;
        ProjectName = projectName;
        ProjectDescription = projectDescription;
    }

    public void Dispose()
    {
        if (IndividualObjsGraphs != null)
        {
            IndividualObjsGraphs.Clear();
        }

        if (GlobalInheritanceGraph != null)
        {
            GlobalInheritanceGraph.Dispose();
        }
    }
}

/// <summary>
/// The Declaration Font Styles is a wrapper class which holds all the different FontDeclarationStyle object which are used in the PDF Rendering Creation.
/// </summary>
public class DeclarationFontStyles
{
    public string FontFamilyName { get; }
    public FontDeclarationStyle ObjectDeclarationStyle { get; }
    public FontDeclarationStyle ObjectDefinitionStyle { get; }
    public FontDeclarationStyle MemberHeadingStyle { get; }
    public FontDeclarationStyle MemberStyle { get; }
    public FontDeclarationStyle MemberTypeStyle { get; }
    public FontDeclarationStyle MemberDefinitionStyle { get; }

    public DeclarationFontStyles(string fontFamilyName, FontDeclarationStyle objectDeclarationStyle, FontDeclarationStyle objectDefinitionStyle, FontDeclarationStyle memberHeadingStyle, FontDeclarationStyle memberTypeStyle, FontDeclarationStyle memberStyle, FontDeclarationStyle memberDefinitionStyle)
    {
        FontFamilyName = fontFamilyName;
        ObjectDeclarationStyle = objectDeclarationStyle;
        ObjectDefinitionStyle = objectDefinitionStyle;
        MemberHeadingStyle = memberHeadingStyle;
        MemberStyle = memberStyle;
        MemberTypeStyle = memberTypeStyle;
        MemberDefinitionStyle = memberDefinitionStyle;
    }
}