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
    /// Check whether this node has done execution and can be exited. 
    /// If yes, any exit steps of the node will be run.
    /// </summary>
    void TryExit();
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
    void AddChild(INode child);
    bool RemoveChild(INode child);
    void OnChildExit(INode child);
}

public interface ILeafNode : INode { }

public enum NodeState {
    Unvisited,
    Running,
    Success,
    Failure,
}
