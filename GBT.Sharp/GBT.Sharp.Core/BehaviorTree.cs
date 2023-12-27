using GBT.Sharp.Core.Nodes;

namespace GBT.Sharp.Core;

public class BehaviorTree {
    public static TreeLogger Logger { get; } = new TreeLogger();


    private ITreeContext _context;
    private INode? _rootNode;

    public INode? RunningNode { get; private set; }

    public ITreeContext Context {
        get => _context;
        set {
            if (_context != value) {
                _context = value;
                OnContextChanged();
            }
        }
    }

    public BehaviorTree(ITreeContext? context = null) {
        _context = context ?? CreateContext();
    }

    public void SetRootNode(INode rootNode) {
        _rootNode = rootNode;
        _rootNode.Context = _context;
    }

    public void Tick() {
        Context.NewTrace();
        if (_rootNode is null) {
            Logger.Error("the tree has no root node", null);
        } else {
            (RunningNode ?? _rootNode).Tick();
        }
    }

    public void SetRunningNode(INode node) {
        RunningNode = node;
    }
    public void ExitRunningNode(INode node) {
        if (RunningNode != node) {
            Logger.Warn($"skip: try to exit running node {node} but the running node is {RunningNode}", node);
            return;
        }
        RunningNode = node.Parent;
        node.Parent?.OnChildExit(node);
    }
    public void Interrupt() {
        if (RunningNode is null) return;

        INode? node = RunningNode;
        while (node is not null) {
            node.Reset();
            node = node.Parent;
        }
        RunningNode = null;
    }

    private TreeContext CreateContext() {
        return new TreeContext(this);
    }

    private void OnContextChanged() {
        Interrupt();
    }
}