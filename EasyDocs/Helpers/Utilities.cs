using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using SkiaSharp;

namespace EasyDocs.Helpers
{
    /// <summary>
    /// Provides a collection of utility methods for file management,
    /// color conversion, and low-level hexadecimal operations.
    /// </summary>
    /// <remarks>
    /// This class includes asynchronous methods for working with
    /// <see cref="IStorageFile"/> and <see cref="IStorageFolder"/> objects
    /// within the Avalonia storage system, as well as convenience methods
    /// for color conversion and hexadecimal manipulation.
    /// </remarks>
    public static class Utilities
    {
        /// <summary>
        /// Recursively deletes all files within the specified folder, then deletes the folder itself.
        /// </summary>
        /// <param name="folder">The target <see cref="IStorageFolder"/> to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method enumerates and deletes all nested files before removing the folder.
        /// Subdirectories are handled automatically.
        /// </remarks>
        public static async Task DeleteFolder(IStorageFolder folder)
        {
            List<IStorageFile> files = await EnumerateAllFilesAsync(folder);
            for (int i = 0; i < files.Count; i++)
            {
                await files[i].DeleteAsync();
            }

            files.Clear();
            await folder.DeleteAsync();
        }

        /// <summary>
        /// Recursively enumerates all files within a folder and its subfolders.
        /// </summary>
        /// <param name="folder">The root folder to enumerate.</param>
        /// <param name="searchPattern">
        /// An optional filename pattern (e.g. <c>"*.txt"</c>). Defaults to <c>"*"</c> (all files).
        /// </param>
        /// <returns>
        /// A list of all <see cref="IStorageFile"/> objects found in the specified folder and its subfolders.
        /// </returns>
        /// <remarks>
        /// Avalonia’s <see cref="IStorageFolder.GetItemsAsync"/> does not support pattern matching directly,
        /// so this method performs simple manual filtering based on file extensions.
        /// </remarks>
        public static async Task<List<IStorageFile>> EnumerateAllFilesAsync(IStorageFolder folder, string searchPattern = "*")
        {
            var files = new List<IStorageFile>();

            await foreach (var item in folder.GetItemsAsync())
            {
                switch (item)
                {
                    case IStorageFile file:
                        if (MatchesPattern(file.Name, searchPattern))
                            files.Add(file);
                        break;

                    case IStorageFolder subfolder:
                        var subFiles = await EnumerateAllFilesAsync(subfolder, searchPattern);
                        files.AddRange(subFiles);
                        break;
                }
            }

            return files;
        }

        /// <summary>
        /// Performs basic wildcard pattern matching against a filename.
        /// </summary>
        /// <param name="fileName">The name of the file to test.</param>
        /// <param name="pattern">A pattern string such as <c>"*.png"</c> or <c>"document.txt"</c>.</param>
        /// <returns>
        /// <see langword="true"/> if the file name matches the pattern; otherwise, <see langword="false"/>.
        /// </returns>
        private static bool MatchesPattern(string fileName, string pattern)
        {
            if (pattern == "*") return true;

            if (pattern.StartsWith("*."))
            {
                var ext = pattern.Substring(1); // e.g., ".cs"
                return fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(fileName, pattern, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Converts a <see cref="MigraDoc.DocumentObjectModel.Color"/> to a <see cref="SKColor"/>.
        /// </summary>
        /// <param name="color">The MigraDoc color value to convert.</param>
        /// <returns>An equivalent <see cref="SKColor"/> instance.</returns>
        public static SKColor MigraDocColorToSKColor(MigraDoc.DocumentObjectModel.Color color)
        {
            return new SKColor(color.Argb);
        }

        /// <summary>
        /// Converts a two-character hexadecimal string to its corresponding byte value.
        /// </summary>
        /// <param name="hex">A string representing a hexadecimal value (e.g. <c>"FF"</c>).</param>
        /// <returns>The byte value represented by the hexadecimal string.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="hex"/> is not exactly two characters long or contains invalid characters.
        /// </exception>
        public static byte HexToByte(string hex)
        {
            if (hex.Length != 2)
                throw new ArgumentException("Hex string must be 2 characters long.");

            int high = HexCharToInt(hex[0]);
            int low = HexCharToInt(hex[1]);

            return (byte)((high << 4) + low);
        }

        /// <summary>
        /// Converts a single hexadecimal character into its integer equivalent.
        /// </summary>
        /// <param name="c">The hexadecimal character to convert (0–9, A–F, or a–f).</param>
        /// <returns>The integer value of the character.</returns>
        /// <exception cref="ArgumentException">Thrown if the character is not a valid hexadecimal digit.</exception>
        public static int HexCharToInt(char c)
        {
            if (c >= '0' && c <= '9')
                return c - '0';
            if (c >= 'A' && c <= 'F')
                return c - 'A' + 10;
            if (c >= 'a' && c <= 'f')
                return c - 'a' + 10;

            throw new ArgumentException("Invalid hex character: " + c);
        }

        /// <summary>
        /// Copies a file from a given path to a target Avalonia storage folder.
        /// </summary>
        /// <param name="sourceFileName">The name to assign to the copied file in the destination folder.</param>
        /// <param name="sourceFilePath">The full path to the source file on disk.</param>
        /// <param name="outputFolder">The destination folder in which to create the new file.</param>
        /// <returns>
        /// <see langword="true"/> if the copy succeeds; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method uses Avalonia’s <see cref="IStorageProvider"/> to open and create storage files.
        /// It safely disposes of streams after copying.
        /// </remarks>
        public static async Task<bool> CopyFileAsync(string sourceFileName, string sourceFilePath, IStorageFolder outputFolder)
        {
            Stream? sourceStream = await TryOpenReadStream(sourceFilePath);
            if (sourceStream == null) return false;

            IStorageFile? copyFile = await outputFolder.CreateFileAsync(sourceFileName);
            if (copyFile == null) return false;

            Stream destinationStream = await copyFile.OpenWriteAsync();
            await sourceStream.CopyToAsync(destinationStream);

            destinationStream.Close();
            await destinationStream.DisposeAsync();

            sourceStream.Close();
            await sourceStream.DisposeAsync();

            copyFile.Dispose();

            return true;
        }

        /// <summary>
        /// Attempts to open a read-only stream for a file path using Avalonia’s storage provider.
        /// </summary>
        /// <param name="sourceFilePath">The full path to the file to open.</param>
        /// <returns>
        /// A readable <see cref="Stream"/> if the file exists and can be opened; otherwise, <see langword="null"/>.
        /// </returns>
        /// <remarks>
        /// This method safely handles missing files, invalid directories, and unexpected exceptions,
        /// logging any errors to the <see cref="Debug"/> output stream.
        /// </remarks>
        public static async Task<Stream?> TryOpenReadStream(string sourceFilePath)
        {
            try
            {
                if (App.Instance == null || App.Instance.TopLevel == null)
                    return null;

                IStorageFile? file = await App.Instance.TopLevel.StorageProvider.TryGetFileFromPathAsync(sourceFilePath);
                if (file == null)
                    throw new FileNotFoundException();

                return await file.OpenReadAsync();
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine($"File not found: {sourceFilePath}");
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                Debug.WriteLine($"Directory not found for file: {sourceFilePath}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading file '{sourceFilePath}': {ex.Message}");
                return null;
            }
        }
    }

    /// <summary>
    /// Represents standard XML documentation tag identifiers used for markup generation.
    /// </summary>
    public enum XmlTag
    {
        /// <summary>
        /// Represents the XML &lt;summary&gt; tag.
        /// </summary>
        summary,

        /// <summary>
        /// Represents the XML &lt;returns&gt; tag.
        /// </summary>
        returns
    }
}
