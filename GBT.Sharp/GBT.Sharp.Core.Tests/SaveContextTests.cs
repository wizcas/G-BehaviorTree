using GBT.Sharp.Core.Nodes;

namespace GBT.Sharp.Core.Tests;

public class SaveContextTests {
    [Fact]
    public void ShouldSaveLoadContext() {
        BehaviorTree tree = new();
        tree.SetRootNode(
            new SelectorNode("sel") { State = NodeState.Running }.AddChildren(
                new CallbackNode("cb1") { State = NodeState.Success },
                new CallbackNode("cb2") { State = NodeState.Failure }
                )
            );

        var bin = tree.Runtime.Save();
        var runtime2 = new TreeRuntime(tree);
        runtime2.Load(bin);
    }
}