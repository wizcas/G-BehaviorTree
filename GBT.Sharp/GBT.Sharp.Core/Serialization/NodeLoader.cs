using GBT.Sharp.Core.Nodes;

namespace GBT.Sharp.Core.Serialization;

public class NodeLoader {
    private readonly Dictionary<string, GBTNode> _cache = new();

    public void Reset() {
        _cache.Clear();
    }

    public TNode Load<TNode>(GBTNode.Data data) where TNode : GBTNode {
        return data.LoadNode<TNode>(_cache);
    }
    public GBTNode Load(GBTNode.Data data) {
        return data.LoadNode(_cache);
    }
    public Dictionary<string, GBTNode> LoadAll(IEnumerable<GBTNode.Data> datas) {
        foreach (GBTNode.Data data in datas) {
            data.LoadNode(_cache);
        }
        return _cache;
    }
}