using GBT.Sharp.Core.Nodes;
using Moq;

namespace GBT.Sharp.Core.Tests;

public class BaseNodeTests {
    public class TestBaseNode : BaseNode {
        public NodeState? nextState;
        public TestBaseNode(string id, string name) : base(id, name) { }

        public override void DoTick() { }
        public void SetTestState(NodeState state) {
            SetState(state);
        }
    }

    private Mock<TestBaseNode> _mock;
    private TestBaseNode _node => _mock.Object;
    private BehaviorTree _tree;
    public BaseNodeTests() {
        _mock = new("TEST", "test node");
        _tree = new BehaviorTree();
        _tree.SetRootNode(_node);
    }
    [Fact]
    public void Should_Have_Default_State() {
        Assert.Equal("TEST", _node.ID);
        Assert.Equal("test node", _node.Name);
        Assert.Equal(NodeState.Unvisited, _node.State);
        Assert.False(_node.IsDisabled);
        Assert.Equal(_tree.Context, _node.Context);
        Assert.Null(_node.Parent);
    }
    [Fact]
    public void Should_Not_Run_If_Not_Enabled() {
        _node.IsDisabled = true;
        _node.Tick();
        Assert.Equal(NodeState.Unvisited, _node.State);
    }
    [Fact]
    public void Should_Not_Run_If_No_Context() {
        _node.Context = null;
        _node.Tick();
        Assert.Equal(NodeState.Unvisited, _node.State);
    }
    [Fact]
    public void Should_Initialize_And_Running_After_Tick() {
        _node.Tick();
        Assert.Equal(NodeState.Running, _node.State);
        _mock.Verify(node => node.Initialize(), Times.Once());
        _mock.Verify(node => node.Exit(), Times.Never());
    }
    [Theory]
    [InlineData(NodeState.Success)]
    [InlineData(NodeState.Failure)]
    public void Should_Exit_If_On_End_States(NodeState nextState) {
        _node.nextState = nextState;
        _mock.Setup(node => node.DoTick()).Callback(() => _mock.Object.SetTestState(nextState));
        _node.Tick();
        Assert.Equal(nextState, _node.State);
        _mock.Verify(node => node.Exit(), Times.Once());
    }
}