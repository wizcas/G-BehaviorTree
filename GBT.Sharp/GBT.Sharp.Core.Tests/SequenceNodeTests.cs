using GBT.Sharp.Core.Nodes;
using Xunit.Abstractions;

namespace GBT.Sharp.Core.Tests;
public class SequenceNodeTests {
    private readonly SequenceNode _node;
    private readonly BehaviorTree _tree;
    private readonly ITestOutputHelper _output;
    private readonly CallbackNode _child1;
    private readonly CallbackNode _child2;

    public SequenceNodeTests(ITestOutputHelper output) {
        _output = output;
        // TreeLogger.WriteLog = output.WriteLine;

        _node = new("test sequence");
        _child1 = new CallbackNode("child 1");
        _child2 = new CallbackNode("child 2");
        _node.AddChild(_child1);
        _node.AddChild(_child2);

        _tree = new();
        _tree.SetRootNode(_node);
    }

    [Fact]
    public void ShouldSuccessOnAllChildrenSuccess() {
        _child1.OnTick = (node) => node.State = NodeState.Success;
        _child2.OnTick = (node) => node.State = NodeState.Success;
        _node.Tick();
        Assert.Equal(NodeState.Running, _node.State);
        Assert.Equal(_child2, _node.NodeContext?.CurrentChild);
        _node.Tick();
        Assert.Equal(NodeState.Success, _node.State);
        Assert.Null(_node.NodeContext?.CurrentChild);
    }
    [Fact]
    public void ShouldFailWhenAnyChildFails() {
        _child1.OnTick = (node) => node.State = NodeState.Failure;
        _child2.OnTick = (node) => node.State = NodeState.Success;
        _node.Tick();
        Assert.Equal(NodeState.Failure, _node.State);
        Assert.Null(_node.NodeContext?.CurrentChild);
    }
    [Fact]
    public void ShouldKeepRunningIfChildIsRunning() {
        _child1.OnTick = (node) => node.State = NodeState.Running;
        _child2.OnTick = (node) => node.State = NodeState.Success;
        for (var i = 0; i < 10; i++) {
            _node.Tick();
            Assert.Equal(NodeState.Running, _node.State);
            Assert.Equal(_child1, _node.NodeContext?.CurrentChild);
        }
    }
    [Fact]
    public void ShouldSkipDisabledNode() {
        _child1.IsDisabled = true;
        _child2.OnTick = (node) => node.State = NodeState.Success;
        _node.Tick();
        Assert.Equal(NodeState.Success, _node.State);
        Assert.Null(_node.NodeContext?.CurrentChild);
        Assert.Equivalent(new[] { _child2.ID, "<tree>" },
                          _tree.Context.Trace.Passes.FirstOrDefault()?.FootprintsByNodes.Keys);
    }
}