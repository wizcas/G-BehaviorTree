using GBT.Nodes;
using MessagePack;

namespace GBT;

public partial class BehaviorTree {
    [MessagePackObject(true)]
    public record struct Data(
        string ID,
        GBTNode.Data[] Nodes,
        string? RootID);
}