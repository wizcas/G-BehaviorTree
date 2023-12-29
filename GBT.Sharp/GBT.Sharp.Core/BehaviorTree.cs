using GBT.Sharp.Core.Nodes;

namespace GBT.Sharp.Core;

public class BehaviorTree {
    public static TreeLogger Logger { get; } = new TreeLogger();


    private TreeContext _context;
    private Node? _rootNode;

    public TreeContext Context {
        get => _context;
        set {
            if (_context != value) {
                _context = value;
                OnContextChanged();
            }
        }
    }

    public BehaviorTree(TreeContext? context = null) {
        _context = context ?? CreateContext();
    }

    public void SetRootNode(Node rootNode) {
        _rootNode = rootNode;
        _rootNode.Context = _context;
    }

    public void Tick() {
        if (_rootNode is null) {
            throw new InvalidOperationException("the tree has no root node");
        } else {
            if (Context.RunningNode is null) {
                Context.Trace.NewPass();
            }
            (Context.RunningNode ?? _rootNode).Tick();
        }
    }

    public void Interrupt() {
        if (Context.RunningNode is null) return;

        Context.Trace.Add(null, $"interrupt");
        Node? node = Context.RunningNode;
        while (node is not null) {
            node.Reset();
            node = node.Parent;
        }
        Context.RunningNode = null;
    }

    private TreeContext CreateContext() {
        return new TreeContext(this);
    }

    private void OnContextChanged() {
        Interrupt();
    }
}