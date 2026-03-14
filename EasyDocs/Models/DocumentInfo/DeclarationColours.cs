using MigraDoc.DocumentObjectModel;

namespace EasyDocs.Models.DocumentInfo;

/// <summary>
/// The Declaration Colours class stores the MigraDoc colours for the different declarations which are used in the PDF Rendering process.
/// </summary>
public struct DeclarationColours
{
    public Color ClassDeclarationColour { get; set; }
    public Color EnumDeclarationColour { get; set; }
    public Color PrimitiveDeclarationColour { get; set; }
    public Color InterfaceDeclarationColour { get; set; }
    public Color StructDeclarationColour { get; set; }

    public DeclarationColours(Color classDeclarationColour, Color enumDeclarationColour, Color primitiveDeclarationColour, Color interfaceDeclarationColour, Color structDeclarationColour)
    {
        ClassDeclarationColour = classDeclarationColour;
        EnumDeclarationColour = enumDeclarationColour;
        PrimitiveDeclarationColour = primitiveDeclarationColour;
        InterfaceDeclarationColour = interfaceDeclarationColour;
        StructDeclarationColour = structDeclarationColour;
    }
}