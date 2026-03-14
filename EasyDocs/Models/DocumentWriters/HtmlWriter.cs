using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Utilities;
using EasyDocs.Helpers;
using EasyDocs.Models.Declarations;
using EasyDocs.Models.DocumentInfo;

namespace EasyDocs.Models.DocumentWriters;

/// <summary>
/// The HtmlWriter is a wrapper class for generating HTML Documentation based on the loaded source. 
/// It holds functionality for generating pages for each Object Declaration, generating a navigation bar, and the home page. 
/// The wrapper also copies resources to the target folder where the documentation is created, such as the CSS files for the styling and JS code which makes the documentation interactive.
/// </summary>
public class HtmlWriter
{
    public async Task<bool> WriteAsync(ClassDeclaration[]? classes, EnumDeclaration[]? enums, InterfaceDeclaration[]? interfaces, StructDeclaration[]? structs, DocumentInformation docInfo)
    {
        if (string.IsNullOrEmpty(docInfo.SavePath.Path.ToString()) || string.IsNullOrWhiteSpace(docInfo.SavePath.Path.ToString())) { return false; }
        IStorageFolder parentFolder = (IStorageFolder)docInfo.SavePath;

        IStorageFolder? outputFolder = await parentFolder.CreateFolderAsync($"{docInfo.ProjectName} Documentation");
        if (outputFolder == null) { return false; }

        bool valid = await TryCopyHtmlResources(outputFolder);

        if (!valid) { return false; }

        if (docInfo.GenerateInheritanceGraphs && docInfo.GlobalInheritanceGraph != null && docInfo.IndividualObjsGraphs != null)
        {
            await SaveBitmaps(docInfo.GlobalInheritanceGraph, docInfo.IndividualObjsGraphs, outputFolder);
        }

        string homepageSideBar = GenerateSideBar(classes, enums, interfaces, structs, docInfo);
        await GenerateHomePage(outputFolder, homepageSideBar, docInfo);

        string objSideBar = GenerateSideBarForObjs(classes, enums, interfaces, structs, docInfo);
        await GenerateObjPages(outputFolder, classes, enums, interfaces, structs, objSideBar, docInfo);
        return true;
    }

    private async Task<bool> TryCopyHtmlResources(IStorageFolder outputFolder)
    {
        if (await Utilities.CopyFileAsync("helper.js", Path.Combine(AppContext.BaseDirectory, "HTML Doc Templates/helper.js"), outputFolder) == false)
        {
            return false;
        }
        if (await Utilities.CopyFileAsync("docsHelpers.js", Path.Combine(AppContext.BaseDirectory, "HTML Doc Templates/docsHelpers.js"), outputFolder) == false)
        {
            return false;
        }
        if (await Utilities.CopyFileAsync("homePageStyles.css", Path.Combine(AppContext.BaseDirectory, "HTML Doc Templates/homePageStyles.css"), outputFolder) == false)
        {
            return false;
        }
        if (await Utilities.CopyFileAsync("sidebar.css", Path.Combine(AppContext.BaseDirectory, "HTML Doc Templates/sidebar.css"), outputFolder) == false)
        {
            return false;
        }
        if (await Utilities.CopyFileAsync("docStyles.css", Path.Combine(AppContext.BaseDirectory, "HTML Doc Templates/docStyles.css"), outputFolder) == false)
        {
            return false;
        }
        return true;
    }



    private async Task SaveBitmaps(Bitmap globalInheritanceGraph, Dictionary<string, Bitmap> individualGraphs, IStorageFolder outputFolder)
    {
        IStorageFile? file = await outputFolder.CreateFileAsync("globalInheritanceGraph.png");
        if (file != null)
        {
            Stream? stream = await file.OpenWriteAsync();
            globalInheritanceGraph.Save(stream);
            stream.Close();
            await stream.DisposeAsync();
            file.Dispose();
        }

        IStorageFolder? folder = await outputFolder.CreateFolderAsync("objGraphs");
        if (folder == null) { return; }

        foreach (string o in individualGraphs.Keys)
        {
            Bitmap bitmap = individualGraphs[o];
            IStorageFile? subFile = await folder.CreateFileAsync($"{o}_Graph.png");
            if (subFile == null) { continue; }

            Stream? stream = await subFile.OpenWriteAsync();
            bitmap.Save(stream);
            stream.Close();
            await stream.DisposeAsync();
            subFile.Dispose();
        }
    }

    private async Task GenerateObjPages(IStorageFolder parentFolder, ClassDeclaration[]? classes, EnumDeclaration[]? enums, InterfaceDeclaration[]? interfaces, StructDeclaration[]? structs, string sideBar, DocumentInformation docInfo)
    {
        IStorageFolder? folder = await parentFolder.CreateFolderAsync("objs");
        if (folder == null) { return; }
        string searchDataString = "";
        if (classes != null && classes.Length > 0)
        {
            for (int i = 0; i < classes.Length; i++)
            {
                searchDataString += @"""" + classes[i].Name + @"""" + ",";
                await TryCreateObjFile(folder, classes[i].Name, classes[i].Definition, sideBar, docInfo, classes[i].Properties, classes[i].Methods);
            }

        }

        if (interfaces != null && interfaces.Length > 0)
        {
            for (int i = 0; i < interfaces.Length; i++)
            {
                searchDataString += @"""" + interfaces[i].Name + @"""" + ",";
                await TryCreateObjFile(folder, interfaces[i].Name, interfaces[i].Definition, sideBar, docInfo, interfaces[i].Properties, interfaces[i].Methods);
            }
        }

        if (structs != null && structs.Length > 0)
        {
            for (int i = 0; i < structs.Length; i++)
            {
                searchDataString += @"""" + structs[i].Name + @"""" + ",";
                await TryCreateObjFile(folder, structs[i].Name, structs[i].Definition, sideBar, docInfo, structs[i].Properties, structs[i].Methods);
            }
        }

        if (enums != null && enums.Length > 0)
        {
            for (int i = 0; i < enums.Length; i++)
            {
                searchDataString += @"""" + enums[i].Name + @"""" + ",";
                await TryCreateObjFile(folder, enums[i].Name, enums[i].Definition, sideBar, docInfo, null, null, enums[i].EnumMembers);
            }
        }

        if (!string.IsNullOrEmpty(searchDataString) && !string.IsNullOrWhiteSpace(searchDataString))
        {
            IStorageFile? searchDataFile = await parentFolder.CreateFileAsync("search-data.js");
            if (searchDataFile == null) { return; }

            Stream stream = await searchDataFile.OpenWriteAsync();
            StreamWriter writer = new StreamWriter(stream);

            await writer.WriteAsync($"window.searchablePages = [{searchDataString}]");
            writer.Close(); await writer.DisposeAsync();
            stream.Close(); await stream.DisposeAsync();
            searchDataFile.Dispose();
        }
    }

    private async Task<bool> TryCreateObjFile(IStorageFolder folder, string name, string? definition, string sideBar, DocumentInformation docInfo, Declaration[]? properties = null, Declaration[]? methods = null, Declaration[]? enumMembers = null)
    {
        IStorageFile? storageFile = await folder.CreateFileAsync($"{name}.html");
        if (storageFile == null) { return false; }
        Stream stream = await storageFile.OpenWriteAsync();
        StreamWriter writer = new StreamWriter(stream);
        string classOutput = GeneratePage(name, definition, sideBar, docInfo, properties, methods, enumMembers);
        await writer.WriteAsync(classOutput);
        writer.Close(); writer.Dispose();
        stream.Close(); await stream.DisposeAsync();
        storageFile.Dispose();
        return true;
    }

    private async Task<bool> GenerateHomePage(IStorageFolder outputPath, string sidebar, DocumentInformation docInfo)
    {
        // string filePath = Path.Combine(outputPath, "index.html");
        IStorageFile? file = await outputPath.CreateFileAsync("index.html");
        if (file == null) { return false; }

        Stream stream = await file.OpenWriteAsync();

        string output = @$"<!DOCTYPE html>
                <html lang=""en"">

                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>{docInfo.ProjectName} Documentation</title>
                    <link rel=""stylesheet"" href=""./homePageStyles.css"">
                    <link rel=""stylesheet"" href=""./sidebar.css"">
                    <script src=""./search-data.js"" defer></script>
                    <script src=""./helper.js"" defer></script>
                </head>

                <body> 
                

                    <aside class=""sidebar"">
                    <a href=""./"">
                        <h2>{docInfo.ProjectName}</h2>
                    </a>

                    <div class=""search-container"">
                <input type=""text"" id=""search"" placeholder=""Search..."" autocomplete=""off"">
                <div id=""search-results"" class=""search-results""></div>
            </div>

                    {sidebar}
                    </aside> 

                    <!-- Main Content -->
                        <div class=""content"">
                            <h1>{docInfo.ProjectName}</h1>
                            <p>{docInfo.ProjectDescription}</p>
                            {GetGlobalRelationshipGraph(docInfo.GenerateInheritanceGraphs)}
                        </div>
                </body>

                </html>";

        StreamWriter streamWriter = new StreamWriter(stream);
        await streamWriter.WriteAsync(output);
        streamWriter.Close();
        streamWriter.Dispose();
        stream.Close();
        stream.Dispose();
        file.Dispose();
        return true;
    }

    private string GetGlobalRelationshipGraph(bool generateHomePageGraph)
    {
        if (!generateHomePageGraph) { return ""; }
        return @$"<img src=""./globalInheritanceGraph.png"" alt=""Global Relationship Graph"">";
    }

    private string GeneratePage(string objectName, string? objectDefinition, string sidebar, DocumentInformation docInfo, Declaration[]? properties = null, Declaration[]? methods = null, Declaration[]? enumMembers = null)
    {
        string output = @$"<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
    <title>Everybody Bleeds Documentation: {objectName}</title>

    <link rel=""stylesheet"" href=""../docStyles.css"">
    <link rel=""stylesheet"" href=""../sidebar.css"">
    <script src=""../search-data.js"" defer></script>
    <script src=""../docsHelpers.js"" defer></script>
</head>

<body>
    <div class=""page"">
        <aside class=""sidebar"">
            <a href=""../"" class=""brand"">
                <h2>{docInfo.ProjectName}</h2>
            </a>
            
            <div class=""search-container"">
                <input type=""text"" id=""search"" placeholder=""Search..."" autocomplete=""off"">
                <div id=""search-results"" class=""search-results""></div>
            </div>

            {sidebar}
        </aside>
        

        <main class=""content"">
            <header class=""doc-header"">
                <h1>{objectName} </h1>
                <p class=""lead"">{objectDefinition}</p>
                
            </header>

            {GetObjDiagram(objectName, docInfo)}

            <section class=""members"">
                {GenerateProperties(properties)}
                {GenerateMethods(methods)}
                {GenerateEnumMembers(enumMembers)}
            </section>

            <footer class=""doc-footer"">
                <small>Generated from: <code>Source</code></small>
            </footer>
        </main>
    </div>
</body>

</html>";


        return output;
    }

    private object GetObjDiagram(string objectName, DocumentInformation docInfo)
    {
        if (docInfo.IndividualObjsGraphs != null && !docInfo.IndividualObjsGraphs.ContainsKey(objectName)) { return ""; }
        return @$"            <section class=""diagrams"">
                <div class=""diagram-placeholder""><img src=""../objGraphs/{objectName}_Graph.png"" alt=""Graph""></div>
            </section>";
    }

    private string GenerateEnumMembers(Declaration[]? enumMembers)
    {
        if (enumMembers == null || enumMembers.Length == 0) { return ""; }

        string output = $@"<div>
                    <h2>Enum Members</h2>

                    {GetAllEnumMembers(enumMembers)}
                </div>";

        return output;
    }

    private string GetAllEnumMembers(Declaration[] enumMembers)
    {
        string output = $@"";

        foreach (Declaration member in enumMembers)
        {
            output += $@"<article class=""member"">
                        <button class=""member-toggle"" onclick=""toggleMember(this)"">◈ {member.Name}</button>
                        <div class=""member-body"">
                            <p><strong>Signature:</strong> <code>{member.Name}</code></p>
                            <p>{member.Definition}</p>
                        </div>
                    </article>
";
        }

        return output;
    }

    private string GenerateMethods(Declaration[]? methods)
    {
        if (methods == null || methods.Length == 0) { return ""; }

        string output = $@"<div>
                    <h2>Methods</h2>

                    {GetAllMethods(methods)}
                </div>";

        return output;
    }

    private string GetAllMethods(Declaration[] methods)
    {
        string output = $@"";

        foreach (Declaration method in methods)
        {
            output += $@"<article class=""member"">
                        <button class=""member-toggle"" onclick=""toggleMember(this)"">◈ {method.Name}</button>
                        <div class=""member-body"">
                            <p><strong>Signature:</strong> <code>{method.Type} {method.Name}({method.GetMethodParameters()})</code></p>
                            <p>{method.Definition}</p>
                        </div>
                    </article>
";
        }

        return output;
    }

    

    private string GenerateProperties(Declaration[]? properties)
    {
        if (properties == null || properties.Length == 0) { return ""; }

        string output = $@"<div>
                    <h2>Properties</h2>

                    {GetAllProperties(properties)}
                </div>";

        return output;
    }

    private string GetAllProperties(Declaration[] properties)
    {
        string output = $@"";

        foreach (Declaration property in properties)
        {
            output += $@"<article class=""member"">
                        <button class=""member-toggle"" onclick=""toggleMember(this)"">◈ {property.Name}</button>
                        <div class=""member-body"">
                            <p><strong>Signature:</strong> <code>{property.Type} {property.Name}</code></p>
                            <p>{property.Definition}</p>
                        </div>
                    </article>
";
        }

        return output;
    }


    #region Sidebar For Home Page
    private string GenerateSideBar(ClassDeclaration[]? classes, EnumDeclaration[]? enums, InterfaceDeclaration[]? interfaces, StructDeclaration[]? structs, DocumentInformation docinfo)
    {
        string output = "";
        output = @$"
                            <div class=""nav-section"">
                ";

        if (classes != null && classes.Length > 0)
        {
            output += WriteSidebarClasses(classes);
        }

        if (interfaces != null && interfaces.Length > 0)
        {
            output += WriteSidebarInterfaces(interfaces);
        }

        if (structs != null && structs.Length > 0)
        {
            output += WriteSidebarStructs(structs);
        }

        if (enums != null && enums.Length > 0)
        {
            output += WriteSidebarEnums(enums);
        }


        output += @"
                </div>";

        return output;
    }

    private string WriteSidebarStructs(StructDeclaration[] structs)
    {
        string output = @$"<button class=""toggle-btn"" onclick=""toggleMenu('structs')"">Structs <span class=""arrow"">▼</span></button>
            <div id=""structs"" class=""nav-links"">
                    ";


        foreach (StructDeclaration current in structs)
        {
            output += @$"<a href=""./objs/{current.Name}.html"">{current.Name}</a>";
        }


        output += @"       </div>
                ";

        return output;
    }

    private string WriteSidebarInterfaces(InterfaceDeclaration[] interfaces)
    {

        string output = @$"<button class=""toggle-btn"" onclick=""toggleMenu('interfaces')"">Interfaces <span class=""arrow"">▼</span></button>
            <div id=""interfaces"" class=""nav-links"">
                    ";


        foreach (InterfaceDeclaration current in interfaces)
        {
            output += @$"<a href=""./objs/{current.Name}.html"">{current.Name}</a>";
        }


        output += @"       </div>
                ";

        return output;
    }

    private string WriteSidebarEnums(EnumDeclaration[] enums)
    {
        string output = @$"<button class=""toggle-btn"" onclick=""toggleMenu('enums')"">Enums <span class=""arrow"">▼</span></button>
            <div id=""enums"" class=""nav-links"">
                    ";


        foreach (EnumDeclaration current in enums)
        {
            output += @$"<a href=""./objs/{current.Name}.html"">{current.Name}</a>";
        }


        output += @"       </div>
                ";

        return output;
    }

    private string WriteSidebarClasses(ClassDeclaration[] classes)
    {
        string output = @$"<button class=""toggle-btn"" onclick=""toggleMenu('classes')"">Classes <span class=""arrow"">▼</span></button>
            <div id=""classes"" class=""nav-links"">
                    ";


        foreach (ClassDeclaration current in classes)
        {
            output += @$"<a href=""./objs/{current.Name}.html"">{current.Name}</a>";
        }


        output += @"       </div>
                ";
        return output;
    }
    #endregion


    #region Sidebar For Object Pages
    private string GenerateSideBarForObjs(ClassDeclaration[]? classes, EnumDeclaration[]? enums, InterfaceDeclaration[]? interfaces, StructDeclaration[]? structs, DocumentInformation docinfo)
    {
        string output = "";
        output = @$"
                            <div class=""nav-section"">
                ";

        if (classes != null && classes.Length > 0)
        {
            output += WriteSidebarClassesForObjs(classes);
        }

        if (interfaces != null && interfaces.Length > 0)
        {
            output += WriteSidebarInterfacesForObjs(interfaces);
        }

        if (structs != null && structs.Length > 0)
        {
            output += WriteSidebarStructsForObjs(structs);
        }

        if (enums != null && enums.Length > 0)
        {
            output += WriteSidebarEnumsForObjs(enums);
        }


        output += @"
                </div>";

        return output;
    }

    private string WriteSidebarStructsForObjs(StructDeclaration[] structs)
    {
        string output = @$"<button class=""toggle-btn"" onclick=""toggleMenu('structs')"">Structs <span class=""arrow"">▼</span></button>
            <div id=""structs"" class=""nav-links"">
                    ";


        foreach (StructDeclaration current in structs)
        {
            output += @$"<a href=""./{current.Name}.html"">{current.Name}</a>";
        }


        output += @"       </div>
                ";

        return output;
    }

    private string WriteSidebarInterfacesForObjs(InterfaceDeclaration[] interfaces)
    {

        string output = @$"<button class=""toggle-btn"" onclick=""toggleMenu('interfaces')"">Interfaces <span class=""arrow"">▼</span></button>
            <div id=""interfaces"" class=""nav-links"">
                    ";


        foreach (InterfaceDeclaration current in interfaces)
        {
            output += @$"<a href=""./{current.Name}.html"">{current.Name}</a>";
        }


        output += @"       </div>
                ";

        return output;
    }

    private string WriteSidebarEnumsForObjs(EnumDeclaration[] enums)
    {
        string output = @$"<button class=""toggle-btn"" onclick=""toggleMenu('enums')"">Enums <span class=""arrow"">▼</span></button>
            <div id=""enums"" class=""nav-links"">
                    ";


        foreach (EnumDeclaration current in enums)
        {
            output += @$"<a href=""./{current.Name}.html"">{current.Name}</a>";
        }


        output += @"       </div>
                ";

        return output;
    }

    private string WriteSidebarClassesForObjs(ClassDeclaration[] classes)
    {
        string output = @$"<button class=""toggle-btn"" onclick=""toggleMenu('classes')"">Classes <span class=""arrow"">▼</span></button>
            <div id=""classes"" class=""nav-links"">
                    ";


        foreach (ClassDeclaration current in classes)
        {
            output += @$"<a href=""./{current.Name}.html"">{current.Name}</a>";
        }


        output += @"       </div>
                ";
        return output;
    }
    #endregion
}