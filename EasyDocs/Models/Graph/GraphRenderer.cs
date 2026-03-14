using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace EasyDocs.Models.Graph;

/// <summary>
/// Provides functionality to render a simple hierarchical directed graph
/// into a bitmap image using SkiaSharp. The renderer arranges nodes in
/// top-down layers, connects them with straight lines, and draws labeled
/// rounded rectangles for each node.
/// </summary>
/// <remarks>
/// This renderer is designed for lightweight visualization of tree-like or
/// acyclic directed graphs. It uses a simple Sugiyama-inspired layering
/// algorithm to determine node placement.
/// </remarks>
public class GraphRenderer
{
    /// <summary>
    /// Tracks the current horizontal offset for placing leaf nodes during
    /// recursive layout operations.
    /// </summary>
    private float currentLeafX = 50;

    /// <summary>
    /// Gets or sets the vertical distance (in pixels) between consecutive
    /// layers of nodes.
    /// </summary>
    public int LayerSpacing { get; set; }

    /// <summary>
    /// Gets or sets the horizontal distance (in pixels) between sibling
    /// nodes within the same layer.
    /// </summary>
    public int NodeSpacing { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphRenderer"/> class
    /// using default spacing values for nodes and layers.
    /// </summary>
    public GraphRenderer()
    {
        NodeSpacing = 150;
        LayerSpacing = 150;
    }

    /// <summary>
    /// Builds a sample graph and renders it to the specified output file path
    /// as a PNG image. Intended as a quick demonstration of rendering.
    /// </summary>
    /// <param name="outputPath">The file system path where the rendered PNG image will be saved.</param>
    /// <remarks>
    /// The generated graph includes a small hierarchy of labeled nodes and
    /// can be used to test or showcase rendering functionality.
    /// </remarks>
    public static void TestRun(string outputPath)
    {
        Graph g = new Graph("Example");
        g.AddEdge("A", "B");
        g.AddEdge("A", "C");
        g.AddEdge("C", "E");
        g.AddEdge("C", "F");
        g.AddEdge("C", "D");
        g.AddEdge("D", "G");

        GraphRenderer render = new GraphRenderer();
        var bitmap = render.RenderSugiyamaStyleGraph(g);
        using FileStream fs = File.OpenWrite(outputPath);
        bitmap.Save(fs);
    }

    /// <summary>
    /// Computes the number of incoming edges for each node in the provided
    /// list of nodes. This helps identify root nodes (those with no incoming edges).
    /// </summary>
    /// <param name="nodes">A list of nodes in the graph.</param>
    /// <returns>
    /// A dictionary mapping each node to its count of incoming edges.
    /// </returns>
    private Dictionary<Node, int> ComputeIncomingCounts(List<Node> nodes)
    {
        var incoming = nodes.ToDictionary(n => n, n => 0);

        foreach (var n in nodes)
            foreach (var c in n.Children)
                incoming[c]++;

        return incoming;
    }

    /// <summary>
    /// Renders the given graph as a hierarchical image with layered nodes
    /// and connecting lines.
    /// </summary>
    /// <param name="graph">The graph instance to render.</param>
    /// <returns>
    /// An <see cref="Avalonia.Media.Imaging.Bitmap"/> object representing
    /// the rendered visualization.
    /// </returns>
    /// <remarks>
    /// This method computes node layers, positions, and dimensions, then
    /// draws them using SkiaSharp. It uses a simple top-down layout suitable
    /// for acyclic directed graphs.
    /// </remarks>
    public Bitmap RenderSugiyamaStyleGraph(Graph graph)
    {
        List<Node> nodes = graph.Nodes.Values.ToList();

        Dictionary<Node, int> incoming = ComputeIncomingCounts(nodes);
        List<Node> roots = incoming.Where(p => p.Value == 0).Select(p => p.Key).ToList();

        List<List<Node>> layers = ComputeLayers(roots, nodes);
        Dictionary<Node, Vector2> positions = ComputeNodePositions(layers);

        SKFont font = new SKFont(SKTypeface.Default, 16);
        foreach (Node node in nodes)
            MeasureNode(node, font);

        Vector2 boundingBox = FindBoundingBox(positions);
        int imgW = (int)(boundingBox.X + 100);
        int imgH = (int)(boundingBox.Y + 100);

        using SKBitmap skBitmap = new SKBitmap(imgW, imgH);
        using SKCanvas canvas = new SKCanvas(skBitmap);
        canvas.Clear(SKColors.White);

        SKPaint linePaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true };

        DrawEdges(positions, canvas, linePaint);
        DrawNodes(positions, canvas, font);

        using SKData data = skBitmap.Encode(SKEncodedImageFormat.Png, 100);
        using Stream ms = data.AsStream();
        return new Bitmap(ms);
    }

    /// <summary>
    /// Finds the maximum X and Y coordinates among all node positions,
    /// effectively determining the bounding box of the rendered graph.
    /// </summary>
    /// <param name="positions">Dictionary of node-to-position mappings.</param>
    /// <returns>
    /// A <see cref="Vector2"/> structure containing the maximum X and Y
    /// coordinates found.
    /// </returns>
    private Vector2 FindBoundingBox(Dictionary<Node, Vector2> positions)
    {
        float maxX = 0;
        float maxY = 0;

        foreach (Vector2 p in positions.Values)
        {
            if (p.X > maxX) maxX = p.X;
            if (p.Y > maxY) maxY = p.Y;
        }

        return new Vector2(maxX, maxY);
    }

    /// <summary>
    /// Measures the visual size of a node based on its text label, using
    /// the provided font, and adds padding for aesthetics.
    /// </summary>
    /// <param name="node">The node to measure.</param>
    /// <param name="font">The SkiaSharp font used for measurement.</param>
    /// <remarks>
    /// The computed width and height values are stored directly on the node
    /// and later used for layout and rendering.
    /// </remarks>
    private void MeasureNode(Node node, SKFont font)
    {
        font.MeasureText(node.Name, out SKRect bounds);

        node.Width = bounds.Width + 20;
        node.Height = bounds.Height + 20;

        foreach (Node child in node.Children)
            MeasureNode(child, font);
    }

    /// <summary>
    /// Recursively computes and assigns (x, y) coordinates for each node
    /// in a hierarchical layout.
    /// </summary>
    /// <param name="node">The current node being positioned.</param>
    /// <param name="depth">The current layer depth.</param>
    /// <param name="pos">Dictionary storing node position assignments.</param>
    /// <param name="spacing">Horizontal spacing between sibling nodes.</param>
    /// <param name="maxDepth">Maximum depth of the entire tree.</param>
    /// <param name="inverted">If true, flips the layout vertically (root at bottom).</param>
    /// <returns>The X-coordinate of the positioned node.</returns>
    private float LayoutNodes(Node node, int depth, Dictionary<Node, Vector2> pos, float spacing, int maxDepth, bool inverted)
    {
        if (node.Children.Count == 0)
        {
            pos[node] = new Vector2(currentLeafX, depth * 100 + 50);
            currentLeafX += spacing;
            return pos[node].X;
        }

        float minX = float.MaxValue;
        float maxX = float.MinValue;

        foreach (var child in node.Children)
        {
            float childX = LayoutNodes(child, depth + 1, pos, spacing, maxDepth, inverted);
            if (childX < minX) minX = childX;
            if (childX > maxX) maxX = childX;
        }

        float nodeX = (minX + maxX) / 2;
        float nodeY = inverted ? (maxDepth - depth) * 100 + 50 : depth * 100 + 50;

        pos[node] = new Vector2(nodeX, nodeY);
        return nodeX;
    }

    /// <summary>
    /// Draws all nodes as rounded rectangles with centered text labels
    /// on the provided Skia canvas.
    /// </summary>
    /// <param name="positions">Dictionary mapping nodes to positions.</param>
    /// <param name="canvas">The Skia canvas to draw on.</param>
    /// <param name="font">The font used for rendering node text.</param>
    private void DrawNodes(Dictionary<Node, Vector2> positions, SKCanvas canvas, SKFont font)
    {
        foreach (KeyValuePair<Node, Vector2> kvp in positions)
        {
            Node node = kvp.Key;
            Vector2 pos = kvp.Value;

            float left = pos.X - node.Width / 2;
            float top = pos.Y - node.Height / 2;
            float right = left + node.Width;
            float bottom = top + node.Height;

            SKRect rect = new SKRect(left, top, right, bottom);
            canvas.DrawRoundRect(rect, 8, 8, node.ShapePaint);

            font.MeasureText(node.Name, out SKRect bounds);

            float textX = pos.X - bounds.Width / 2;
            float textY = pos.Y + bounds.Height / 2;

            canvas.DrawText(node.Name, textX, textY, font, node.TextPaint);
        }
    }

    /// <summary>
    /// Draws connecting lines between parent and child nodes
    /// using the provided Skia canvas and paint settings.
    /// </summary>
    /// <param name="positions">Dictionary mapping nodes to their positions.</param>
    /// <param name="canvas">The Skia canvas used for drawing.</param>
    /// <param name="linePaint">The paint used for drawing edges.</param>
    private void DrawEdges(Dictionary<Node, Vector2> positions, SKCanvas canvas, SKPaint linePaint)
    {
        foreach (var kvp in positions)
        {
            Node node = kvp.Key;
            Vector2 p = kvp.Value;

            foreach (Node child in node.Children)
            {
                Vector2 cp = positions[child];
                canvas.DrawLine(p.X, p.Y, cp.X, cp.Y, linePaint);
            }
        }
    }

    /// <summary>
    /// Recursively determines the maximum hierarchical depth (number of layers)
    /// in the subtree starting from the specified node.
    /// </summary>
    /// <param name="node">The node from which to begin depth calculation.</param>
    /// <returns>The maximum depth of the subtree.</returns>
    private int GetMaxDepth(Node node)
    {
        if (node.Children.Count == 0)
            return 0;

        int max = 0;
        foreach (var child in node.Children)
        {
            int depth = GetMaxDepth(child);
            if (depth > max) max = depth;
        }
        return max + 1;
    }

    /// <summary>
    /// Collects all nodes reachable from a given root node using
    /// depth-first traversal.
    /// </summary>
    /// <param name="root">The root node from which traversal begins.</param>
    /// <returns>A list containing all reachable nodes.</returns>
    private List<Node> CollectNodes(Node root)
    {
        var result = new HashSet<Node>();
        var stack = new Stack<Node>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (result.Add(node))
            {
                foreach (var child in node.Children)
                    stack.Push(child);
            }
        }

        return result.ToList();
    }

    /// <summary>
    /// Computes the in-degree (number of incoming edges) for each node.
    /// </summary>
    /// <param name="nodes">The collection of nodes to analyze.</param>
    /// <returns>
    /// A dictionary mapping each node to its in-degree value.
    /// </returns>
    private static Dictionary<Node, int> ComputeInDegree(IEnumerable<Node> nodes)
    {
        var indegree = nodes.ToDictionary(n => n, n => 0);

        foreach (var node in nodes)
            foreach (var child in node.Children)
                indegree[child]++;

        return indegree;
    }

    /// <summary>
    /// Performs a breadth-first traversal to assign nodes to hierarchical
    /// layers starting from the provided root nodes.
    /// </summary>
    /// <param name="roots">The list of root nodes (no incoming edges).</param>
    /// <param name="allNodes">All nodes in the graph.</param>
    /// <returns>
    /// A list of layers, where each layer is a list of nodes at the same depth.
    /// </returns>
    private static List<List<Node>> ComputeLayers(List<Node> roots, List<Node> allNodes)
    {
        var layers = new List<List<Node>>();
        var assigned = new HashSet<Node>();
        Queue<(Node node, int depth)> queue = new();

        foreach (var r in roots)
            queue.Enqueue((r, 0));

        while (queue.Count > 0)
        {
            var (node, depth) = queue.Dequeue();

            if (layers.Count <= depth)
                layers.Add(new List<Node>());

            if (!assigned.Contains(node))
            {
                layers[depth].Add(node);
                assigned.Add(node);

                foreach (var child in node.Children)
                    queue.Enqueue((child, depth + 1));
            }
        }

        return layers;
    }

    /// <summary>
    /// Computes a two-dimensional position for each node in the layout
    /// based on its assigned layer and index within that layer.
    /// </summary>
    /// <param name="layers">A list of layers, each containing nodes at a particular depth.</param>
    /// <returns>
    /// A dictionary mapping each node to its corresponding <see cref="Vector2"/> position.
    /// </returns>
    private Dictionary<Node, Vector2> ComputeNodePositions(List<List<Node>> layers)
    {
        var positions = new Dictionary<Node, Vector2>();

        for (int depth = 0; depth < layers.Count; depth++)
        {
            var layer = layers[depth];
            float y = depth * LayerSpacing + 50;

            for (int i = 0; i < layer.Count; i++)
            {
                float x = i * NodeSpacing + 100;
                positions[layer[i]] = new Vector2(x, y);
            }
        }

        return positions;
    }
}