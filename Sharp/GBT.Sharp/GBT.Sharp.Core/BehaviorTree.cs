using GBT.Sharp.Core.Nodes;

namespace GBT.Sharp.Core;

public interface ITreeContext {
    BehaviorTree Tree { get; init; }
}
public struct TreeContext : ITreeContext {
    public BehaviorTree Tree { get; init; }
    public TreeContext(BehaviorTree tree) {
        Tree = tree;
    }
}
public class BehaviorTree {
    private ITreeContext _context;
    private INode? _rootNode;

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
            // TODO: warn no root node
        } else {
            _rootNode.Tick();
        }
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
