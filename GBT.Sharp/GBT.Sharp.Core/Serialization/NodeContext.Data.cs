using GBT.Exceptions;
using GBT.Nodes;
using MessagePack;

namespace GBT;

public partial class NodeContext {
    [MessagePackObject(true)]
    public record struct Data(string NodeID, NodeState State) {
        public static Data From(NodeContext context) {
            return new(context.Node.ID, context.State);
        }
        public static NodeContext Load(Data data, IDictionary<string, GBTNode> loadedNodes) {
            if (loadedNodes.TryGetValue(data.NodeID, out GBTNode? node)) {
                NodeContext context = node.Context;
                context.ReadSavedData(data);
                return context;
            } else {
                throw new NodeNotFoundException(data.NodeID, "failed loading node context");
            }
        }
    }
}