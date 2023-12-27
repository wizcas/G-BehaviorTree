using GBT.Sharp.Core.Nodes;
using Moq;
using Xunit.Abstractions;

namespace GBT.Sharp.Core.Tests;

public class BaseNodeTests {
    private readonly Mock<CallbackNode> _t;
    private readonly BehaviorTree _tree;
    private readonly ITestOutputHelper _output;

    public BaseNodeTests(ITestOutputHelper output) {
        _output = output;
        _t = new("test node") { CallBase = true };
        _tree = new BehaviorTree();
        _tree.SetRootNode(_t.Object);
    }
    [Fact]
    public void ShouldHaveDefaultState() {
        Assert.Equal("test node", _t.Object.Name);
        Assert.Equal(NodeState.Unvisited, _t.Object.State);
        Assert.False(_t.Object.IsDisabled);
        Assert.Equal(_tree.Context, _t.Object.Context);
        Assert.Null(_t.Object.Parent);
    }
    [Fact]
    public void ShouldNotRunIfNotEnabled() {
        _t.Object.IsDisabled = true;
        _t.Object.Tick();
        Assert.Equal(NodeState.Unvisited, _t.Object.State);
    }
    [Fact]
    public void ShouldNotRunIfNoContext() {
        _t.Object.Context = null;
        _t.Object.Tick();
        Assert.Equal(NodeState.Unvisited, _t.Object.State);
    }
    [Fact]
    public void ShouldInitializeAndRunningAfterTick() {
        _t.Object.Tick();
        Assert.Equal(NodeState.Running, _t.Object.State);
        _t.Verify(node => node.Initialize(), Times.Once());
        _t.Verify(node => node.CleanUp(), Times.Never());
    }
    [Theory]
    [InlineData(NodeState.Success)]
    [InlineData(NodeState.Failure)]
    public void ShouldExitIfOnEndStates(NodeState nextState) {
        _t.Object.OnTick = (node) => node.State = nextState;
        _t.Object.Tick();
        Assert.Equal(nextState, _t.Object.State);
        _t.Verify(node => node.CleanUp(), Times.Once());
    }
}