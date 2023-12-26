using GBT.Sharp.Core.Nodes;
using Xunit.Abstractions;

namespace GBT.Sharp.Core.Tests;
public class SequenceNodeTests {
    private readonly SequenceNode _node;
    private readonly BehaviorTree _tree;
    private readonly ITestOutputHelper _output;
    private readonly TestNode _child1;
    private readonly TestNode _child2;

    public SequenceNodeTests(ITestOutputHelper output) {
        _output = output;
        // TreeLogger.WriteLog = output.WriteLine;

        _node = new("TEST SEQUENCE NODE", "test sequence");
        _child1 = new TestNode("CHILD 1", "child 1");
        _child2 = new TestNode("CHILD 2", "child 2");
        _node.AttachChild(_child1.Node);
        _node.AttachChild(_child2.Node);

        _tree = new();
        _tree.SetRootNode(_node);
    }

    [Fact]
    public void ShouldSuccessOnAllChildrenSuccess() {
        _child1.MockNextState(NodeState.Success);
        _child2.MockNextState(NodeState.Success);
        _node.Initialize();
        Assert.Equal(_child1.Node, _node.CurrentChild);
        _node.Tick();
        Assert.Equal(_child2.Node, _node.CurrentChild);
        _node.Tick();
        Assert.Equal(NodeState.Success, _node.State);
        Assert.Null(_node.CurrentChild);
    }
    [Fact]
    public void ShouldFailWhenAnyChildFails() {
        _child1.MockNextState(NodeState.Failure);
        _child2.MockNextState(NodeState.Success);
        _node.Initialize();
        Assert.Equal(_child1.Node, _node.CurrentChild);
        _node.Tick();
        Assert.Equal(NodeState.Failure, _node.State);
        Assert.Equal(_child1.Node, _node.CurrentChild);
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