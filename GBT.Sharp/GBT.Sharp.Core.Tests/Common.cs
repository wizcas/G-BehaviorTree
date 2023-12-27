using GBT.Sharp.Core.Nodes;
using Moq;

namespace GBT.Sharp.Core.Tests;

public class TestNode {
    public TestNode(string id, string name) {
        Mock.Setup(node => node.ID).Returns(id);
        Mock.Setup(node => node.Name).Returns(name);
    }
    public TestNode() : this("TEST", "test node") { }

    public Mock<INode> Mock { get; init; } = new();
    public INode Node => Mock.Object;


    public void MockNextState(NodeState nextState, Action? callback = null) {
        Mock.SetupProperty(node => node.State);
        Mock.Setup(node => node.Tick()).Callback(() => {
            Node.State = nextState;
            callback?.Invoke();
        });
    }
}

public class CounterNode(string id, string name) : BaseNode(id, name) {
    public int Count { get; private set; }
    public override void Initialize() {
        Count = 0;
    }
    protected override void DoTick() {
        Count++;
    }
}