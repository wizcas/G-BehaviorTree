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
    IParentNode AddChild(Node child);
    /// <summary>
    /// Appending multiple children to this node. It will not remove any
    /// existing children.
    /// </summary
    IParentNode AddChildren(params Node[] children);
    /// <summary>
    /// Removing a child from this node. Depending on the type of the node,
    /// this may remove the current child or remove the child from a list.
    /// </summary>
    bool RemoveChild(Node child);
    /// <summary>
    /// Called when a child node is exited, which is when the parent node
    /// needs to determine its next state.
    /// </summary>
    void AfterChildExit(Node child);
    /// <summary>
    /// Cast this instance to <see cref="Node"/> type.
    T Cast<T>() where T : Node;
}
public interface IParentNode<TNode> : IParentNode where TNode : Node {
    new TNode AddChild(Node child);
    new TNode AddChildren(params Node[] children);
}