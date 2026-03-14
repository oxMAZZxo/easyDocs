namespace EasyDocs.Models.Declarations;

/// <summary>
/// An Enum Declaration represents an enum and stores the enums' name, and definition (if has one), along with its members.
/// </summary>
public struct EnumDeclaration
{
    /// <summary>
    /// This enums' name.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// This enums' definition.
    /// </summary>
    public string? Definition { get; }
    /// <summary>
    /// An array containing all of this enums' members.
    /// </summary>
    public Declaration[] EnumMembers { get; }

    public EnumDeclaration(string name, string? definition, Declaration[] members)
    {
        Name = name;
        Definition = definition;
        EnumMembers = members;
    }
}