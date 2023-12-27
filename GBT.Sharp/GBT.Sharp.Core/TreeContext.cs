using GBT.Sharp.Core.Nodes;
using System.Collections;

namespace GBT.Sharp.Core;

public interface ITreeContext {
    BehaviorTree Tree { get; init; }
    IEnumerable<TreeTrace> Histories { get; }
    TreeTrace CurrentTrace { get; }

    void NewTrace();
}

public class TreeTrace : IEnumerable<INode> {
    private List<INode> _nodes = new();

    public IEnumerator<INode> GetEnumerator() {
        return _nodes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    internal void Add(INode node) {
        _nodes.Add(node);
    }
}

public class TreeContext : ITreeContext {
    public BehaviorTree Tree { get; init; }

    private readonly List<TreeTrace> _histories = new();
    public IEnumerable<TreeTrace> Histories => _histories;

    private TreeTrace? _currentTrace;
    public TreeTrace CurrentTrace {
        get {
            if (_currentTrace is null) {
                NewTrace();
            }
            return _currentTrace!;
        }
    }

    public TreeContext(BehaviorTree tree) {
        Tree = tree;
    }

    public void NewTrace() {
        _currentTrace = new();
        _histories.Add(_currentTrace);
    }
}