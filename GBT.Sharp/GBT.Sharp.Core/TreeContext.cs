using GBT.Sharp.Core.Diagnostics;
using GBT.Sharp.Core.Nodes;
using System.Collections;

namespace GBT.Sharp.Core;

public interface ITreeContext {
    BehaviorTree Tree { get; init; }
    Trace Trace { get; }

}

public class TreeContext : ITreeContext {
    public BehaviorTree Tree { get; init; }

    public Trace Trace { get; } = new();


    public TreeContext(BehaviorTree tree) {
        Tree = tree;
    }
}