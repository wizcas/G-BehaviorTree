namespace GBT.Nodes;

/// <summary>
/// CallbackNode can be attached with callback functions to its lifecycle events,
/// which can be useful for testing or debugging.
/// </summary>
public class CallbackNode : GBTNode {
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

    protected override void Initialize() {
        base.Initialize();
        OnInitialize?.Invoke(this);
    }
    protected override void CleanUp() {
        base.CleanUp();
        OnCleanUp?.Invoke(this);
    }
}