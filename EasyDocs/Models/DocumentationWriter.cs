using System.Threading.Tasks;
using EasyDocs.Helpers;
using EasyDocs.Models.Declarations;
using EasyDocs.Models.DocumentInfo;
using EasyDocs.Models.DocumentWriters;

namespace EasyDocs.Models;

/// <summary>
/// The Documentation Writer provides functionality for creating different types of documentation from Declarations (Classes, Interfaces etc.).
/// </summary>
public class DocumentationWriter
{
    /// <summary>
    /// An instance of a PDF Writer
    /// </summary>
    private PdfWriter pdfWriter;
    /// <summary>
    /// An instance of a HTML Writer
    /// </summary>
    private HtmlWriter htmlWriter;

    public DocumentationWriter()
    {
        pdfWriter = new PdfWriter();
        htmlWriter = new HtmlWriter();
    }

    /// <summary>
    /// Write a type of documentation to a specified destination, providing all the nessary declarations, styles etc. 
    /// </summary>
    /// <param name="type">The type of documentation to be created.</param>
    /// <param name="classDeclarations">The class declarations to use for the documentation.</param>
    /// <param name="enumDeclarations">The enum declarations to use for the documentation.</param>
    /// <param name="interfaceDeclarations">The interface declarations to use for the documentation.</param>
    /// <param name="structDeclarations">The struct declarations to use for the documentation.</param>
    /// <param name="docInfo">The documents information (regarding styles, fonts, graphs etc.).</param>
    /// <returns>Returns true if operation was successful, else returns false</returns>
    public async Task<bool> WriteDocumentationAsync(DocumentationType type, ClassDeclaration[]? classDeclarations, EnumDeclaration[]? enumDeclarations, InterfaceDeclaration[]? interfaceDeclarations, StructDeclaration[]? structDeclarations, DocumentInformation docInfo)
    {
        if (docInfo.GenerateInheritanceGraphs)
        {
            docInfo.GlobalInheritanceGraph = InheritanceGraphGenerator.GenerateGlobalGraph(classDeclarations, interfaceDeclarations, docInfo.DeclarationColours);
            docInfo.IndividualObjsGraphs = InheritanceGraphGenerator.GenerateIndividualGraphs(classDeclarations, docInfo.DeclarationColours);
        }

        bool success;
        if (type == DocumentationType.PDF)
        {
            success = await pdfWriter.WriteAsync(classDeclarations, enumDeclarations, interfaceDeclarations, structDeclarations, docInfo);
            if (success && SettingsModel.Instance.KeepGraphFilesPostPDFGeneration == false && docInfo.GraphFolder != null)
            {
                await Utilities.DeleteFolder(docInfo.GraphFolder);
            }
        }
        else
        {
            success = await htmlWriter.WriteAsync(classDeclarations, enumDeclarations, interfaceDeclarations, structDeclarations, docInfo);
        }

        if (success && App.Instance != null && App.Instance.TopLevel != null)
        {
            await App.Instance.TopLevel.Launcher.LaunchUriAsync(docInfo.SavePath.Path);
        }
        return success;
    }
}

/// <summary>
/// Represents the different types of documentation that can be created.
/// </summary>
public enum DocumentationType
{
    /// <summary>
    /// PDF Document
    /// </summary>
    PDF,
    /// <summary>
    /// HTML Document(s)
    /// </summary>
    HTML
}