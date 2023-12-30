using GBT.Sharp.Core.Nodes;

namespace GBT.Sharp.Core.Serialization;

public class NodeLoader {
    private readonly Dictionary<string, Node> _cache = new();

    public void Reset() {
        _cache.Clear();
    }

    public TNode Load<TNode>(Node.Data data) where TNode : Node {
        return data.LoadNode<TNode>(_cache);
    }
    public Node Load(Node.Data data) {
        return data.LoadNode(_cache);
    }
    public Dictionary<string, Node> LoadAll(IEnumerable<Node.Data> datas) {
        foreach (Node.Data data in datas) {
            data.LoadNode(_cache);
        }
        return _cache;
    }
}