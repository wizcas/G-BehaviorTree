using GBT.Nodes;

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

        tree.Runtime.RunningNode = tree.FindNodeByName("cb1");
        BehaviorTree tree2 = new();
        tree2.Load(tree.Save());
        var bin = tree.Runtime.Save();
        var runtime2 = new TreeRuntime(tree2);
        runtime2.Load(bin);
        Assert.Equivalent(tree.Runtime.RunningNode?.ID, runtime2.RunningNode?.ID);
        Assert.Equivalent(tree.Runtime.NodeContexts.Keys, runtime2.NodeContexts.Keys);
        Assert.Equal(tree.Runtime.NodeContexts.Values.Select(v => (v.Node.ID, v.State)),
                     runtime2.NodeContexts.Values.Select(v => (v.Node.ID, v.State)));
    }
}