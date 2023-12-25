using GBT.Sharp.Core.Nodes;

namespace GBT.Sharp.Core.Tests;

public class BaseNodeTests {
    public class TestBaseNode : BaseNode {
        public TestBaseNode(string id, string name) : base(id, name) { }

        public override void DoTick() { }
    }

    private TestBaseNode _node;
    private BehaviorTree _tree;
    public BaseNodeTests() {
        _node = new("TEST", "test node");
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
}