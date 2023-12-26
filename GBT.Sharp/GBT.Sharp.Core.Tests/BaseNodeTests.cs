using GBT.Sharp.Core.Nodes;
using Moq;

namespace GBT.Sharp.Core.Tests;

public class BaseNodeTests {
    private TestNode _t;
    private BehaviorTree _tree;
    public BaseNodeTests() {
        _t = new("TEST", "test node");
        _tree = new BehaviorTree();
        _tree.SetRootNode(_t.Node);
    }
    [Fact]
    public void Should_Have_Default_State() {
        Assert.Equal("TEST", _t.Node.ID);
        Assert.Equal("test node", _t.Node.Name);
        Assert.Equal(NodeState.Unvisited, _t.Node.State);
        Assert.False(_t.Node.IsDisabled);
        Assert.Equal(_tree.Context, _t.Node.Context);
        Assert.Null(_t.Node.Parent);
    }
    [Fact]
    public void Should_Not_Run_If_Not_Enabled() {
        _t.Node.IsDisabled = true;
        _t.Node.Tick();
        Assert.Equal(NodeState.Unvisited, _t.Node.State);
    }
    [Fact]
    public void Should_Not_Run_If_No_Context() {
        _t.Node.Context = null;
        _t.Node.Tick();
        Assert.Equal(NodeState.Unvisited, _t.Node.State);
    }
    [Fact]
    public void Should_Initialize_And_Running_After_Tick() {
        _t.Node.Tick();
        Assert.Equal(NodeState.Running, _t.Node.State);
        _t.Mock.Verify(node => node.Initialize(), Times.Once());
        _t.Mock.Verify(node => node.Exit(), Times.Never());
    }
    [Theory]
    [InlineData(NodeState.Success)]
    [InlineData(NodeState.Failure)]
    public void Should_Exit_If_On_End_States(NodeState nextState) {
        _t.MockNextState(nextState);
        _t.Node.Tick();
        Assert.Equal(nextState, _t.Node.State);
        _t.Mock.Verify(node => node.Exit(), Times.Once());
    }
}