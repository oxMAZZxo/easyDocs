
using System.Collections.Generic;
using SkiaSharp;

namespace EasyDocs.Models.Graph;

/// <summary>
/// Represents a single node within a directed graph.
/// Each node has a name, a set of children, and rendering properties
/// used during visualization.
/// </summary>
public class Node
{
    /// <summary>
    /// Gets the textual label displayed for this node.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the visual width of the node, computed from its text content.
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// Gets or sets the visual height of the node, computed from its text content.
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// Gets the list of child nodes connected to this node.
    /// </summary>
    public List<Node> Children { get; }

    /// <summary>
    /// Gets or sets the paint used for rendering the node's background or border.
    /// </summary>
    public SKPaint ShapePaint { get; set; }

    /// <summary>
    /// Gets or sets the paint used for rendering the node's text.
    /// </summary>
    public SKPaint TextPaint { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class with
    /// default rendering styles.
    /// </summary>
    /// <param name="name">The text label for this node.</param>
    public Node(string name)
    {
        Name = name;
        ShapePaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true };
        TextPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        Children = new List<Node>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class with custom
    /// rendering styles.
    /// </summary>
    /// <param name="name">The text label for this node.</param>
    /// <param name="shapePaint">The paint used to draw the node’s shape.</param>
    /// <param name="textPaint">The paint used to draw the node’s text.</param>
    public Node(string name, SKPaint shapePaint, SKPaint textPaint)
    {
        Name = name;
        ShapePaint = shapePaint;
        TextPaint = textPaint;
        Children = new List<Node>();
    }
}

/// <summary>
/// Represents a directed graph consisting of named nodes and edges
/// connecting them.
/// </summary>
public class Graph
{
    /// <summary>
    /// Gets the name assigned to this graph.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a dictionary of all nodes in the graph, keyed by node name.
    /// </summary>
    public Dictionary<string, Node> Nodes { get; }

    /// <summary>
    /// Gets a collection of all directed edges in the graph.
    /// </summary>
    public List<Edge> Edges { get; }

    /// <summary>
    /// Initializes a new empty graph with the specified name.
    /// </summary>
    /// <param name="name">The name of the graph.</param>
    public Graph(string name)
    {
        Name = name;
        Nodes = new Dictionary<string, Node>();
        Edges = new List<Edge>();
    }

    /// <summary>
    /// Adds a node to the graph or retrieves an existing one by name.
    /// </summary>
    /// <param name="name">The unique name of the node.</param>
    /// <returns>The existing or newly created <see cref="Node"/>.</returns>
    public Node AddNode(string name)
    {
        Node node;
        if (Nodes.ContainsKey(name))
        {
            node = Nodes[name];
        }
        else
        {
            node = new Node(name);
        }

        return node;
    }

    /// <summary>
    /// Adds a directed edge between two named nodes, creating them if necessary.
    /// </summary>
    /// <param name="parentName">The name of the source (parent) node.</param>
    /// <param name="childName">The name of the destination (child) node.</param>
    /// <returns>
    /// The <see cref="Edge"/> object representing the created connection.
    /// </returns>
    public Edge AddEdge(string parentName, string childName)
    {
        Node parent;
        Node child;

        if (Nodes.ContainsKey(parentName))
            parent = Nodes[parentName];
        else
        {
            parent = new Node(parentName);
            Nodes.Add(parentName, parent);
        }

        if (Nodes.ContainsKey(childName))
            child = Nodes[childName];
        else
        {
            child = new Node(childName);
            Nodes.Add(childName, child);
        }

        Edge edge = new Edge(parent, child);
        return edge;
    }
}

/// <summary>
/// Represents a single directed edge connecting a parent node to a child node.
/// </summary>
public class Edge
{
    /// <summary>
    /// Gets the parent (source) node of the edge.
    /// </summary>
    public Node Parent { get; }

    /// <summary>
    /// Gets the child (destination) node of the edge.
    /// </summary>
    public Node Child { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Edge"/> class, connecting
    /// the specified parent and child nodes.
    /// </summary>
    /// <param name="parent">The parent node of the edge.</param>
    /// <param name="child">The child node of the edge.</param>
    /// <remarks>
    /// This constructor automatically adds the child node to the parent’s
    /// <see cref="Node.Children"/> collection.
    /// </remarks>
    public Edge(Node parent, Node child)
    {
        Parent = parent;
        Child = child;
        Parent.Children.Add(child);
    }
}