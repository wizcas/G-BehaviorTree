using GBT.Sharp.Core.Nodes;

namespace GBT.Sharp.Core.Diagnostics;

public class Pass {
    public record struct Footprint(GBTNode? Node, string Content) {
        public string NodeName => Node?.Name ?? "<tree>";
        public DateTime Time { get; } = DateTime.Now;

        public string ToShortString() {
            return $"{NodeName} -> {Content}";
        }
    }

    private readonly Stack<Footprint> _footprints = new();

    public Footprint[] Footprints => _footprints.ToArray();
    public IDictionary<string, Footprint[]> FootprintsByNodes =>
        _footprints.GroupBy(e => e.Node).ToDictionary(g => g.Key?.ID ?? "<tree>", g => g.ToArray());


    public Pass() {
    }

    public Footprint Add(GBTNode? node, string content) {
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

    public void Add(GBTNode? node, string content) {
        if (_passes.Count == 0) {
            NewPass();
        }
        Pass.Footprint fp = _passes.Last().Add(node, content);
        FootprintAdded?.Invoke();
    }
}