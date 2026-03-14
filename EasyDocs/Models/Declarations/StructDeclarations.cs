namespace EasyDocs.Models.Declarations;

/// <summary>
/// A Struct Declaration reprensets a struct that is declared within a script file (.cs for example), and can store the structs' name and definition (if has one), along with its fields, properties and methods (and functions).
/// </summary>
public struct StructDeclaration
{
    public string Name { get; }
    public string? Definition { get; }
    public Declaration[]? Properties { get; set; }
    public Declaration[]? Fields { get; set; }
    public Declaration[]? Methods { get; set; }

    public StructDeclaration(string name, string? defintion, Declaration[]? properties, Declaration[]? fields, Declaration[]? methods)
    {
        this.Name = name;
        this.Definition = defintion;
        this.Properties = properties;
        this.Fields = fields;
        this.Methods = methods;
    }

}