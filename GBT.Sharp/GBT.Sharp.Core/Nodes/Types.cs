namespace GBT.Nodes;

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
    public IEnumerable<GBTNode> Children { get; }
    public int GetChildIndex(GBTNode child);
    /// <summary>
    /// Adding a child to this node. Depending on the type of the node,
    /// this may substitute the current child or add the child to a list.
    /// </summary>
    IParentNode AddChild(GBTNode child);
    /// <summary>
    /// Appending multiple children to this node. It will not remove any
    /// existing children.
    /// </summary
    IParentNode AddChildren(params GBTNode[] children);
    /// <summary>
    /// Removing a child from this node. Depending on the type of the node,
    /// this may remove the current child or remove the child from a list.
    /// </summary>
    bool RemoveChild(GBTNode child);
    /// <summary>
    /// Called when a child node is exited, which is when the parent node
    /// needs to determine its next state.
    /// </summary>
    void AfterChildExit(GBTNode child);
    /// <summary>
    /// Cast this instance to <see cref="GBTNode"/> type.
    T Cast<T>() where T : GBTNode;
}
public interface IParentNode<TNode> : IParentNode where TNode : GBTNode {
    new TNode AddChild(GBTNode child);
    new TNode AddChildren(params GBTNode[] children);
}