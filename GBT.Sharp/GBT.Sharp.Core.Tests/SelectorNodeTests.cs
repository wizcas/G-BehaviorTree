using GBT.Sharp.Core.Nodes;
using Xunit.Abstractions;

namespace GBT.Sharp.Core.Tests;

public class SelectorNodeTests {
    private readonly SelectorNode _node;
    private readonly BehaviorTree _tree;
    private readonly ITestOutputHelper _output;
    private readonly TestNode _child1;
    private readonly TestNode _child2;

    public SelectorNodeTests(ITestOutputHelper output) {
        _node = new("TEST SELECTOR NODE", "test selector");
        _child1 = new TestNode("CHILD 1", "child 1");
        _child2 = new TestNode("CHILD 2", "child 2");
        _node.AttachChild(_child1.Node);
        _node.AttachChild(_child2.Node);

        _tree = new();
        _tree.SetRootNode(_node);
        _output = output;
    }

    [Fact]
    public void ShouldSuccessWhenAnyChildSucceeds() {
        _child1.MockNextState(NodeState.Failure);
        _child2.MockNextState(NodeState.Success);
        _node.Initialize();
        Assert.Equal(_child1.Node, _node.CurrentChild);
        _node.Tick();
        Assert.Equal(_child2.Node, _node.CurrentChild);
        _node.Tick();
        Assert.Equal(NodeState.Success, _node.State);
        Assert.Equal(_child2.Node, _node.CurrentChild);
    }
    [Fact]
    public void ShouldFailIfAllChildrenFail() {
        _child1.MockNextState(NodeState.Failure);
        _child2.MockNextState(NodeState.Failure);
        _node.Initialize();
        Assert.Equal(_child1.Node, _node.CurrentChild);
        _node.Tick();
        Assert.Equal(_child2.Node, _node.CurrentChild);
        _node.Tick();
        Assert.Equal(NodeState.Failure, _node.State);
        Assert.Null(_node.CurrentChild);
    }
    [Fact]
    public void ShouldKeepRunningIfChildIsRunning() {
        _child1.MockNextState(NodeState.Running);
        _child2.MockNextState(NodeState.Success);
        _node.Initialize();
        Assert.Equal(_child1.Node, _node.CurrentChild);
        for (var i = 0; i < 10; i++) {
            _node.Tick();
            Assert.Equal(NodeState.Running, _node.State);
            Assert.Equal(_child1.Node, _node.CurrentChild);
        }
    }
}