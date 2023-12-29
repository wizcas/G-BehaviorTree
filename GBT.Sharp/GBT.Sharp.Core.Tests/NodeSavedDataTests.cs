using GBT.Sharp.Core.Nodes;
using System.Collections;
using Xunit.Abstractions;

namespace GBT.Sharp.Core.Tests;

public class NodeSavedDataTests(ITestOutputHelper output) {
    [Theory]
    [ClassData(typeof(NodeHierarchyGeneartor))]
    public void ShouldSaveLoadHierarchy(NodeHierarchyGeneartor.TestCase testCase) {
        // var root = new SequenceNode("root");
        // var child1 = new CallbackNode("child 1") { Parent = root };
        // var child2 = new CallbackNode("child 2") { Parent = root };
        // var testedNodes = new Node[] { root, child1, child2 };
        var testedNodes = testCase.Nodes;
        // Save
        List<Node.SavedData> saves = [];
        testCase.Root.Save(saves);
        Assert.Equal(testedNodes.Length, saves.Count);
        var index = 0;
        foreach (Node node in testedNodes) {
            Node.SavedData save = saves[index++];
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
        Dictionary<string, Node> cache = [];
        Node[] loads = saves.Select(save => save.LoadNode(cache)).ToArray();
        index = 0;
        foreach (Node node in testedNodes) {
            Node load = loads[index++];
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
}

public class NodeHierarchyGeneartor : IEnumerable<object[]> {
    public record TestCase(Node Root) {
        public Node[] Nodes {
            get {
                List<Node> nodes = [];
                Collect(Root, nodes);
                return [.. nodes];
            }
        }
        private static void Collect(Node node, List<Node> nodes) {
            nodes.Add(node);
            if (node is IParentNode parent) {
                foreach (Node child in parent.Children) {
                    Collect(child, nodes);
                }
            }
        }
    }
    public IEnumerator<object[]> GetEnumerator() {
        // Case: Single Node
        yield return [new TestCase(new CallbackNode("child 1"))];

        // Case: Single empty parent
        yield return [new TestCase(new SequenceNode("root"))];

        // Case: Sequence
        SequenceNode root = new SequenceNode("root").AddChildren(
            new CallbackNode("child 1"),
            new CallbackNode("child 2")).Cast<SequenceNode>();
        yield return [new TestCase(root)];

        // Case: Nested Selector
        root = new SequenceNode("root").AddChild(
            new SelectorNode("root 2").AddChildren(
                new CallbackNode("child 1"),
                new CallbackNode("child 2")).Cast<Node>())
            .Cast<SequenceNode>();
        yield return [new TestCase(root)];
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}