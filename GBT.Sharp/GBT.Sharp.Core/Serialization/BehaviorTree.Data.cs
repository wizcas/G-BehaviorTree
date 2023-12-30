using GBT.Sharp.Core.Nodes;
using MessagePack;

namespace GBT.Sharp.Core;

public partial class BehaviorTree {
    [MessagePackObject(true)]
    public record struct Data(
        string ID,
        Node.Data[] Nodes,
        string? RootID);
}