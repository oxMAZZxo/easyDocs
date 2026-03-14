namespace EasyDocs.Models.Declarations;

/// <summary>
/// A Class Declaration reprensets a class that is declared within a script file (.cs for example), and can store the classes name, and definition (if has one), along with its fields, properties and methods (and functions).
/// </summary>
public struct ClassDeclaration
{
    public string Name { get; }
    public string? Definition { get; }
    public string[] BaseTypes { get; set; }
    public Declaration[]? Methods { get; set; }
    public Declaration[]? Fields { get; set; }
    public Declaration[]? Properties { get; set; }

    public ClassDeclaration(string name, string? Definition, string[] baseTypes, Declaration[]? methodDeclarations, Declaration[]? fieldDeclarations, Declaration[]? propertiesDeclarations)
    {
        Name = name;
        this.Definition = Definition;
        BaseTypes = baseTypes;
        Methods = methodDeclarations;
        Fields = fieldDeclarations;
        Properties = propertiesDeclarations;
    }
}

/// <summary>
/// A declaration refers to anything declared within a script file. This could be classes, enums, interfaces, fields, properties, methods.
/// This can be used to stored any of these declarations along with their definition, types and return definitions.
/// </summary>
public struct Declaration
{
    public string Name { get; set; }
    public string? Type { get; set; }
    public string? Definition { get; set; }
    public string? ReturnDefinition { get; set; }
    public bool? IsTypePrimitive { get; set; }
    public string[]? Parameters { get; set; }
    public ObjectType? WhatIsType { get; set; }

    public Declaration(string name, string? definition, string? type = null, string? returns = null, bool? isTypePrimitive = null, ObjectType? objDeclaration = null)
    {
        Name = name;
        Type = type;
        Definition = definition;
        ReturnDefinition = returns;
        IsTypePrimitive = isTypePrimitive;
        WhatIsType = objDeclaration;
    }

    /// <returns>If this Declaration contains parameters, all the parameters will be returned seperated by commas.</returns>
    public string GetMethodParameters()
    {
        if (Parameters == null || Parameters.Length == 0) { return ""; }
        string output = "";

        for (int i = 0; i < Parameters.Length; i++)
        {
            if (i == Parameters.Length - 1)
            {
                output += Parameters[i];
            }
            else
            {
                output += Parameters[i] + ", ";
            }
        }

        return output;
    }
}

/// <summary>
/// Indicates what type of object a declaration is. This allows for changing certain logic at runtime without checking for specific types.
/// </summary>
public enum ObjectType
{
    /// <summary>
    /// A class object.
    /// </summary>
    Class,
    /// <summary>
    /// An enum object.
    /// </summary>
    Enum,
    /// <summary>
    /// An interface object.
    /// </summary>
    Interface,
    /// <summary>
    /// A struct object.
    /// </summary>
    Struct,
    /// <summary>
    /// A primitive.
    /// </summary>
    Primitive
}