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
        Assert.Equal(_tree.Context, _node.Context);
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
        _node.Context = null;
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
    [Fact]
    public void ShouldSaveLoadByHierarchy() {
        var root = new SequenceNode("root");
        var child1 = new CallbackNode("child 1") { Parent = root };
        var child2 = new CallbackNode("child 2") { Parent = root };
        List<Node.SavedData> saves = [];
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