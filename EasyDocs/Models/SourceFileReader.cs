using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using EasyDocs.Helpers;
using EasyDocs.Models.Declarations;
using EasyDocs.Models.LanguageParsers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

namespace EasyDocs.Models;
/// <summary>
/// Represents the different programming languages that are available read from source.
/// </summary>
public enum ProgLanguage
{
    /// <summary>
    /// C#
    /// </summary>
    CSharp = 0,
    /// <summary>
    /// Visual Basic or vb.Net
    /// </summary>
    VisualBasic = 1,
    /// <summary>
    /// C++
    /// </summary>
    CPP = 2
}

public class SourceFileReader : IDisposable
{
    /// <summary>
    /// A list of all the class declarations that have been parsed from source.
    /// </summary>
    public List<ClassDeclaration> Classes { get; private set; }
    /// <summary>
    /// A list of all the enum declarations that have been parsed from source.
    /// </summary>
    public List<EnumDeclaration> Enums { get; private set; }
    /// <summary>
    /// A list of all the interface declarations that have been parsed from source.
    /// </summary>
    public List<InterfaceDeclaration> Interfaces { get; private set; }
    /// <summary>
    /// A list of all the struct declarations that have been parsed from source.
    /// </summary>
    public List<StructDeclaration> Structs { get; private set; }
    /// <summary>
    /// Indicates whether this source file reader contains any data.
    /// </summary>
    public bool HasData { get; private set; }

    public SourceFileReader()
    {
        Classes = new List<ClassDeclaration>();
        Enums = new List<EnumDeclaration>();
        Interfaces = new List<InterfaceDeclaration>();
        Structs = new List<StructDeclaration>();
    }

    ~SourceFileReader()
    {
        Dispose();
    }

    /// <summary>
    /// Attempts to asyncronously read a source directory and parse all data from the chosen programming language into each respective declaration.
    /// </summary>
    /// <param name="directory">The base directory to start reading from.</param>
    /// <param name="progLanguage">The programming language to read.</param>
    /// <returns>Returns a Task which can be awaited</returns>
    public async Task ReadSourceDirectory(IStorageFolder directory, ProgLanguage progLanguage)
    {
        string searchPattern = "*";
        switch (progLanguage)
        {
            case ProgLanguage.CSharp:
                searchPattern = "*.cs";
                break;
        }

        List<IStorageFile> files = await Utilities.EnumerateAllFilesAsync(directory, searchPattern);
        await ReadSourceFilesAsync(files, progLanguage);
    }

    /// Attempts to read multiple source files asyncronously and awaits for all source files to be read until it Parses all the Results into the object.
    /// </summary>
    /// <param name="fileNames">The source files to read.</param>
    /// <returns>Returns the async Task.</returns>
    public async Task ReadSourceFilesAsync(List<IStorageFile> fileNames, ProgLanguage progLanguage)
    {
        IEnumerable<Task<ParsedSourceResults>>? tasks = fileNames.Select(path => ReadSourceFileAsync(path, progLanguage));

        ParsedSourceResults[] results = await Task.WhenAll(tasks);

        foreach (ParsedSourceResults parsedSourceResults in results)
        {
            Classes.AddRange(parsedSourceResults.Classes);
            Enums.AddRange(parsedSourceResults.Enums);
            Interfaces.AddRange(parsedSourceResults.Interfaces);
            Structs.AddRange(parsedSourceResults.Structs);
        }

        if (Classes.Count > 0 || Enums.Count > 0 || Structs.Count > 0 || Interfaces.Count > 0)
        {
            HasData = true;
        }
    }

    /// <summary>
    /// Reads a given source file for Syntax Node to extract into Declaration along with their Definitions if any.
    /// </summary>
    /// <param name="sourceFile">The source file to read.</param>
    /// <returns>Returns the Parsed Source Results which may contain Class, Enum, etc. Declarations.</returns>
    private async Task<ParsedSourceResults> ReadSourceFileAsync(IStorageFile sourceFile, ProgLanguage progLanguage)
    {
        Stream? stream = await sourceFile.OpenReadAsync();
        StreamReader streamReader = new StreamReader(stream);
        string rawCode = await streamReader.ReadToEndAsync();
        ParsedSourceResults results = ParseSourceResults(rawCode, progLanguage);

        await Task.Run(() => results.HandleCustomTypes());

        streamReader.Close();
        streamReader.Dispose();
        stream.Close();
        await stream.DisposeAsync();

        return results;
    }

    /// <summary>
    /// Attempts to parse source code into its respective declarations (Classes, Interfaces, Structs, Enums)
    /// </summary>
    /// <param name="rawCode">The code to attempt to parse.</param>
    /// <param name="progLanguage">The programming language that is being passed.</param>
    /// <returns>Returns ParsedSourceResults.</returns>
    private ParsedSourceResults ParseSourceResults(string rawCode, ProgLanguage progLanguage)
    {
        ParsedSourceResults results;
        switch (progLanguage)
        {
            case ProgLanguage.VisualBasic: results = VbDeclarationsParser.ReadAllDeclarations(VisualBasicSyntaxTree.ParseText(rawCode)); break;
            default: results = CSharpDeclarationsParser.ReadAllDeclarations(CSharpSyntaxTree.ParseText(rawCode)); break;
        }
        return results;
    }

    /// <summary>
    /// Clears all declarations cached.
    /// </summary>
    public void Clear()
    {
        Classes.Clear();
        Enums.Clear();
        Interfaces.Clear();
        Structs.Clear();
        HasData = false;
    }

    /// <summary>
    /// Gets all the declaration read from the source files combining them into a string. The only formatting it applies is tab spacing and new lines.
    /// NOTE! Structs have need been implemented yet inside this function.
    /// </summary>
    /// <returns>Returns a string with all declarations (besides enums).</returns>
    public string GetAllDeclarations()
    {
        string output = "";

        output += GetClassDeclarations();
        output += GetEnumDeclarations();
        output += GetInterfaceDeclarations();

        return output;
    }

    /// <summary>
    /// Assembles all Class Declarations from the sources.  The only formatting it applies is tab spacing and new lines.
    /// </summary>
    /// <returns>Returns a string with all class declarations and their fields, properties and method/functions.</returns>
    public string GetClassDeclarations()
    {
        string output = "";
        if (Classes == null) { return ""; }

        foreach (ClassDeclaration classDeclaration in Classes)
        {
            output += $"Class: {classDeclaration.Name} - {classDeclaration.Definition}\n";
            if (classDeclaration.Properties != null)
            {
                output += $"    Properties:\n";
                foreach (Declaration declaration in classDeclaration.Properties)
                {
                    output += $"    {declaration.Type} {declaration.Name} - {declaration.Definition}\n";
                }

            }

            if (classDeclaration.Fields != null)
            {
                output += $"    Fields:\n";
                foreach (Declaration declaration in classDeclaration.Fields)
                {
                    output += $"    {declaration.Type} {declaration.Name} - {declaration.Definition}\n";
                }

            }

            if (classDeclaration.Methods != null)
            {
                output += $"    Methods:\n";
                foreach (Declaration declaration in classDeclaration.Methods)
                {
                    output += $"    {declaration.Type} {declaration.Name} - {declaration.Definition} - {declaration.ReturnDefinition}\n";
                }

            }
        }

        return output;
    }

    /// <summary>
    /// Assembles all interface declarations into one string.
    /// </summary>
    /// <returns>Returns a string comprised of all the interface declarations.</returns>
    public string GetInterfaceDeclarations()
    {
        string output = "";

        if (Interfaces == null) return "";

        foreach (InterfaceDeclaration interfaceDeclaration in Interfaces)
        {
            output += $"Interface: {interfaceDeclaration.Name} - {interfaceDeclaration.Definition}\n";

            if (interfaceDeclaration.Properties != null)
            {
                output += $"    Properties:\n";
                foreach (Declaration declaration in interfaceDeclaration.Properties)
                {
                    output += $"    {declaration.Type} {declaration.Name} - {declaration.Definition}\n";
                }
            }

            if (interfaceDeclaration.Methods != null)
            {
                output += $"    Methods:\n";
                foreach (Declaration declaration in interfaceDeclaration.Methods)
                {
                    output += $"    {declaration.Type} {declaration.Name} - {declaration.Definition} - {declaration.ReturnDefinition}\n";
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Assembles all Enum Declarations from the sources.  The only formatting it applies is tab spacing and new lines.
    /// </summary>
    /// <returns>Returns a string with all enum declarations and their members.</returns>
    public string GetEnumDeclarations()
    {
        string output = "";

        if (Enums == null) { return ""; }

        foreach (EnumDeclaration enumDeclaration in Enums)
        {
            output += $"Enum: {enumDeclaration.Name} - {enumDeclaration.Definition}\n   Members:\n";

            foreach (Declaration declaration in enumDeclaration.EnumMembers)
            {
                output += $"    {declaration.Name} - {declaration.Definition}\n";
            }
        }

        return output;
    }

    public void Dispose()
    {
        Clear();
    }
}

/// <summary>
/// The parsed source results holds collections of different declarations which have either been parsed from a language parser.
/// </summary>
public class ParsedSourceResults
{
    /// <summary>
    /// The file path from where these source results have been parsed from.
    /// </summary>
    public string FilePath { get; set; }
    public List<ClassDeclaration> Classes { get; set; }
    public List<EnumDeclaration> Enums { get; set; }
    public List<InterfaceDeclaration> Interfaces { get; set; }
    public List<StructDeclaration> Structs { get; set; }

    public ParsedSourceResults()
    {
        FilePath = "";
        Classes = new List<ClassDeclaration>();
        Enums = new List<EnumDeclaration>();
        Interfaces = new List<InterfaceDeclaration>();
        Structs = new List<StructDeclaration>();
    }

    /// <summary>
    /// Combines incoming source results with the current source results.
    /// </summary>
    /// <param name="incoming">The source results to combine with.</param>
    public void Add(ParsedSourceResults incoming)
    {
        if (incoming == null) { return; }

        if (incoming.Classes != null && incoming.Classes.Count > 0)
        {
            Classes.AddRange(incoming.Classes);
        }
        if (incoming.Interfaces != null && incoming.Interfaces.Count > 0)
        {
            Interfaces.AddRange(incoming.Interfaces);
        }
        if (incoming.Structs != null && incoming.Structs.Count > 0)
        {
            Structs.AddRange(incoming.Structs);
        }
        if (incoming.Enums != null && incoming.Enums.Count > 0)
        {
            Enums.AddRange(incoming.Enums);
        }
    }

    /// <summary>
    /// Attempts to find custom types which have been declared/defined within the loaded source code and find references.
    /// Used for figuring out what colours can be assigned to have declaration (for fields or method return types).
    /// </summary>
    public void HandleCustomTypes()
    {
        var enumNames = new HashSet<string>(Enums.Select(e => e.Name));
        var structNames = new HashSet<string>(Structs.Select(s => s.Name));
        var interfaceNames = new HashSet<string>(Interfaces.Select(i => i.Name));

        HandleClassCustomTypes(enumNames, structNames, interfaceNames);
        HandleStructCustomTypes(enumNames, structNames, interfaceNames);
        HandleInterfaceCustomTypes(enumNames, structNames, interfaceNames);
    }

    private void HandleClassCustomTypes(HashSet<string> enumNames, HashSet<string> structNames, HashSet<string> interfaceNames)
    {
        for (int i = 0; i < Classes.Count; i++)
        {
            if (Classes[i].Fields != null)
            {
                ClassDeclaration current = Classes[i];
                current.Fields = HandleDeclarationType(current.Fields, enumNames, structNames, interfaceNames);
                Classes[i] = current;
            }

            if (Classes[i].Methods != null)
            {
                ClassDeclaration current = Classes[i];
                current.Methods = HandleDeclarationType(current.Methods, enumNames, structNames, interfaceNames);
                Classes[i] = current;
            }

            if (Classes[i].Properties != null)
            {
                ClassDeclaration current = Classes[i];
                current.Properties = HandleDeclarationType(current.Properties, enumNames, structNames, interfaceNames);
                Classes[i] = current;
            }

        }
    }

    private Declaration[]? HandleDeclarationType(Declaration[]? declarations, HashSet<string> enumNames, HashSet<string> structNames, HashSet<string> interfaceNames)
    {
        if (declarations == null) { return null; }
        for (int i = 0; i < declarations.Length; i++)
        {
            Declaration field = declarations[i];
            if (field.Type != null &&
                field.IsTypePrimitive.HasValue &&
                field.IsTypePrimitive.Value == false)
            {
                field.WhatIsType = ObjectType.Class; //default

                if (enumNames.Contains(field.Type))
                {
                    field.WhatIsType = ObjectType.Enum;
                }
                else if (structNames.Contains(field.Type))
                {
                    field.WhatIsType = ObjectType.Struct;
                }
                else if (interfaceNames.Contains(field.Type))
                {
                    field.WhatIsType = ObjectType.Interface;
                }

                declarations[i] = field;
            }
            else if (field.Type != null && field.IsTypePrimitive.HasValue && field.IsTypePrimitive.Value == true)
            {
                field.WhatIsType = ObjectType.Primitive;
            }
        }
        return declarations;
    }

    private void HandleInterfaceCustomTypes(HashSet<string> enumNames, HashSet<string> structNames, HashSet<string> interfaceNames)
    {
        for (int i = 0; i < Interfaces.Count; i++)
        {
            if (Interfaces[i].Properties != null)
            {
                InterfaceDeclaration current = Interfaces[i];

                current.Properties = HandleDeclarationType(Interfaces[i].Properties, enumNames, structNames, interfaceNames);

                Interfaces[i] = current;
            }

            if (Interfaces[i].Methods != null)
            {
                InterfaceDeclaration current = Interfaces[i];

                current.Methods = HandleDeclarationType(Interfaces[i].Methods, enumNames, structNames, interfaceNames);

                Interfaces[i] = current;
            }
        }
    }

    private void HandleStructCustomTypes(HashSet<string> enumNames, HashSet<string> structNames, HashSet<string> interfaceNames)
    {
        for (int i = 0; i < Structs.Count; i++)
        {
            if (Structs[i].Fields != null)
            {
                StructDeclaration current = Structs[i];
                current.Fields = HandleDeclarationType(current.Fields, enumNames, structNames, interfaceNames);
                Structs[i] = current;
            }

            if (Structs[i].Methods != null)
            {
                StructDeclaration current = Structs[i];
                current.Methods = HandleDeclarationType(current.Methods, enumNames, structNames, interfaceNames);
                Structs[i] = current;
            }

            if (Structs[i].Properties != null)
            {
                StructDeclaration current = Structs[i];
                current.Properties = HandleDeclarationType(current.Properties, enumNames, structNames, interfaceNames);
                Structs[i] = current;
            }
        }
    }
}