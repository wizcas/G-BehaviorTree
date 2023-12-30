using MessagePack;
using System.Buffers;

namespace GBT.Sharp.Core;

public partial class TreeRuntime {
    /// <summary>
    /// Persistable data for the TreeRuntime.
    /// </summary>
    [MessagePackObject(true)]
    public record struct Data(string TreeID, string? RunningNodeID) {
        public NodeContext.Data[] NodeContexts { get; set; } = Array.Empty<NodeContext.Data>();
        public static Data From(TreeRuntime runtime) {
            return new(runtime.Tree.ID, runtime.RunningNode?.ID) {
                NodeContexts = runtime.NodeContexts.Values.Select(NodeContext.Data.From).ToArray()
            };
        }
    }
}