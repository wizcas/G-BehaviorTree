using GBT.Sharp.Core.Nodes;
using GBT.Sharp.Core.Serialization;
using System.Collections;
using Xunit.Abstractions;

namespace GBT.Sharp.Core.Tests;

public class SaveNodeTests(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output ?? throw new ArgumentNullException(nameof(output));
    private readonly NodeLoader _loader = new();

    [Theory]
    [ClassData(typeof(NodeHierarchyGeneartor))]
    public void ShouldSaveLoadHierarchy(NodeHierarchyGeneartor.TestCase testCase) {
        GBTNode[] testedNodes = testCase.Nodes;
        // Save
        List<GBTNode.Data> saves = [];
        testCase.Root.Save(saves);
        Assert.Equal(testedNodes.Length, saves.Count);
        var index = 0;
        foreach (GBTNode node in testedNodes) {
            GBTNode.Data save = saves[index++];
            // Assert basic data
            Assert.Equivalent(new {
                NodeType = node.GetType(),
                node.ID,
                node.Name,
                node.IsDisabled,
            }, save);
            // Assert hierarchical data
            if (node.Parent is not null) {
                Assert.Equal(node.Parent.ID, save.ParentID);
            }
        }
        // Load
        GBTNode[] loads = saves.Select(_loader.Load).ToArray();
        index = 0;
        foreach (GBTNode node in testedNodes) {
            GBTNode load = loads[index++];
            // Assert basic data
            Assert.Equivalent(new {
                node.ID,
                node.Name,
                node.IsDisabled,
            }, new {
                load.ID,
                load.Name,
                load.IsDisabled,
            });
            // Assert hierarchical data
            if (node.Parent is not null) {
                output.WriteLine($"check parent on {load.Name}. Expecting: {node.Parent.Name}");
                Assert.Equal(node.Parent.ID, load.Parent?.ID);
            }
            if (node is IParentNode parent) {
                Assert.Equivalent(
                    parent.Children.Select(c => c.ID),
                    (load as IParentNode)?.Children.Select(c => c.ID));
            }
        }
    }
    [Fact]
    public void ShouldSaveRepeatTimes() {
        var node = new RepeaterNode("rep") { Times = 3 };
        GBTNode.Data saved = node.Save(null)[0];
        RepeaterNode loaded = _loader.Load<RepeaterNode>(saved);
        Assert.Equal(3, loaded.Times);
    }
}

public class NodeHierarchyGeneartor : IEnumerable<object[]> {
    public record TestCase(GBTNode Root) {
        public GBTNode[] Nodes {
            get {
                List<GBTNode> nodes = [];
                Collect(Root, nodes);
                return [.. nodes];
            }
        }
        private static void Collect(GBTNode node, List<GBTNode> nodes) {
            nodes.Add(node);
            if (node is IParentNode parent) {
                foreach (GBTNode child in parent.Children) {
                    Collect(child, nodes);
                }
            }
        }
    }
    public IEnumerator<object[]> GetEnumerator() {
        // Case: Single Node
        yield return [new TestCase(new CallbackNode("child 1"))];

        // Case: Single empty parent
        yield return [new TestCase(new SequenceNode("seq"))];

        // Case: Sequence
        SequenceNode root = new SequenceNode("seq").AddChildren(
            new CallbackNode("child 1"),
            new CallbackNode("child 2"));
        yield return [new TestCase(root)];

        // Case: Nested Selector
        root = new SequenceNode("seq").AddChild(
            new SelectorNode("sel").AddChildren(
                new CallbackNode("child 1"),
                new CallbackNode("child 2")));
        yield return [new TestCase(root)];

        // Cast: Composite and Decorator
        yield return [new TestCase(
                new InverterNode("inv").AddChild(
                    new SelectorNode("sel").AddChildren(
                        new SucceederNode("succ").AddChild(
                            new CallbackNode("child 1")),
                        new CallbackNode("child 2")
            )))];
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}