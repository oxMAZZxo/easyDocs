using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia.Media.Imaging;
using EasyDocs.Models.Declarations;
using EasyDocs.Models.DocumentInfo;
using EasyDocs.Models.Graph;
using SkiaSharp;

namespace EasyDocs.Helpers;

/// <summary>
/// The Inheritance Graph Generator is a static wrapper class for creating and rendering graph from declarations.
/// </summary>
public static class InheritanceGraphGenerator
{
    /// <summary>
    /// Generated a global inheritance graph, from given declarations.
    /// </summary>
    /// <param name="classes">The source classes.</param>
    /// <param name="interfaces">The source interfaces.</param>
    /// <param name="declarationColours">The declaration colours which will be assigned on the nodes.</param>
    /// <returns>Returns a bitmap which represents the global inheritance hierarchy of the source codebase.</returns>
    public static Bitmap GenerateGlobalGraph(ClassDeclaration[]? classes, InterfaceDeclaration[]? interfaces, DeclarationColours declarationColours)
    {
        Graph graph = new Graph("Global Inheritance Graph");
        if (classes != null && classes.Length > 0)
        {
            foreach (ClassDeclaration dec in classes)
            {
                if (dec.BaseTypes != null && dec.BaseTypes.Length > 0)
                {
                    foreach (string type in dec.BaseTypes)
                    {
                        Edge currentEdge = graph.AddEdge(type, dec.Name);
                        currentEdge.Child.ShapePaint = new SKPaint {Color = new SKColor(declarationColours.ClassDeclarationColour.Argb), StrokeWidth = 2, IsAntialias = true };
                        if (type[0] == 'I')
                        {
                            currentEdge.Parent.ShapePaint = new SKPaint { Color = new SKColor(declarationColours.InterfaceDeclarationColour.Argb), StrokeWidth = 2, IsAntialias = true };
                        }
                        else
                        {
                            currentEdge.Parent.ShapePaint = new SKPaint { Color = new SKColor(declarationColours.ClassDeclarationColour.Argb), StrokeWidth = 2, IsAntialias = true };
                        }
                    }
                }
            }
        }

        if (interfaces != null && interfaces.Length > 0)
        {
            foreach (InterfaceDeclaration dec in interfaces)
            {
                Node node = graph.AddNode(dec.Name);

                node.ShapePaint = new SKPaint { Color = SKColors.Orange, StrokeWidth = 2, IsAntialias = true };
            }
        }

        GraphRenderer renderer = new GraphRenderer();
        renderer.LayerSpacing = 100;
        renderer.NodeSpacing = 200;

        return renderer.RenderSugiyamaStyleGraph(graph);
    }

    /// <summary>
    /// Generate Individual Graphs for each Class Declaration, in the process assigning the right colours to the nodes, rendering each graph and saving it in the specified outputPath
    /// </summary>
    /// <param name="classes"></param>
    /// <param name="declarationColours"></param>
    /// <param name="outputPath"></param>
    /// <returns>Returns a Dictionary of string to string key value pairs, where the key is the name of the Class, and the value being a bitmap which contains the rendered graph.</returns>
    public static Dictionary<string, Bitmap> GenerateIndividualGraphs(ClassDeclaration[]? classes, DeclarationColours declarationColours)
    {
        Dictionary<string, Graph> graphs = new Dictionary<string, Graph>();

        if (classes != null)
        {
            Dictionary<string, ClassDeclaration> classDictionary = classes.ToDictionary(x => x.Name, x => x);
            for (int i = 0; i < classes.Length; i++)
            {
                if (classes[i].Name == "NPC")
                {
                    Debug.WriteLine("NPC");
                }
                if (classes[i].BaseTypes != null && classes[i].BaseTypes.Length > 0)
                {
                    Graph graph = new Graph(classes[i].Name);
                    HandleBaseTypes(graph, classes[i], classDictionary, declarationColours);
                    graphs.Add(classes[i].Name, graph);
                }
            }
        }

        Dictionary<string, Bitmap> bitmaps = RenderGraphs(graphs);

        return bitmaps;
    }

    /// <summary>
    /// Gets the base types of a declaration and generates nodes and edges, where the parent and child are assigned accordingly.
    /// </summary>
    /// <param name="graph">The current graph that is being generated.</param>
    /// <param name="current">The current class declaration.</param>
    /// <param name="sourceClasses">A dictionary of the source classes, where the key to a value is a classes name.</param>
    /// <param name="declarationColours">The declaration colours.</param>
    private static void HandleBaseTypes(Graph graph, ClassDeclaration current, Dictionary<string, ClassDeclaration> sourceClasses, DeclarationColours declarationColours)
    {
        foreach (string b in current.BaseTypes)
        {
            Edge edge = graph.AddEdge(b, current.Name);
            edge.Child.ShapePaint = new SKPaint { Color = new SKColor(declarationColours.ClassDeclarationColour.Argb) };
            if (b[0] == 'I')
            {
                edge.Parent.ShapePaint = new SKPaint { Color = new SKColor(declarationColours.InterfaceDeclarationColour.Argb) };
            }
            else
            {
                edge.Parent.ShapePaint = new SKPaint { Color = new SKColor(declarationColours.ClassDeclarationColour.Argb) };

            }

            if (sourceClasses.ContainsKey(b))
            {
                HandleBaseTypes(graph, sourceClasses[b], sourceClasses, declarationColours);
            }
        }
    }

    /// <returns>Returns a Dictionary of string to string key value pairs, where the key is the name of the Class, and the value being a bitmap which contains the rendered graph.</returns>
    private static Dictionary<string, Bitmap> RenderGraphs(Dictionary<string, Graph> graphs)
    {
        Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
        GraphRenderer renderer = new GraphRenderer();
        renderer.LayerSpacing = 100;
        renderer.NodeSpacing = 200;

        foreach (string graphName in graphs.Keys)
        {
            Bitmap bitmap = renderer.RenderSugiyamaStyleGraph(graphs[graphName]);
            bitmaps.Add(graphs[graphName].Name, bitmap);
        }

        return bitmaps;
    }
}