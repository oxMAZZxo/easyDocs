using System;
using System.IO;
using PdfSharp.Fonts;

namespace EasyDocs.Helpers
{
    /// <summary>
    /// A simple implementation of the <see cref="IFontResolver"/> interface
    /// for resolving and loading TrueType fonts used by PDFsharp.
    /// </summary>
    /// <remarks>
    /// This resolver supports basic Arial and Courier New font families,
    /// including bold and italic variations. Font files are expected to
    /// exist in a <c>fonts</c> subdirectory located under the application’s
    /// base directory.
    /// </remarks>
    public class SimpleFontResolver : IFontResolver
    {
        /// <summary>
        /// Retrieves the binary font data for a given face name identifier.
        /// </summary>
        /// <param name="faceName">The internal font face name (e.g. <c>Arial#</c> or <c>Courier-New-Bold#</c>).</param>
        /// <returns>A byte array containing the raw font data.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the specified face name is not recognized or does not map to a known font file.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the corresponding font file cannot be located on disk.
        /// </exception>
        /// <remarks>
        /// Font files are loaded from <c>{AppContext.BaseDirectory}/fonts/</c>.
        /// </remarks>
        public byte[] GetFont(string faceName)
        {
            string fontPath = "Invalid";

            switch (faceName)
            {
                case "Arial#": fontPath = "arial.ttf"; break;
                case "Arial-Black#": fontPath = "ariblk.ttf"; break;
                case "Arial-Bold#": fontPath = "arialbd.ttf"; break;
                case "Arial-Italic#": fontPath = "ariali.ttf"; break;
                case "Arial-BoldItalic#": fontPath = "arialbi.ttf"; break;

                case "Courier-New#": fontPath = "cour.ttf"; break;
                case "Courier-New-Bold#": fontPath = "courbd.ttf"; break;
                case "Courier-New-Italic#": fontPath = "couri.ttf"; break;
                case "Courier-New-BoldItalic#": fontPath = "courbi.ttf"; break;
            }

            if (fontPath == "Invalid")
                throw new InvalidOperationException("Unknown face name: " + faceName);

            string path = Path.Combine(AppContext.BaseDirectory, "fonts", fontPath);
            return File.ReadAllBytes(path);
        }

        /// <summary>
        /// Resolves a font family name and style (bold/italic) to a specific
        /// <see cref="FontResolverInfo"/> instance that uniquely identifies
        /// the corresponding face name for use by PDFsharp.
        /// </summary>
        /// <param name="familyName">The logical font family name (e.g. <c>Arial</c> or <c>Courier New</c>).</param>
        /// <param name="isBold">Specifies whether the font is bold.</param>
        /// <param name="isItalic">Specifies whether the font is italic.</param>
        /// <returns>
        /// A <see cref="FontResolverInfo"/> instance referencing the matching
        /// face name identifier for the requested family and style.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the specified font family name cannot be resolved to a supported face.
        /// </exception>
        /// <remarks>
        /// Supported families include:
        /// <list type="bullet">
        /// <item><description>Arial (regular, bold, italic, bold italic)</description></item>
        /// <item><description>Arial Black</description></item>
        /// <item><description>Courier New (regular, bold, italic, bold italic)</description></item>
        /// </list>
        /// </remarks>
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            familyName = familyName.ToLowerInvariant();

            if (familyName == "courier new")
            {
                if (isBold && isItalic) return new FontResolverInfo("Courier-New-BoldItalic#");
                if (isBold) return new FontResolverInfo("Courier-New-Bold#");
                if (isItalic) return new FontResolverInfo("Courier-New-Italic#");
                return new FontResolverInfo("Courier-New#");
            }

            if (familyName == "arial black")
            {
                return new FontResolverInfo("Arial-Black#");
            }

            if (familyName == "arial")
            {
                if (isBold && isItalic) return new FontResolverInfo("Arial-BoldItalic#");
                if (isBold) return new FontResolverInfo("Arial-Bold#");
                if (isItalic) return new FontResolverInfo("Arial-Italic#");
                return new FontResolverInfo("Arial#");
            }

            throw new InvalidOperationException("Font family not resolved: " + familyName);
        }
    }
}
