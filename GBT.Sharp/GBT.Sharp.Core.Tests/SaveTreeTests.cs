using GBT.Nodes;
using MessagePack;
using System.Buffers;
using Xunit.Abstractions;

namespace GBT.Sharp.Core.Tests;

public class SaveTreeTests(ITestOutputHelper output) {
    [Fact]
    public void ShouldSaveLoadTree() {
        BehaviorTree tree = new();
        tree.SetRootNode(
            new RepeaterNode("rep") { Times = 8 }
            .AddChild(
                new SequenceNode("seq").AddChildren(
                    new SucceederNode("suc").AddChild(new CallbackNode("c1")),
                    new CallbackNode("c2"),
                    new RepeatUntilFailureNode("ruf").AddChild(
                        new SelectorNode("sel").AddChildren(
                            new CallbackNode("c3"),
                            new CallbackNode("c4"))
                    )
                )
            )
        );
        ArrayBufferWriter<byte> buffer = new();
        tree.Save(buffer);
        ReadOnlyMemory<byte> bin = buffer.WrittenMemory;
        output.WriteLine("bin size: {0}", bin.Length);
        output.WriteLine(MessagePackSerializer.ConvertToJson(bin));


        BehaviorTree tree2 = new();
        tree2.Load(bin);

        Assert.Equal(tree.ID, tree2.ID);
        Assert.Equivalent(
            tree.Flatten().Select(node => node.ID),
            tree2.Flatten().Select(node => node.ID));
        // Test extra data
        Assert.Equal(8, tree2.Flatten()
                            .FirstOrDefault(n => n.Name == "rep")?
                            .Cast<RepeaterNode>().Times);
    }

}