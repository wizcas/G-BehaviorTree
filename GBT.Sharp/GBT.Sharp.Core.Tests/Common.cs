using GBT.Sharp.Core.Nodes;
using Moq;

namespace GBT.Sharp.Core.Tests;

public class TestNode(string id, string name) {
    public Mock<BaseNode> Mock { get; init; } = new(id, name);
    public BaseNode Node => Mock.Object;

    public void MockNextState(NodeState nextState) {
        Mock.Setup(node => node.DoTick()).Callback(() => Node.State = nextState);
    }
}
