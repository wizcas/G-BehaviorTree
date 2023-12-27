using GBT.Sharp.Core.Nodes;

namespace GBT.Sharp.Core.Tests;

public class DecoratorNodeTests {
    private readonly BehaviorTree _tree;

    public DecoratorNodeTests() {
        _tree = new BehaviorTree();
    }

    [Fact]
    public void ShouldInverterNodeInvertChildResult() {
        var node = new InverterNode("INVERTER", "inverter");
        _tree.SetRootNode(node);
        var child = new CallbackNode("CHILD", "child");
        node.AddChild(child);

        child.OnTick = (node) => node.State = NodeState.Success;
        node.Tick();
        Assert.Equal(NodeState.Failure, node.State);

        child.OnTick = (node) => node.State = NodeState.Failure;
        node.Tick();
        Assert.Equal(NodeState.Success, node.State);
    }
    [Fact]
    public void ShouldSucceederNodeAlwaysReturnSuccess() {
        var node = new SucceederNode("SUCCEEDER", "succeeder");
        _tree.SetRootNode(node);
        var child = new CallbackNode("CHILD", "child");
        node.AddChild(child);

        child.OnTick = (node) => node.State = NodeState.Success;
        node.Tick();
        Assert.Equal(NodeState.Success, node.State);

        child.OnTick = (node) => node.State = NodeState.Failure;
        node.Tick();
        Assert.Equal(NodeState.Success, node.State);
    }
    [Fact]
    public void ShouldRepeaterNodeRepeatChildInfinitelyWithTimesUnset() {
        var node = new RepeaterNode("REPEATER", "repeater");
        _tree.SetRootNode(node);
        var child = new CallbackNode("CHILD", "child");
        var count = 0;
        node.AddChild(child);
        child.OnTick = (node) => {
            node.State = NodeState.Success;
            count++;
        };

        for (var i = 0; i < 100; i++) {
            node.Tick();
            Assert.Equal(NodeState.Running, node.State);
            Assert.Equal(NodeState.Success, child.State);
        }
        Assert.Equal(100, count);
    }
    [Fact]
    public void ShouldRepeaterNodeRepeatChildWithGivenTimes() {
        var node = new RepeaterNode("REPEATER", "repeater") { Times = 10 };
        _tree.SetRootNode(node);
        var child = new CallbackNode("CHILD", "child");
        var count = 0;
        node.AddChild(child);
        child.OnTick = (node) => {
            node.State = NodeState.Failure;
            count++;
        };

        for (var i = 0; i < 10; i++) {
            node.Tick();
            Assert.Equal(i < 9 ? NodeState.Running : NodeState.Success, node.State);
        }
        Assert.Equal(10, count);

        count = 0;
        node.Times = 0;
        node.Tick();
        Assert.Equal(NodeState.Success, node.State);
        Assert.Equal(0, count);
    }
    [Fact]
    public void ShouldRepeatUntilFailureNodeRepeatChildUntilFailure() {
        var node = new RepeatUntilFailureNode("REPEAT_UNTIL_FAILURE", "repeat until failure");
        _tree.SetRootNode(node);
        var child = new CallbackNode("CHILD", "child");
        var count = 0;
        node.AddChild(child);
        child.OnTick = (node) => {
            // Since 5th call the child node fails and thus stops the repeating.
            node.State = ++count >= 5 ? NodeState.Failure : NodeState.Success;
        };

        for (var i = 0; i < 10; i++) {
            node.Tick();
            if (node.State == NodeState.Success) {
                break;
            }
            Assert.Equal(NodeState.Running, node.State);
        }
        Assert.Equal(NodeState.Success, node.State);
        Assert.Equal(5, count);
    }

}