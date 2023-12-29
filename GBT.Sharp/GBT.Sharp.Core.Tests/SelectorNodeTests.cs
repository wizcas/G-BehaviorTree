using GBT.Sharp.Core.Nodes;
using Xunit.Abstractions;

namespace GBT.Sharp.Core.Tests;

public class SelectorNodeTests {
    private readonly SelectorNode _root;
    private readonly BehaviorTree _tree;
    private readonly ITestOutputHelper _output;
    private readonly CallbackNode _child1;
    private readonly CallbackNode _child2;

    public SelectorNodeTests(ITestOutputHelper output) {
        _output = output;
        // TreeLogger.WriteLog = output.WriteLine;

        _root = new("test selector");
        _child1 = new CallbackNode("child 1");
        _child2 = new CallbackNode("child 2");
        _root.AddChild(_child1);
        _root.AddChild(_child2);

        _tree = new();
        _tree.SetRootNode(_root);
    }

    [Fact]
    public void ShouldSuccessWhenAnyChildSucceeds() {
        _child1.OnTick = (node) => node.State = NodeState.Failure;
        _child2.OnTick = (node) => node.State = NodeState.Success;
        _root.Tick();
        Assert.Equal(NodeState.Running, _root.State);
        Assert.Equal(_child2, _root.Context.CurrentChild);
        _root.Tick();
        Assert.Equal(NodeState.Success, _root.State);
        Assert.Null(_root.Context.CurrentChild);
    }
    [Fact]
    public void ShouldFailIfAllChildrenFail() {
        _child1.OnTick = (node) => node.State = NodeState.Failure;
        _child2.OnTick = (node) => node.State = NodeState.Failure;
        _root.Tick();
        Assert.Equal(NodeState.Running, _root.State);
        Assert.Equal(_child2, _root.Context.CurrentChild);
        _root.Tick();
        Assert.Equal(NodeState.Failure, _root.State);
        Assert.Null(_root.Context.CurrentChild);
    }
    [Fact]
    public void ShouldKeepRunningIfChildIsRunning() {
        _child1.OnTick = (node) => node.State = NodeState.Running;
        _child2.OnTick = (node) => node.State = NodeState.Success;
        for (var i = 0; i < 10; i++) {
            _root.Tick();
            Assert.Equal(NodeState.Running, _root.State);
            Assert.Equal(_child1, _root.Context.CurrentChild);
        }
    }
    [Fact]
    public void ShouldSkipDisabledNode() {
        _child1.IsDisabled = true;
        _child2.OnTick = (node) => node.State = NodeState.Success;
        _root.Tick();
        Assert.Equal(NodeState.Success, _root.State);
        Assert.Null(_root.Context.CurrentChild);
        Assert.Equivalent(
            new[] { _child2.ID, "<tree>" },
            _tree.Context.Trace.Passes.FirstOrDefault()?.FootprintsByNodes.Keys);
    }
}