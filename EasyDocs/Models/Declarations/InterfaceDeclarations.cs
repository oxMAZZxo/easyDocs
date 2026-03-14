namespace EasyDocs.Models.Declarations;

/// <summary>
/// An Interface Declaration reprensets an interface that is declared within a script file (.cs for example), and can store the name and definition (if has one), along with its properties and methods (and functions).
/// </summary>
public struct InterfaceDeclaration
{
    public string Name { get; }
    public string? Definition { get; }
    public Declaration[]? Properties { get; set; }
    public Declaration[]? Methods { get; set; }

    public InterfaceDeclaration(string name, string? definition, Declaration[]? properties, Declaration[]? methods)
    {
        Name = name;
        Definition = definition;
        Properties = properties;
        Methods = methods;
    }
}