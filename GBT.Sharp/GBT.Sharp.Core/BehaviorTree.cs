using GBT.Sharp.Core.Nodes;

namespace GBT.Sharp.Core;

public class BehaviorTree {
    private ITreeContext _context;
    private INode? _rootNode;
    private INode? _runningNode;

    public ITreeContext Context {
        get => _context;
        set {
            if (_context != value) {
                _context = value;
                OnContextChanged();
            }
        }
    }

    public BehaviorTree(ITreeContext treeContext) {
        _context = treeContext;
    }

    public void SetRootNode(INode rootNode) {
        _rootNode = rootNode;
        if (_context == null) {
            CreateContext();
        }
        _rootNode.Context = _context;
    }

    public void Tick() {
        if (_rootNode is null) {
            TreeLogger.Error("the tree has no root node", null);
        } else {
            (_runningNode ?? _rootNode).Tick();
        }
    }

    public void SetRunningNode(INode node) {
        _runningNode = node;
    }
    public void ExitRunningNode(INode node) {
        if (_runningNode != node) {
            TreeLogger.Warn($"skip: try to exit running node {node} but the running node is {_runningNode}", node);
            return;
        }
        _runningNode = _runningNode?.Parent;
    }
    public void Interrupt() {
        if (_runningNode is null) return;

        INode? node = _runningNode;
        while (node is not null) {
            node.Reset();
            node = node.Parent;
        }
        _runningNode = null;
    }

    private void CreateContext() {
        _context = new TreeContext(this);
    }

    private void OnContextChanged() {
        if (_rootNode != null) {
            // TODO: reset all node states to re-execute from the root for the new context
        }
    }
}
