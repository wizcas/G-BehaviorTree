using GBT.Nodes;
using Xunit.Abstractions;

namespace GBT.Sharp.Core.Tests;
public class SequenceNodeTests {
    private readonly SequenceNode _root;
    private readonly BehaviorTree _tree;
    private readonly ITestOutputHelper _output;
    private readonly CallbackNode _child1;
    private readonly CallbackNode _child2;

    public SequenceNodeTests(ITestOutputHelper output) {
        _output = output;
        // TreeLogger.WriteLog = output.WriteLine;

        _root = new("test sequence");
        _child1 = new CallbackNode("child 1");
        _child2 = new CallbackNode("child 2");
        _root.AddChild(_child1);
        _root.AddChild(_child2);

        _tree = new();
        _tree.SetRootNode(_root);
    }

    private void AssertRootExitTimes(int times) {
        Assert.Equal(times,
            _tree.Runtime.Trace.Passes.FirstOrDefault()?.FootprintsByNodes[_root.ID]
            .Where(footprint => footprint.Content == "exit")
            .Count());
    }

    [Fact]
    public void ShouldSuccessOnAllChildrenSuccess() {
        _child1.OnTick = (node) => node.State = NodeState.Success;
        _child2.OnTick = (node) => node.State = NodeState.Success;
        _root.Tick();
        Assert.Equal(NodeState.Running, _root.State);
        Assert.Equal(_child2, _root.Context.CurrentChild);
        _root.Tick();
        Assert.Equal(NodeState.Success, _root.State);
        Assert.Null(_root.Context.CurrentChild);
        AssertRootExitTimes(1);
    }
    [Fact]
    public void ShouldFailWhenAnyChildFails() {
        _child1.OnTick = (node) => node.State = NodeState.Failure;
        _child2.OnTick = (node) => node.State = NodeState.Success;
        _root.Tick();
        Assert.Equal(NodeState.Failure, _root.State);
        Assert.Null(_root.Context.CurrentChild);
        AssertRootExitTimes(1);
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
        // The root node never exists because child 1 is still running
        AssertRootExitTimes(0);
    }
    [Fact]
    public void ShouldSkipDisabledNode() {
        _child1.IsDisabled = true;
        _child2.OnTick = (node) => node.State = NodeState.Success;
        _root.Tick();
        Assert.Equal(NodeState.Success, _root.State);
        Assert.Null(_root.Context.CurrentChild);
        Assert.Equivalent(new[] { _child2.ID, "<tree>" },
                          _tree.Runtime.Trace.Passes.FirstOrDefault()?.FootprintsByNodes.Keys);
        AssertRootExitTimes(1);
    }
}