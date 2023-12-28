using GBT.Sharp.Core.Diagnostics;
using GBT.Sharp.Core.Nodes;

namespace GBT.Sharp.Core.Tests;

public class TracingTests {
    private readonly BehaviorTree _tree;

    public TracingTests() {
        _tree = new();
    }

    [Fact]
    public void ShouldRecordPassByFootprints() {
        var root = new SequenceNode("root");
        var child1 = new CallbackNode("child 1") {
            OnTick = (node) => node.State = NodeState.Success,
            Parent = root,
        };
        var child2 = new CallbackNode("child 2") {
            OnTick = (node) => node.State = NodeState.Failure,
            Parent = root,
        };
        _tree.SetRootNode(root);
        do {
            _tree.Tick();
        } while (_tree.Context.RunningNode is not null);
        Trace trace = _tree.Context.Trace;
        Assert.Single(trace.Passes);
        Pass pass = trace.Passes.First();
        Assert.Equivalent(new string[] { root.ID, child1.ID, child2.ID, "<tree>" },
                     pass.FootprintsByNodes.Keys);
    }
}