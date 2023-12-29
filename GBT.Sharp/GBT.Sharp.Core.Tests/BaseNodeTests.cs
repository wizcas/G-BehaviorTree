using GBT.Sharp.Core.Nodes;
using Xunit.Abstractions;

namespace GBT.Sharp.Core.Tests;

internal class CallbackCounters {
    public int Initialize { get; set; }
    public int CleanUp { get; set; }

    internal CallbackCounters(CallbackNode node) {
        node.OnInitialize = (node) => Initialize++;
        node.OnCleanUp = (node) => CleanUp++;
    }
}
public class BaseNodeTests {
    private readonly ITestOutputHelper _output;
    private readonly CallbackNode _node;
    private readonly BehaviorTree _tree;
    private readonly CallbackCounters _callbackCounters;
    public BaseNodeTests(ITestOutputHelper output) {
        _output = output;
        _node = new("test node");
        _tree = new BehaviorTree();
        _tree.SetRootNode(_node);
        _callbackCounters = new(_node); ;
    }
    [Fact]
    public void ShouldHaveDefaultState() {
        Assert.Equal("test node", _node.Name);
        Assert.Equal(NodeState.Unvisited, _node.State);
        Assert.False(_node.IsDisabled);
        Assert.Equal(_tree.Runtime, _node.Runtime);
        Assert.Null(_node.Parent);
    }
    [Fact]
    public void ShouldNotRunIfDisabled() {
        _node.IsDisabled = true;
        _node.Tick();
        Assert.Equal(NodeState.Unvisited, _node.State);
    }
    [Fact]
    public void ShouldNotRunIfNoContext() {
        _node.Runtime = null;
        _node.Tick();
        Assert.Equal(NodeState.Unvisited, _node.State);
    }
    [Fact]
    public void ShouldInitializeAndRunOnTick() {
        _node.Tick();
        Assert.Equal(NodeState.Running, _node.State);
        Assert.Equal(1, _callbackCounters.Initialize);
        Assert.Equal(0, _callbackCounters.CleanUp);
    }
    [Theory]
    [InlineData(NodeState.Success)]
    [InlineData(NodeState.Failure)]
    public void ShouldExitIfOnEndStates(NodeState nextState) {
        _node.OnTick = (node) => node.State = nextState;
        _node.Tick();
        Assert.Equal(nextState, _node.State);
        Assert.Equal(1, _callbackCounters.Initialize);
        Assert.Equal(1, _callbackCounters.CleanUp);
    }
}