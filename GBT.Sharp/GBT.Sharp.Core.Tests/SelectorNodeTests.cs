using GBT.Sharp.Core.Nodes;
using Xunit.Abstractions;

namespace GBT.Sharp.Core.Tests;

public class SelectorNodeTests {
    private readonly SelectorNode _node;
    private readonly BehaviorTree _tree;
    private readonly ITestOutputHelper _output;
    private readonly CallbackNode _child1;
    private readonly CallbackNode _child2;

    public SelectorNodeTests(ITestOutputHelper output) {
        _output = output;
        // TreeLogger.WriteLog = output.WriteLine;

        _node = new("TEST SELECTOR NODE", "test selector");
        _child1 = new CallbackNode("CHILD 1", "child 1");
        _child2 = new CallbackNode("CHILD 2", "child 2");
        _node.AddChild(_child1);
        _node.AddChild(_child2);

        _tree = new();
        _tree.SetRootNode(_node);
    }

    [Fact]
    public void ShouldSuccessWhenAnyChildSucceeds() {
        _child1.OnTick = (node) => node.State = NodeState.Failure;
        _child2.OnTick = (node) => node.State = NodeState.Success;
        _node.Initialize();
        Assert.Equal(_child1, _node.CurrentChild);
        _node.Tick();
        Assert.Equal(_child2, _node.CurrentChild);
        _node.Tick();
        Assert.Equal(NodeState.Success, _node.State);
        Assert.Equal(_child2, _node.CurrentChild);
    }
    [Fact]
    public void ShouldFailIfAllChildrenFail() {
        _child1.OnTick = (node) => node.State = NodeState.Failure;
        _child2.OnTick = (node) => node.State = NodeState.Failure;
        _node.Initialize();
        Assert.Equal(_child1, _node.CurrentChild);
        _node.Tick();
        Assert.Equal(_child2, _node.CurrentChild);
        _node.Tick();
        Assert.Equal(NodeState.Failure, _node.State);
        Assert.Null(_node.CurrentChild);
    }
    [Fact]
    public void ShouldKeepRunningIfChildIsRunning() {
        _child1.OnTick = (node) => node.State = NodeState.Running;
        _child2.OnTick = (node) => node.State = NodeState.Success;
        _node.Initialize();
        Assert.Equal(_child1, _node.CurrentChild);
        for (var i = 0; i < 10; i++) {
            _node.Tick();
            Assert.Equal(NodeState.Running, _node.State);
            Assert.Equal(_child1, _node.CurrentChild);
        }
    }
}