using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DocumentationGenerator.Helpers;
using DocumentationGenerator.Models.Declarations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocumentationGenerator.Models.LanguageParsers;

/// <summary>
/// The CSharp Declaration Parser, is a wrapper class for parsing raw C# code into the different declarations.
/// </summary>
public static class CSharpDeclarationsParser
{
    /// <summary>
    /// Reads all the declarations from a syntax tree.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to read</param>
    /// <returns>Returns ParsedSourceResults which may contain parsed data</returns>
    public static ParsedSourceResults ReadAllDeclarations(SyntaxTree syntaxTree)
    {
        ParsedSourceResults results = new ParsedSourceResults();
        SyntaxNode root = syntaxTree.GetRoot();
        IEnumerable<SyntaxNode> nodes = root.ChildNodes();

        foreach (SyntaxNode node in nodes)
        {
            switch (node)
            {
                case ClassDeclarationSyntax classDec:

                    results.Classes.Add(HandleClassDeclaration(classDec));
                    break;

                case EnumDeclarationSyntax enumDec:

                    results.Enums.Add(HandleEnumDeclaration(enumDec));

                    break;

                case InterfaceDeclarationSyntax interfaceDec:
                    results.Interfaces.Add(HandleInterfaceDeclaration(interfaceDec));
                    break;

                case StructDeclarationSyntax structDec:
                    results.Structs.Add(HandleStructDeclaration(structDec));
                    break;

                case NamespaceDeclarationSyntax namespaceDeclarationSyntax:
                    results.Add(HandleNamespaceDeclaration(namespaceDeclarationSyntax));
                    break;

            }
        }

        return results;
    }

    /// <summary>
    /// Handles namespace declarations in raw code. A source file may contatin multiple namespace declarations, this function handles all the declarations within a namespace.
    /// </summary>
    /// <param name="nameSpaceDeclaration">The namespace declaration to read.</param>
    /// <returns>Returns ParsedSourceResults which may contain parsed data from within the provided namespace declaration.</returns>
    private static ParsedSourceResults HandleNamespaceDeclaration(NamespaceDeclarationSyntax nameSpaceDeclaration)
    {
        ParsedSourceResults results = new ParsedSourceResults();
        IEnumerable<SyntaxNode> nodes = nameSpaceDeclaration.ChildNodes();

        foreach (SyntaxNode node in nodes)
        {
            switch (node)
            {
                case ClassDeclarationSyntax classDec:

                    results.Classes.Add(HandleClassDeclaration(classDec));
                    break;

                case EnumDeclarationSyntax enumDec:

                    results.Enums.Add(HandleEnumDeclaration(enumDec));

                    break;

                case InterfaceDeclarationSyntax interfaceDec:
                    results.Interfaces.Add(HandleInterfaceDeclaration(interfaceDec));
                    break;

                case StructDeclarationSyntax structDec:
                    results.Structs.Add(HandleStructDeclaration(structDec));
                    break;
            }
        }

        return results;
    }

    /// <summary>
    /// Handles a struct declaration syntax, turning it into a StructDeclaration.
    /// </summary>
    /// <param name="structDec">The Struct Declaration Syntax to parse.</param>
    /// <returns>Returns a StructDeclaration containing all the important information about the struct delcaration syntax.</returns>
    private static StructDeclaration HandleStructDeclaration(StructDeclarationSyntax structDec)
    {
        string structName = structDec.Identifier.Text;
        string structDefinition = GetXML(structDec, XmlTag.summary);

        IEnumerable<PropertyDeclarationSyntax> properties = structDec.Members.OfType<PropertyDeclarationSyntax>();
        IEnumerable<FieldDeclarationSyntax> fields = structDec.Members.OfType<FieldDeclarationSyntax>();
        IEnumerable<MethodDeclarationSyntax> methods = structDec.Members.OfType<MethodDeclarationSyntax>();

        Declaration[]? newProperties = null;
        Declaration[]? newFields = null;
        Declaration[]? newMethods = null;
        int index = 0;

        if (properties.Count() > 0)
        {
            newProperties = new Declaration[properties.Count()];
            foreach (var property in properties)
            {
                newProperties[index] = new Declaration(
                    property.Identifier.Text,
                    GetXML(property, XmlTag.summary),
                    property.Type.ToString(),
                    null,
                    IsPrimitiveType(property.Type.ToString())
                );
                index++;
            }
        }

        if (fields.Count() > 0)
        {
            index = 0;
            newFields = new Declaration[fields.Count()];
            foreach (var field in fields)
            {
                foreach (var variable in field.Declaration.Variables)
                {
                    newFields[index] = new Declaration(
                        variable.Identifier.Text,
                        GetXML(field, XmlTag.summary),
                        field.Declaration.Type.ToString(),
                        null,
                        IsPrimitiveType(field.Declaration.Type.ToString())
                    );
                    index++;
                }
            }
        }

        if (methods.Count() > 0)
        {
            newMethods = new Declaration[methods.Count()];
            index = 0;
            foreach (var method in methods)
            {
                newMethods[index] = new Declaration(
                    method.Identifier.Text,
                    GetXML(method, XmlTag.summary),
                    method.ReturnType.ToString(),
                    GetXML(method, XmlTag.returns),
                    IsPrimitiveType(method.ReturnType.ToString())
                );
                index++;
            }
        }

        return new StructDeclaration(structName, structDefinition, newProperties, newFields, newMethods);
    }

    /// <summary>
    /// Handles a interface declaration syntax, turning it into a InterfaceDeclaration.
    /// </summary>
    /// <param name="structDec">The Interface Declaration Syntax to parse.</param>
    /// <returns>Returns a InterfaceDeclaration containing all the important information about the interface delcaration syntax.</returns>
    private static InterfaceDeclaration HandleInterfaceDeclaration(InterfaceDeclarationSyntax interfaceDec)
    {
        string interfaceName = interfaceDec.Identifier.Text;
        string interfaceDefinition = GetXML(interfaceDec, XmlTag.summary);

        IEnumerable<PropertyDeclarationSyntax> properties = interfaceDec.Members.OfType<PropertyDeclarationSyntax>();
        IEnumerable<MethodDeclarationSyntax> methods = interfaceDec.Members.OfType<MethodDeclarationSyntax>();

        Declaration[]? newProperties = null;
        Declaration[]? newMethods = null;
        int index = 0;

        if (properties.Count() > 0)
        {
            newProperties = new Declaration[properties.Count()];
            foreach (PropertyDeclarationSyntax property in properties)
            {
                newProperties[index] = new Declaration(
                    property.Identifier.Text,
                    GetXML(property, XmlTag.summary),
                    property.Type.ToString(),
                    null,
                    IsPrimitiveType(property.Type.ToString())
                );
                index++;
            }
        }

        if (methods.Count() > 0)
        {
            newMethods = new Declaration[methods.Count()];
            index = 0;

            foreach (MethodDeclarationSyntax method in methods)
            {
                string[]? parameters = null;
                if (method.ParameterList.Parameters.Count > 0)
                {
                    parameters = new string[method.ParameterList.Parameters.Count];
                    for (int i = 0; i < method.ParameterList.Parameters.Count; i++)
                    {
                        parameters[i] = method.ParameterList.Parameters[i].ToString();
                    }

                }
                newMethods[index] = new Declaration(
                    method.Identifier.Text, 
                    GetXML(method, XmlTag.summary),
                    method.ReturnType.ToString(), 
                    GetXML(method, XmlTag.returns), 
                    IsPrimitiveType(method.ReturnType.ToString()));

                if (parameters != null && parameters.Length > 0)
                {
                    newMethods[index].Parameters = parameters;
                }

                index++;
            }
        }

        return new InterfaceDeclaration(interfaceName, interfaceDefinition, newProperties, newMethods);
    }

    /// <summary>
    /// Creates a new Enum Declaration object with the data from the given Enum Declaration Syntax Node.
    /// </summary>
    /// <param name="enumDec">The Syntax Node to get the data from.</param>
    /// <returns>Returns an Enum Declaration containing all the members found in the given syntax, along with all their definitions read from the Trivia.</returns>
    private static EnumDeclaration HandleEnumDeclaration(EnumDeclarationSyntax enumDec)
    {
        Declaration[] enumMembers = new Declaration[enumDec.Members.Count()];
        int index = 0;

        foreach (EnumMemberDeclarationSyntax member in enumDec.Members)
        {
            enumMembers[index] = new Declaration(member.Identifier.Text, GetXML(member, XmlTag.summary), null, null);
            index++;
        }

        return new EnumDeclaration(enumDec.Identifier.Text, GetXML(enumDec, XmlTag.summary), enumMembers);
    }

    /// <summary>
    /// Creates a new Class Declaration object with the data from the given Class Declaration Syntax Node.
    /// </summary>
    /// <param name="classDec">The Syntax Node to get the data from.</param>
    /// <returns>Returns a Class Declaration containing all the fields, properties, methods and functions found in the given syntax, along with all their definitions read from the Trivia.</returns>
    private static ClassDeclaration HandleClassDeclaration(ClassDeclarationSyntax classDec)
    {
        string className = classDec.Identifier.Text;
        string classDefinition = GetXML(classDec, XmlTag.summary);

        IEnumerable<PropertyDeclarationSyntax> properties = classDec.Members.OfType<PropertyDeclarationSyntax>();
        IEnumerable<FieldDeclarationSyntax> fields = classDec.Members.OfType<FieldDeclarationSyntax>();
        IEnumerable<MethodDeclarationSyntax> methods = classDec.Members.OfType<MethodDeclarationSyntax>();

        Declaration[]? newProperties = null;
        Declaration[]? newFields = null;
        Declaration[]? newMethods = null;
        int index = 0;

        if (properties.Count() > 0)
        {
            newProperties = new Declaration[properties.Count()];
            foreach (PropertyDeclarationSyntax property in properties)
            {
                newProperties[index] = new Declaration(
                    property.Identifier.Text,
                    GetXML(property, XmlTag.summary), 
                    property.Type.ToString(), null, 
                    IsPrimitiveType(property.Type.ToString()));

                index++;
            }
        }

        if (fields.Count() > 0)
        {
            index = 0;
            newFields = new Declaration[fields.Count()];

            foreach (FieldDeclarationSyntax field in fields)
            {
                foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                {
                    //tempOutput += $"  {field.Declaration.Type} {variable.Identifier.Text} - {GetXML(field, XmlTag.summary)}" + Environment.NewLine;
                    newFields[index] = new Declaration(
                        variable.Identifier.Text,
                        GetXML(field, XmlTag.summary), 
                        field.Declaration.Type.ToString(), 
                        null, 
                        IsPrimitiveType(field.Declaration.Type.ToString()));
                    index++;
                }
            }

        }

        if (methods.Count() > 0)
        {
            newMethods = new Declaration[methods.Count()];

            index = 0;

            foreach (MethodDeclarationSyntax method in methods)
            {
                string[]? parameters = null;
                if (method.ParameterList.Parameters.Count > 0)
                {
                    parameters = new string[method.ParameterList.Parameters.Count];
                    for (int i = 0; i < method.ParameterList.Parameters.Count; i++)
                    {
                        parameters[i] = method.ParameterList.Parameters[i].ToString();
                    }

                }
                newMethods[index] = new Declaration(
                    method.Identifier.Text, 
                    GetXML(method, XmlTag.summary),
                    method.ReturnType.ToString(), 
                    GetXML(method, XmlTag.returns), 
                    IsPrimitiveType(method.ReturnType.ToString()));

                if (parameters != null && parameters.Length > 0)
                {
                    newMethods[index].Parameters = parameters;
                }

                index++;
            }
        }

        List<string> baseTypes = new List<string>();
        if (classDec.BaseList != null && classDec.BaseList.Types.Count > 0)
        {
            foreach (BaseTypeSyntax baseType in classDec.BaseList.Types)
            {
                baseTypes.Add(baseType.ToString());
            }

        }

        return new ClassDeclaration(className, classDefinition, baseTypes.ToArray(), newMethods, newFields, newProperties);
    }

    /// <summary>
    /// Attempts to find the XML comment for the given syntax node.
    /// </summary>
    /// <param name="node">The node that may contain a XML comment trivia.</param>
    /// <param name="tag">The type of XML comment to look for.</param>
    /// <returns>Returns a string which may contain the XML Comment, or an indication that theres no valid comment.</returns>
    private static string GetXML(SyntaxNode node, XmlTag tag)
    {
        SyntaxToken token = node.GetFirstToken();
        SyntaxTriviaList triviaList = token.LeadingTrivia;

        foreach (SyntaxTrivia trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
            {
                SyntaxNode? structure = trivia.GetStructure();
                if (structure == null) { return $" NO {tag.ToString().ToUpper()} "; }

                XmlElementSyntax? summary = structure.ChildNodes()
                    .OfType<XmlElementSyntax>()
                    .FirstOrDefault(e => e.StartTag.Name.LocalName.Text == tag.ToString());

                if (summary != null)
                {
                    return CleanXML(summary.GetText().ToString(), tag);
                }
            }
        }

        return $" NO {tag.ToString().ToUpper()} ";
    }

    /// <summary>
    /// Cleans the XML Comment by removing the comment characters (such as '/') and returns the declaration itself.
    /// </summary>
    /// <param name="rawComment">The XML comment.</param>
    /// <param name="tag">The type of XML (returns, summary, etc.)</param>
    /// <returns></returns>
    private static string CleanXML(string rawComment, XmlTag tag)
    {
        var lines = rawComment.Split('\n')
                              .Select(line => line.Trim())
                              .Where(line => !string.IsNullOrWhiteSpace(line))
                              .Select(line =>
                              {
                                  if (line.StartsWith("///"))
                                      line = line.Substring(3).Trim();
                                  line = line.Replace($"<{tag.ToString()}>", "")
                                             .Replace($"</{tag.ToString()}>", "")
                                             .Trim();
                                  return line;
                              });

        return string.Join(" ", lines);
    }

    /// <summary>
    /// Determines whether the given type in a string is a primitive.
    /// </summary>
    /// <param name="type">The type in a string format.</param>
    /// <returns>Returns true if is a primitive, otherwise false.</returns>
    private static bool IsPrimitiveType(string type)
    {
        if (type.ToLower() == "int") { return true; }
        if (type.ToLower() == "bool") { return true; }
        if (type.ToLower() == "sbyte") { return true; }
        if (type.ToLower() == "int16") { return true; }
        if (type.ToLower() == "uint16") { return true; }
        if (type.ToLower() == "int32") { return true; }
        if (type.ToLower() == "uint32") { return true; }
        if (type.ToLower() == "int64") { return true; }
        if (type.ToLower() == "uint64") { return true; }
        if (type.ToLower() == "single") { return true; }
        if (type.ToLower() == "double") { return true; }
        if (type.ToLower() == "char") { return true; }
        if (type.ToLower() == "float") { return true; }
        if (type.ToLower() == "void") { return true; }

        return false;
    }
}