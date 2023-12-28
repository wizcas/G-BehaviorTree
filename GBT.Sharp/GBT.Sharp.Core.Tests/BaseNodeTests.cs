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
    public void ShouldNotRunIfDisabled() {
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
    [Fact]
    public void ShouldSaveLoadByHierarchy() {
        var root = new SequenceNode("root");
        var child1 = new CallbackNode("child 1") { Parent = root };
        var child2 = new CallbackNode("child 2") { Parent = root };
        List<Node.SaveData> saves = [];
        // Save
        root.Save(saves);
        Assert.Equal(3, saves.Count);
        var index = 0;
        foreach (Node node in new Node[] { root, child1, child2 }) {
            Assert.Equivalent(new {
                NodeType = node.GetType(),
                node.ID,
                node.Name,
                node.IsDisabled,
            }, saves[index++]);
        }
        // Load
        Dictionary<string, Node> loadedNodes = [];
        Node[] results = saves.Select(save => save.LoadNode(loadedNodes)).ToArray();
        index = 0;
        foreach (Node node in new Node[] { root, child1, child2 }) {
            Node result = results[index++];
            Assert.Equivalent(new {
                node.ID,
                node.Name,
                node.IsDisabled,
            }, new {
                result.ID,
                result.Name,
                result.IsDisabled,
            });
        }
        Assert.Equivalent(
            root.Children.Select(c => c.ID),
            (results[0] as IParentNode)?.Children.Select(c => c.ID));
    }
}