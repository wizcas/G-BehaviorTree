using MessagePack;

namespace GBT.Sharp.Core.Nodes;

[MessagePackObject(true)]
public record TreeData(
     NodeData[] Nodes,
     string? RootID);

/// <summary>
/// Persistable data for a node.
/// </summary>
[MessagePackObject(true)]
public record struct NodeData(
     Type NodeType,
     string ID,
     string Name,
     string? ParentID,
     bool IsDisabled) {
    /// <summary>
    /// Arbitrary data that can be saved and loaded.
    /// </summary>
    public Dictionary<string, object?> Extra { get; } = new();

    public static NodeData FromNode(Node node) {
        return new(node.GetType(), node.ID, node.Name, node.Parent?.ID, node.IsDisabled);
    }
    public readonly Node LoadNode(Dictionary<string, Node> loadedNodes) {
        var node = (Node)Activator.CreateInstance(NodeType, new object[] { ID, Name })!;
        node.ReadSaveData(this);
        loadedNodes.Add(ID, node);
        if (!string.IsNullOrEmpty(ParentID)) {
            if (loadedNodes.TryGetValue(ParentID, out Node? parent) && parent is IParentNode) {
                node.Parent = parent;
            } else {
                BehaviorTree.Logger.Warn($"failed binding saved parent - parent node not loaded yet or invalid", node);
            }
        }
        return node;
    }
    public readonly TNode LoadNode<TNode>(Dictionary<string, Node> loadedNodes) where TNode : Node {
        return (TNode)LoadNode(loadedNodes);
    }
}

public class NodeLoader {
    private readonly Dictionary<string, Node> _cache = new();

    public void Reset() {
        _cache.Clear();
    }

    public TNode Load<TNode>(NodeData data) where TNode : Node {
        return data.LoadNode<TNode>(_cache);
    }
    public Node Load(NodeData data) {
        return data.LoadNode(_cache);
    }
    public Dictionary<string, Node> LoadAll(IEnumerable<NodeData> datas) {
        foreach (NodeData data in datas) {
            data.LoadNode(_cache);
        }
        return _cache;
    }
}