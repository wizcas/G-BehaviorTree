namespace GBT.Sharp.Core.Nodes;

public enum NodeState {
    Unvisited,
    Running,
    Success,
    Failure,
}

/// <summary>
/// Represents a node that can have children attached to it.
/// </summary>
public interface IParentNode {
    public IEnumerable<Node> Children { get; }
    /// <summary>
    /// Adding a child to this node. Depending on the type of the node,
    /// this may substitute the current child or add the child to a list.
    /// </summary>
    void AddChild(Node child);
    /// <summary>
    /// Removing a child from this node. Depending on the type of the node,
    /// this may remove the current child or remove the child from a list.
    /// </summary>
    bool RemoveChild(Node child);
    /// <summary>
    /// Called when a child node is exited, which is when the parent node
    /// needs to determine its next state.
    /// </summary>
    void OnChildExit(Node child);
}