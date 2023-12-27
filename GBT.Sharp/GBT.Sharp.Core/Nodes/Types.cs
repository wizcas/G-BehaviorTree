namespace GBT.Sharp.Core.Nodes;

public interface INode {
    string ID { get; }
    string Name { get; }
    NodeState State { get; set; }
    bool IsDisabled { get; set; }
    IParentNode? Parent { get; set; }
    ITreeContext? Context { get; set; }

    /// <summary>
    /// Set the node ready for running.
    /// This method is for internal purpose.
    /// </summary>
    void Initialize();
    /// <summary>
    /// Called everytime the node is executed.
    /// </summary>
    void Tick();
    /// <summary>
    /// Clean up any intermediate state or data that was set
    /// by running this node.
    /// </summary>
    void CleanUp();
    /// <summary>
    /// Reset this node to its initial state and data.
    /// </summary>
    void Reset();
}

public interface IParentNode : INode {
    /// <summary>
    /// Adding a child to this node. Depending on the type of the node,
    /// this may substitute the current child or add the child to a list.
    /// </summary>
    void AddChild(INode child);
    /// <summary>
    /// Removing a child from this node. Depending on the type of the node,
    /// this may remove the current child or remove the child from a list.
    /// </summary>
    bool RemoveChild(INode child);
    /// <summary>
    /// Called when a child node is exited, which is when the parent node
    /// needs to determine its next state.
    /// </summary>
    void OnChildExit(INode child);
}

public interface ILeafNode : INode { }

public enum NodeState {
    Unvisited,
    Running,
    Success,
    Failure,
}