namespace GBT.Sharp.Core.Nodes;

/// <summary>
/// CallbackNode can be attached with callback functions to its lifecycle events,
/// which can be useful for testing or debugging.
/// </summary>
public class CallbackNode : Node {
    public Action<CallbackNode>? OnInitialize { get; set; }
    public Action<CallbackNode>? OnTick { get; set; }
    public Action<CallbackNode>? OnCleanUp { get; set; }

    public CallbackNode(string id, string name) : base(id, name) {
    }

    public CallbackNode(string name) : base(name) {
    }

    public CallbackNode() {
    }

    protected override void DoTick() {
        OnTick?.Invoke(this);
    }

    public override void Initialize() {
        base.Initialize();
        OnInitialize?.Invoke(this);
    }
    public override void CleanUp() {
        base.CleanUp();
        OnCleanUp?.Invoke(this);
    }
}