using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using EasyDocs.Models.Declarations;
using EasyDocs.Models.DocumentInfo;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace EasyDocs.Models.DocumentWriters;

/// <summary>
/// The PdfWriter is a wrapper class for generating PDF documentation based on data that has been loaded from source.
/// It holds functionality for creating a pdf document, adding all the declarations on it, styling it, rendering it, and finally saving it to the target location.
/// </summary>
public class PdfWriter
{
    private const string ObjectStyle = "Object";
    private const string ObjectDefinitionStyle = "ObjectDefinition";
    private const string MemberHeading = "MemberHeading";
    private const string MemberStyle = "Member";
    private const string MemberTypeStyle = "MemberType";
    private const string MemberDefinitionStyle = "MemberDefinition";

    /// <summary>
    /// Creates a PDF Document, attempts to write all the declarations provided and saves.
    /// </summary>
    /// <param name="path">The path where you want to save the file.</param>
    /// <param name="classes">The Declaration of classes read from the SourceFileReader.</param>
    /// <param name="enums">The Declaration of enums read from the SourceFileReader.</param>
    /// <returns></returns>
    public async Task<bool> WriteAsync(ClassDeclaration[]? classes, EnumDeclaration[]? enums, InterfaceDeclaration[]? interfaces, StructDeclaration[]? structs, DocumentInformation docInfo)
    {
        if (string.IsNullOrEmpty(docInfo.SavePath.Path.ToString()) || string.IsNullOrWhiteSpace(docInfo.SavePath.Path.ToString())) { return false; }
        Document document = new Document();

        Styles styles = InitialiseDocumentStylesForPDF(document.Styles, docInfo);

        AddFirstPage(document, docInfo.ProjectName);

        // Add TOC before main content
        if (docInfo.GenerateTableOfContents)
        {
            List<string> tocEntries = new List<string>();

            if (classes != null) { tocEntries.AddRange(classes.Select(c => c.Name)); }
            if (structs != null) { tocEntries.AddRange(structs.Select(s => s.Name)); }
            if (interfaces != null) { tocEntries.AddRange(interfaces.Select(i => i.Name)); }
            if (enums != null) { tocEntries.AddRange(enums.Select(e => e.Name)); }

            AddTableOfContentsToPDF(document, tocEntries);
        }

        IStorageFolder? storageFolder = await docInfo.SavePath.GetParentAsync();
        if (storageFolder == null) { return false; }
        IStorageFolder? graphFolder = await storageFolder.CreateFolderAsync("Object_Graphs");

        docInfo.GraphFolder = graphFolder;

        await Task.WhenAll(
            AddIntroPage(document, docInfo, graphFolder),

            WriteClassesToPdfAsync(classes, docInfo.DeclarationColours, document, docInfo, graphFolder)
        );

        if (interfaces != null && interfaces.Length > 0)
        {
            WriteInterfacesToPDF(interfaces, docInfo.DeclarationColours, document);
        }

        if (structs != null && structs.Length > 0)
        {
            WriteStructsToPDF(structs, docInfo.DeclarationColours, document);
        }

        if (enums != null && enums.Length > 0)
        {
            WriteEnumsToPDF(enums, docInfo.DeclarationColours.EnumDeclarationColour, document);
        }

        if (docInfo.GeneratePageNumbers) { AddPageNumbersToPDF(document); }

        PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer
        {
            // Associate the MigraDoc document with a renderer.
            Document = document,
            // Let the PDF viewer show this PDF with full pages.
            PdfDocument =
                {
                    PageLayout = PdfPageLayout.SinglePage,
                    ViewerPreferences =
                    {
                        FitWindow = true
                    }
                }
        };

        // Layout and render document to PDF.
        pdfRenderer.RenderDocument();

        IStorageFile storageFile = (IStorageFile)docInfo.SavePath;

        Stream stream = await storageFile.OpenWriteAsync();
        pdfRenderer.Save(stream, true);
        stream.Close(); await stream.DisposeAsync();

        return true;
    }

    private async Task AddIntroPage(Document document, DocumentInformation docInfo, IStorageFolder? graphFolder)
    {
        Section section = document.AddSection();
        Paragraph paragraph = section.AddParagraph($"{docInfo.ProjectName}");
        paragraph.Format.Font.Color = Color.FromRgb(0, 0, 0);

        paragraph.Style = ObjectStyle;

        paragraph = section.AddParagraph(docInfo.ProjectDescription);
        paragraph.Style = ObjectDefinitionStyle;


        paragraph = section.AddParagraph("Global Inheritance Graph");
        paragraph.Style = ObjectStyle;

        if (docInfo.GlobalInheritanceGraph != null && graphFolder != null)
        {
            await AddGlobalInheritanceGraph(graphFolder, section, docInfo.GlobalInheritanceGraph);
        }
    }

    private async Task AddGlobalInheritanceGraph(IStorageFolder graphFolder, Section section, Bitmap globalInheritanceGraph)
    {
        IStorageFile? file = await graphFolder.CreateFileAsync("globalInheritanceGraph.png");
        if (file == null) { return; }
        Stream stream = await file.OpenWriteAsync();

        globalInheritanceGraph.Save(stream);
        Image image = section.AddImage(file.Path.AbsolutePath);
        image.Width = 500;
        image.LockAspectRatio = true;
        image.Left = 0;
        stream.Close();
        stream.Dispose();
        file.Dispose();
    }

    private void AddFirstPage(Document document, string projectName)
    {
        Section section = document.AddSection();
        Paragraph paragraph = section.AddParagraph($"{projectName} Documentation");
        paragraph.Format.Font.Size = 30;
        paragraph.Format.Font.Bold = true;
    }

    private void AddTableOfContentsToPDF(Document document, List<string> entryNames)
    {
        Section tocSection = document.AddSection();

        // Title styling
        Paragraph tocHeading = tocSection.AddParagraph("Table of Contents");
        tocHeading.Format.Font.Size = 28;
        tocHeading.Format.Font.Bold = true;
        tocHeading.Format.SpaceAfter = "1.5cm";
        tocHeading.Format.Alignment = ParagraphAlignment.Center;

        foreach (var name in entryNames)
        {
            Paragraph tocEntry = tocSection.AddParagraph();

            // Left align entry name
            var link = tocEntry.AddHyperlink(name, HyperlinkType.Bookmark);
            link.AddText(name);

            // Add leader dots before page number
            tocEntry.AddTab();
            tocEntry.AddText(new string('.', 10)); // Long enough to stretch across the page
            tocEntry.AddTab();

            // Right align page number
            tocEntry.AddPageRefField(name);

            // Formatting for the TOC entries
            tocEntry.Format.Font.Size = 14;
            tocEntry.Format.SpaceBefore = 3;
            tocEntry.Format.SpaceAfter = 3;
            tocEntry.Format.Font.Color = Color.FromRgb(31, 78, 121);
            tocEntry.Format.Font.Bold = true;
            tocEntry.Format.TabStops.Clear();
            tocEntry.Format.TabStops.AddTabStop(Unit.FromCentimeter(12), TabAlignment.Right);
        }

        // Add extra space after TOC before starting main content
        tocSection.AddParagraph().Format.SpaceAfter = "1cm";
    }

    private void AddPageNumbersToPDF(Document document)
    {
        foreach (Section? section in document.Sections)
        {
            if (section == null) { continue; }
            // Add footer
            Paragraph footer = section.Footers.Primary.AddParagraph();
            footer.AddPageField();

            footer.Format.Alignment = ParagraphAlignment.Right;
            footer.Format.Font.Size = 8;
        }
    }

    private void WriteStructsToPDF(StructDeclaration[] structs, DeclarationColours declarationColours, Document document)
    {
        Section section;
        foreach (StructDeclaration current in structs)
        {
            section = document.AddSection();

            Paragraph paragraph = section.AddParagraph("Struct ");
            paragraph.AddBookmark(current.Name);
            paragraph.Style = ObjectStyle;

            FormattedText formattedText = paragraph.AddFormattedText(current.Name);
            formattedText.Font.Color = declarationColours.StructDeclarationColour;


            paragraph = section.AddParagraph($"Definition: {current.Definition}");
            paragraph.Style = ObjectDefinitionStyle;

            if (current.Properties != null && current.Properties.Length > 0)
            {
                paragraph = section.AddParagraph(" Properties: ");
                paragraph.Style = MemberHeading;
                WriteVariablesToPDF(current.Properties, declarationColours, section);
            }

            if (current.Fields != null && current.Fields.Length > 0)
            {
                paragraph = section.AddParagraph($" Fields: ");
                paragraph.Style = MemberHeading;
                WriteVariablesToPDF(current.Fields, declarationColours, section);
            }

            if (current.Methods != null && current.Methods.Length > 0)
            {
                paragraph = section.AddParagraph(" Methods & Functions: ");
                paragraph.Style = MemberHeading;
                WriteVariablesToPDF(current.Methods, declarationColours, section);
            }
        }
    }

    private void WriteInterfacesToPDF(InterfaceDeclaration[] interfaces, DeclarationColours declarationColours, Document document)
    {
        Section section;

        foreach (InterfaceDeclaration current in interfaces)
        {
            section = document.AddSection();

            Paragraph paragraph = section.AddParagraph("Interface ");
            paragraph.AddBookmark(current.Name);
            paragraph.Style = ObjectStyle;

            FormattedText formattedText = paragraph.AddFormattedText(current.Name);
            formattedText.Font.Color = declarationColours.InterfaceDeclarationColour;

            paragraph = section.AddParagraph($"Definition: {current.Definition}");
            paragraph.Style = ObjectDefinitionStyle;

            if (current.Properties != null && current.Properties.Length > 0)
            {
                paragraph = section.AddParagraph(" Properties: ");
                paragraph.Style = MemberHeading;
                WriteVariablesToPDF(current.Properties, declarationColours, section);
            }

            if (current.Methods != null && current.Methods.Length > 0)
            {
                paragraph = section.AddParagraph(" Methods & Functions: ");
                paragraph.Style = MemberHeading;
                WriteVariablesToPDF(current.Methods, declarationColours, section);
            }
        }
    }

    private Styles InitialiseDocumentStylesForPDF(Styles styles, DocumentInformation styling)
    {
        Style style = styles.AddStyle(ObjectStyle, StyleNames.Normal);
        style.Font.Size = styling.DeclarationFonts.ObjectDeclarationStyle.FontSize;
        style.Font.Bold = styling.DeclarationFonts.ObjectDeclarationStyle.IsBold;
        style.Font.Italic = styling.DeclarationFonts.ObjectDeclarationStyle.IsItalic;
        style.Font.Name = styling.DeclarationFonts.FontFamilyName;
        style.ParagraphFormat.SpaceAfter = styling.DeclarationFonts.ObjectDeclarationStyle.SpaceAfter;

        style = styles.AddStyle(ObjectDefinitionStyle, StyleNames.Normal);
        style.Font.Size = styling.DeclarationFonts.ObjectDefinitionStyle.FontSize;
        style.Font.Bold = styling.DeclarationFonts.ObjectDefinitionStyle.IsBold;
        style.Font.Italic = styling.DeclarationFonts.ObjectDefinitionStyle.IsItalic;
        style.Font.Name = styling.DeclarationFonts.FontFamilyName;
        style.ParagraphFormat.SpaceAfter = styling.DeclarationFonts.ObjectDefinitionStyle.SpaceAfter;

        style = styles.AddStyle(MemberHeading, StyleNames.Normal);
        style.Font.Size = styling.DeclarationFonts.MemberHeadingStyle.FontSize;
        style.Font.Bold = styling.DeclarationFonts.MemberHeadingStyle.IsBold;
        style.Font.Italic = styling.DeclarationFonts.MemberHeadingStyle.IsItalic;
        style.Font.Name = styling.DeclarationFonts.FontFamilyName;
        style.ParagraphFormat.SpaceAfter = styling.DeclarationFonts.MemberHeadingStyle.SpaceAfter;

        style = styles.AddStyle(MemberStyle, StyleNames.Normal);
        style.Font.Size = styling.DeclarationFonts.MemberStyle.FontSize;
        style.Font.Bold = styling.DeclarationFonts.MemberStyle.IsBold;
        style.Font.Italic = styling.DeclarationFonts.MemberStyle.IsItalic;
        style.Font.Name = styling.DeclarationFonts.FontFamilyName;
        style.ParagraphFormat.SpaceAfter = styling.DeclarationFonts.MemberStyle.SpaceAfter;

        style = styles.AddStyle(MemberTypeStyle, StyleNames.Normal);
        style.Font.Size = styling.DeclarationFonts.MemberTypeStyle.FontSize;
        style.Font.Bold = styling.DeclarationFonts.MemberTypeStyle.IsBold;
        style.Font.Italic = styling.DeclarationFonts.MemberTypeStyle.IsItalic;
        style.Font.Name = styling.DeclarationFonts.FontFamilyName;
        style.ParagraphFormat.SpaceAfter = styling.DeclarationFonts.MemberTypeStyle.SpaceAfter;

        style = styles.AddStyle(MemberDefinitionStyle, StyleNames.Normal);
        style.Font.Size = styling.DeclarationFonts.MemberDefinitionStyle.FontSize;
        style.Font.Bold = styling.DeclarationFonts.MemberDefinitionStyle.IsBold;
        style.Font.Italic = styling.DeclarationFonts.MemberDefinitionStyle.IsItalic;
        style.Font.Name = styling.DeclarationFonts.FontFamilyName;
        style.ParagraphFormat.SpaceAfter = styling.DeclarationFonts.MemberDefinitionStyle.SpaceAfter;

        return styles;
    }

    private void WriteEnumsToPDF(EnumDeclaration[] enums, Color enumColour, Document document)
    {
        Section section;
        foreach (EnumDeclaration current in enums)
        {
            section = document.AddSection();
            Paragraph paragraph = section.AddParagraph($"Enum ");
            paragraph.AddBookmark(current.Name);
            paragraph.Style = ObjectStyle;

            FormattedText formattedText = paragraph.AddFormattedText(current.Name);
            formattedText.Font.Color = enumColour;

            paragraph = section.AddParagraph($"Definition: {current.Definition}");
            paragraph.Style = ObjectDefinitionStyle;
            if (current.EnumMembers.Length > 0)
            {
                WriteEnumMembersToPDF(current.EnumMembers, section);
            }
        }
    }

    private void WriteEnumMembersToPDF(Declaration[] enumMembers, Section section)
    {
        Paragraph paragraph = section.AddParagraph($" Members: ");
        paragraph.Style = MemberHeading;

        foreach (Declaration member in enumMembers)
        {
            paragraph = section.AddParagraph($" {member.Name} - ");
            paragraph.Style = MemberStyle;

            if (member.Definition == null) { continue; }

            FormattedText formatted = paragraph.AddFormattedText(member.Definition);
            formatted.Style = MemberDefinitionStyle;
        }
    }

    private async Task WriteClassesToPdfAsync(ClassDeclaration[]? classDeclarations, DeclarationColours declarationColours, Document document, DocumentInformation docInfo, IStorageFolder? graphFolder)
    {
        if (classDeclarations == null || classDeclarations.Length < 1) { return; }
        string? parentDirectory = Path.GetDirectoryName(docInfo.SavePath.Path.ToString());
        Section section;
        foreach (ClassDeclaration current in classDeclarations)
        {
            section = document.AddSection();
            Paragraph paragraph = section.AddParagraph($"Class ");
            paragraph.AddBookmark(current.Name);
            paragraph.Style = ObjectStyle;

            FormattedText formatted = paragraph.AddFormattedText(current.Name);
            formatted.Font.Color = declarationColours.ClassDeclarationColour;

            if (docInfo.PrintBaseTypes && current.BaseTypes != null && current.BaseTypes.Length > 0)
            {
                WriteClassInheritancesAndInterfacesToPDF(current, paragraph, declarationColours);
            }

            if (parentDirectory != null && docInfo.GenerateInheritanceGraphs && docInfo.IndividualObjsGraphs != null && docInfo.IndividualObjsGraphs.ContainsKey(current.Name) && graphFolder != null)
            {
                IStorageFile? storageFile = await graphFolder.CreateFileAsync($"{current.Name}_Graph.png");
                if (storageFile == null) { continue; }
                Bitmap currentGraph = docInfo.IndividualObjsGraphs[current.Name];
                Stream stream = await storageFile.OpenWriteAsync();
                currentGraph.Save(stream);
                stream.Close(); await stream.DisposeAsync();
                section.AddImage(storageFile.Path.AbsolutePath);

            }

            paragraph = section.AddParagraph($"Definition: {current.Definition}");

            paragraph.Style = ObjectDefinitionStyle;

            if (current.Properties != null && current.Properties.Length > 0)
            {
                paragraph = section.AddParagraph($" Properties: ");
                paragraph.Style = MemberHeading;
                WriteVariablesToPDF(current.Properties, declarationColours, section);
            }

            if (current.Fields != null && current.Fields.Length > 0)
            {
                paragraph = section.AddParagraph($" Fields: ");
                paragraph.Style = MemberHeading;
                WriteVariablesToPDF(current.Fields, declarationColours, section);
            }

            if (current.Methods != null && current.Methods.Length > 0)
            {
                paragraph = section.AddParagraph($" Methods & Functions: ");
                paragraph.Style = MemberHeading;
                WriteMethodsToPDF(current.Methods, declarationColours, section);
            }
        }
    }

    private void WriteClassInheritancesAndInterfacesToPDF(ClassDeclaration current, Paragraph paragraph, DeclarationColours declarationColours)
    {
        FormattedText formatted;

        if (current.BaseTypes[0][0] == 'I')
        {
            WriteInterfaceImplementationsToPDF(false, current, paragraph, declarationColours);
            return;
        }
        else
        {
            formatted = paragraph.AddFormattedText($"\nInherits ");
            formatted.Style = ObjectStyle;
            formatted = paragraph.AddFormattedText($"{current.BaseTypes[0]}");
            formatted.Color = declarationColours.ClassDeclarationColour;
        }

        if (current.BaseTypes.Length > 1)
        {
            WriteInterfaceImplementationsToPDF(true, current, paragraph, declarationColours);
        }
    }

    private void WriteInterfaceImplementationsToPDF(bool skipFirst, ClassDeclaration current, Paragraph paragraph, DeclarationColours declarationColours)
    {
        string interfaces = "";
        if (!skipFirst)
        {
            interfaces = $"{current.BaseTypes[0]}";
            interfaces += ", ";
        }
        if (current.BaseTypes.Length > 1)
        {
            for (int i = 1; i < current.BaseTypes.Length; i++)
            {
                if (i == current.BaseTypes.Length - 1)
                {
                    interfaces += $"{current.BaseTypes[i]}";
                }
                else
                {
                    interfaces += $"{current.BaseTypes[i]}, ";
                }
            }
        }

        FormattedText formatted = paragraph.AddFormattedText($"\nImplements ");
        formatted.Style = ObjectStyle;

        formatted = paragraph.AddFormattedText($"{interfaces}");
        formatted.Color = declarationColours.InterfaceDeclarationColour;
    }

    private void WriteMethodsToPDF(Declaration[] methods, DeclarationColours declarationColours, Section section)
    {
        foreach (Declaration method in methods)
        {
            Paragraph paragraph = section.AddParagraph($" {method.Type} ");
            paragraph.Style = MemberTypeStyle;

            paragraph.Format.Font.Color = declarationColours.PrimitiveDeclarationColour;

            if (method.IsTypePrimitive.HasValue && method.IsTypePrimitive.Value == false)
            {
                switch (method.WhatIsType)
                {
                    case ObjectType.Class: paragraph.Format.Font.Color = declarationColours.ClassDeclarationColour; break;
                    case ObjectType.Enum: paragraph.Format.Font.Color = declarationColours.EnumDeclarationColour; break;
                    case ObjectType.Interface: paragraph.Format.Font.Color = declarationColours.InterfaceDeclarationColour; break;
                        //case ObjectType.Struct: paragraph.Format.Font.Color = declarationColours.ClassDeclarationColour; break;
                }
            }

            FormattedText formatted = paragraph.AddFormattedText($"{method.Name}({method.GetMethodParameters()}) - ");
            formatted.Style = MemberStyle;

            formatted = paragraph.AddFormattedText($"{method.Definition} - {method.ReturnDefinition}");
            formatted.Style = MemberDefinitionStyle;
        }
    }

    private void WriteVariablesToPDF(Declaration[] fields, DeclarationColours declarationColours, Section section)
    {
        foreach (Declaration field in fields)
        {
            Paragraph paragraph = section.AddParagraph($" {field.Type} ");
            paragraph.Style = MemberTypeStyle;

            paragraph.Format.Font.Color = declarationColours.PrimitiveDeclarationColour;

            if (field.IsTypePrimitive.HasValue && field.IsTypePrimitive.Value == false)
            {
                switch (field.WhatIsType)
                {
                    case ObjectType.Class: paragraph.Format.Font.Color = declarationColours.ClassDeclarationColour; break;
                    case ObjectType.Enum: paragraph.Format.Font.Color = declarationColours.EnumDeclarationColour; break;
                    case ObjectType.Interface: paragraph.Format.Font.Color = declarationColours.InterfaceDeclarationColour; break;
                    case ObjectType.Struct: paragraph.Format.Font.Color = declarationColours.ClassDeclarationColour; break;
                }
            }


            FormattedText formatted = paragraph.AddFormattedText($"{field.Name} - ");
            formatted.Style = MemberStyle;

            if (field.Definition == null) { continue; }
            formatted = paragraph.AddFormattedText(field.Definition);
            formatted.Style = MemberDefinitionStyle;

        }
    }
}