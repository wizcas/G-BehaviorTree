using GBT.Sharp.Core.Nodes;
using Moq;

namespace GBT.Sharp.Core.Tests;

public class TestNode {
    public Mock<BaseNode> Mock { get; init; }
    public BaseNode Node => Mock.Object;
    public TestNode(string id, string name) {
        Mock = new(id, name);
    }

    public void MockNextState(NodeState nextState) {
        Mock.Setup(node => node.DoTick()).Callback(() => Node.State = nextState);
    }
}

