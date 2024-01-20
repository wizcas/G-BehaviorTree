using NanoidDotNet;

namespace GBT.Nodes;

/// <summary>
/// The base class of all Behavior Tree Nodes, which defines
/// the basic properties, methods, and behaviors of a node.
/// </summary>
public abstract partial class GBTNode {
    public string ID { get; }
    public string Name { get; set; }
    public IComparable OrderKey { get; set; } = 0;


    private GBTNode? _parent;
    public GBTNode? Parent { get => _parent; set => SetParent(value); }

    public bool IsDisabled { get; set; }

    private BehaviorTree? _tree;
    public BehaviorTree? Tree {
        get => _tree ?? Parent?.Tree;
        internal set {
            if (_tree != value) {
                BehaviorTree? oldTree = _tree;
                _tree = value;
                if (oldTree != Tree) {
                    OnTreeChanged();
                }
            }
        }
    }

    public TreeRuntime? Runtime => Tree?.Runtime;
    public NodeContext Context { get; init; }

    /// <summary>
    /// A quick accessor to <see cref="NodeContext.State"/>.
    /// </summary>
    public NodeState State {
        get => Context.State;
        set => Context.State = value;
    }
    protected bool CanExit => Runtime?.RunningNode == this && State != NodeState.Running;

    public GBTNode(string id, string name) {
        ID = id;
        Name = name;
        Context = CreateContext();
        Reset();
    }
    public GBTNode(string name) : this(Nanoid.Generate(), name) { }
    public GBTNode() : this("") {
        Name = $"New {GetType().Name}";
    }

    protected virtual NodeContext CreateContext() {
        return new NodeContext(this);
    }

    protected void Enter() {
        if (Runtime is not null) {
            Runtime.Trace.Add(this, "enter");
            Initialize();
            Runtime.RunningNode = this;
        }
    }

    /// <summary>
    /// Set the node ready for running.
    /// This method is for internal purpose.
    /// </summary>
    protected virtual void Initialize() {
    }

    /// <summary>
    /// Called everytime the node is executed.
    /// </summary>
    public void Tick() {
        if (Runtime is null) {
            State = NodeState.Unvisited;
            BehaviorTree.Logger.Error("this node has no context", this);
            return;
        }
        if (IsDisabled) {
            State = NodeState.Unvisited;
            Runtime.Trace.Add(this, "skip: disabled");
            return;
        }
        if (State != NodeState.Running) {
            Enter();
        }
        State = NodeState.Running;
        Runtime.Trace.Add(this, "tick");
        DoTick();
        TryExit();
    }
    protected abstract void DoTick();

    /// <summary>
    /// Check whether this node has done execution and can be exited. 
    /// If yes, any exit steps of the node will be run.
    /// </summary>
    protected void TryExit() {
        // Need to check if the node is still to be exited, 
        // because if this is a parent node and its child will exit within this tick,
        // TryExit() will be called twice - once when the child exits, and once in the parent's Tick().
        if (!CanExit) {
            return;
        }
        Runtime!.Trace.Add(this, "exit");
        CleanUp();
        if (State != NodeState.Unvisited) {
            Runtime!.RunningNode = Parent;
            (Parent as IParentNode)?.AfterChildExit(this);
        }
    }
    /// <summary>
    /// Clean up any intermediate state or data that was set
    /// by running this node.
    /// </summary>
    protected virtual void CleanUp() {
    }
    /// <summary>
    /// Reset this node to its initial state and data.
    /// </summary>
    public virtual void Reset() {
        Runtime?.Trace.Add(this, "reset");
        if (State != NodeState.Unvisited) {
            CleanUp();
            Context.Reset();
            State = NodeState.Unvisited;
        }
    }
    public T Cast<T>() where T : GBTNode {
        return (T)this;
    }

    protected virtual void OnTreeChanged() { }

    private void SetParent(GBTNode? parent) {
        if (parent == Parent) {
            return;
        }

        if (parent is not null and not IParentNode) {
            throw new ArgumentException("only IParentNode can be set as parent", nameof(parent));
        }

        if (Parent is not null and IParentNode oldParent) {
            oldParent.RemoveChild(this);
        }
        _parent = parent;
        if (_parent != null) {
            (_parent as IParentNode)?.AddChild(this);
            if (Runtime?.Tree.RootNode == this) {
                Runtime?.Tree.SetRootNode(_parent);
            }
        }
    }

    public override string ToString() {
        return $"{Name} ({ID}/{GetType().Name})";
    }

    public IEnumerable<GBTNode> Flatten() {
        yield return this;
        if (this is IParentNode parent) {
            foreach (GBTNode child in parent.Children) {
                foreach (GBTNode inner in child.Flatten()) {
                    yield return inner;
                }
            }
        }
    }

    public List<Data> Save(List<Data>? savedNodes) {
        BeforeSave();
        savedNodes ??= new();
        savedNodes.Add(WriteSavedData());
        if (this is IParentNode parent) {
            foreach (GBTNode child in parent.Children) {
                child.Save(savedNodes);
            }
        }
        return savedNodes;
    }
    protected virtual void BeforeSave() { }
    public virtual void AfterLoad() { }
    public virtual Data WriteSavedData() {
        return Data.FromNode(this);
    }
    public virtual void ReadSaveData(Data data) {
        IsDisabled = data.IsDisabled;
    }

}

public abstract class Node<TContext> : GBTNode where TContext : NodeContext {
    protected Node() {
    }

    protected Node(string name) : base(name) {
    }

    protected Node(string id, string name) : base(id, name) {
    }

    public new TContext Context => (TContext)base.Context;

    protected override TContext CreateContext() {
        return (TContext)Activator.CreateInstance(typeof(TContext), this)!;
    }
}