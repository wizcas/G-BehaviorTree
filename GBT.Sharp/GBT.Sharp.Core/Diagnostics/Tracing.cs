using GBT.Sharp.Core.Nodes;

namespace GBT.Sharp.Core.Diagnostics;

public readonly struct Pass {
    public record struct Footprint(INode? Node, string Content) {
        public DateTime Time { get; } = DateTime.Now;
    }

    private readonly Stack<Footprint> _footprints = new();

    public IDictionary<string, IEnumerable<Footprint>> NodeEvents =>
        _footprints.GroupBy(e => e.Node).ToDictionary(g => g.Key?.ID ?? "<tree>", g => g.AsEnumerable());


    public Pass() {
    }

    public Footprint Add(INode? node, string content) {
        var fp = new Footprint(node, content);
        _footprints.Push(fp);
        return fp;
    }
}

public class Trace {
    private readonly List<Pass> _passes = new();
    public IEnumerable<Pass> Passes => _passes;
    public event Func<Pass>? FootprintAdded;

    public void NewPass() {
        _passes.Add(new());
    }

    public void Add(INode? node, string content) {
        if (_passes.Count == 0) {
            NewPass();
        }
        Pass.Footprint fp = _passes.Last().Add(node, content);
        FootprintAdded?.Invoke();
    }
}