using GBT.Sharp.Core.Nodes;

namespace GBT.Sharp.Core.Tests;

public class BehaviorTreeTests {
    private readonly BehaviorTree _tree;

    public BehaviorTreeTests() {
        _tree = new();
    }

    [Fact]
    public void ShouldRunRootNode() {
        var rootNode = new CallbackNode("test node");
        var count = 0;
        rootNode.OnTick += (node) => count++;
        _tree.SetRootNode(rootNode);
        _tree.Tick();
        Assert.Equal(1, count);
        Assert.Equal(rootNode, _tree.Runtime.RunningNode);
    }
    [Fact]
    public void ShouldCacheRunningNode() {
        var rootNode = new SequenceNode("sequence node");
        var child1 = new CallbackNode("child 1");
        var child2 = new CallbackNode("child 2");
        rootNode.AddChild(child1);
        rootNode.AddChild(child2);
        _tree.SetRootNode(rootNode);

        for (var i = 0; i < 3; i++) {
            _tree.Tick();
            Assert.Equal(child1, _tree.Runtime.RunningNode);
        }

        child1.OnTick = (node) => node.State = NodeState.Success;
        _tree.Tick();
        Assert.Equal(rootNode, _tree.Runtime.RunningNode);

        for (var i = 0; i < 3; i++) {
            _tree.Tick();
            Assert.Equal(child2, _tree.Runtime.RunningNode);
        }
        child2.OnTick = (node) => node.State = NodeState.Success;
        _tree.Tick();
        // Since the last child is done, the tree exits running states RECURSIVELY
        // to the root node, leaving running nodes in a null state.
        Assert.Null(_tree.Runtime.RunningNode);
    }
    [Fact]
    public void ShouldInterruptRunningState() {
        var rootNode = new SequenceNode("sequence node");
        var child = new CallbackNode("child 1");
        rootNode.AddChild(child);
        _tree.SetRootNode(rootNode);

        var isChildCleanedUp = false;
        child.OnCleanUp = (node) => isChildCleanedUp = true;

        _tree.Tick();
        Assert.Equal(child, _tree.Runtime.RunningNode);
        _tree.Interrupt();
        Assert.Null(_tree.Runtime.RunningNode);
        Assert.Equal(NodeState.Unvisited, rootNode.State);
        Assert.Equal(NodeState.Unvisited, child.State);
        Assert.True(isChildCleanedUp);
    }
}