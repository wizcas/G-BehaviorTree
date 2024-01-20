
using GBT.Nodes;
using Xunit.Abstractions;

namespace GBT.Sharp.Core.Tests;
public class ListCompositeTests {
    private readonly SequenceNode _root;
    private readonly BehaviorTree _tree;
    private readonly ITestOutputHelper _output;
    private readonly CallbackNode _child1;
    private readonly CallbackNode _child2;

    public ListCompositeTests(ITestOutputHelper output) {
        _output = output;

        _root = new("test sequence");
        _child1 = new CallbackNode("child 1");
        _child2 = new CallbackNode("child 2");
        _root.AddChild(_child1);
        _root.AddChild(_child2);

        _tree = new();
        _tree.SetRootNode(_root);
    }

    [Fact]
    public void ShouldMoveChildAsInsert() {
        _root.MoveChild(_child2, 0);
        Assert.Equivalent(new[] { _child2.ID, _child1.ID }, _root.Children.Select(c => c.ID));
        var child3 = new CallbackNode("child 3");
        _root.MoveChild(child3, 1);
        Assert.Equivalent(new[] { _child2.ID, child3.ID, _child1.ID }, _root.Children.Select(c => c.ID));
    }
    [Fact]
    public void ShouldMoveChildToLastForLargeIndex() {
        _root.MoveChild(_child1, 2);
        Assert.Equivalent(new[] { _child2.ID, _child1.ID }, _root.Children.Select(c => c.ID));
        var child3 = new CallbackNode("child 3");
        _root.MoveChild(child3, 999);
        Assert.Equivalent(new[] { _child2.ID, _child1.ID, child3.ID }, _root.Children.Select(c => c.ID));
    }
    [Fact]
    public void ShouldMoveChildForNegativeIndex() {
        var child3 = new CallbackNode("child 3");
        var child4 = new CallbackNode("child 4");
        _root.AddChild(child3);
        _root.AddChild(child4);
        _root.MoveChild(_child1, -1);
        Assert.Equivalent(new[] { _child2.ID, child3.ID, child4.ID, _child1.ID }, _root.Children.Select(c => c.ID));
        _root.MoveChild(_child2, -2);
        Assert.Equivalent(new[] { child3.ID, child4.ID, _child2.ID, _child1.ID }, _root.Children.Select(c => c.ID));
    }
    [Fact]
    public void ShouldPassDownTheOwnerTree() {
        var tree = new BehaviorTree();
        tree.SetRootNode(_root);
        foreach (GBTNode node in (GBTNode[])[_root, _child1, _child2]) {
            Assert.Equal(tree, node.Tree);
        }
    }
    [Fact]
    public void ShouldUpdateRootIfCurrentRootHasAParent() {
        var tree = new BehaviorTree();
        tree.SetRootNode(_root);
        var newRoot = new SequenceNode("new root");
        _root.Parent = newRoot;
        Assert.Equal(newRoot, tree.RootNode);
    }
}