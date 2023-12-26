using GBT.Sharp.Core.Nodes;
using Moq;
using Xunit.Abstractions;

namespace GBT.Sharp.Core.Tests;

public class BaseNodeTests {
    private readonly TestNode _t;
    private readonly BehaviorTree _tree;
    private readonly ITestOutputHelper _output;

    public BaseNodeTests(ITestOutputHelper output) {
        _output = output;
        // TreeLogger.WriteLog = output.WriteLine;

        _t = new("TEST", "test node");
        _tree = new BehaviorTree();
        _tree.SetRootNode(_t.Node);
    }
    [Fact]
    public void ShouldHaveDefaultState() {
        Assert.Equal("TEST", _t.Node.ID);
        Assert.Equal("test node", _t.Node.Name);
        Assert.Equal(NodeState.Unvisited, _t.Node.State);
        Assert.False(_t.Node.IsDisabled);
        Assert.Equal(_tree.Context, _t.Node.Context);
        Assert.Null(_t.Node.Parent);
    }
    [Fact]
    public void ShouldNotRunIfNotEnabled() {
        _t.Node.IsDisabled = true;
        _t.Node.Tick();
        Assert.Equal(NodeState.Unvisited, _t.Node.State);
    }
    [Fact]
    public void ShouldNotRunIfNoContext() {
        _t.Node.Context = null;
        _t.Node.Tick();
        Assert.Equal(NodeState.Unvisited, _t.Node.State);
    }
    [Fact]
    public void ShouldInitializeAndRunningAfterTick() {
        _t.Node.Tick();
        Assert.Equal(NodeState.Running, _t.Node.State);
        _t.Mock.Verify(node => node.Initialize(), Times.Once());
        _t.Mock.Verify(node => node.Exit(), Times.Never());
    }
    [Theory]
    [InlineData(NodeState.Success)]
    [InlineData(NodeState.Failure)]
    public void ShouldExitIfOnEndStates(NodeState nextState) {
        _t.MockNextState(nextState);
        _t.Node.Tick();
        Assert.Equal(nextState, _t.Node.State);
        _t.Mock.Verify(node => node.Exit(), Times.Once());
    }
}