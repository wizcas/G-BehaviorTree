namespace GBT.Sharp.Core;

public interface ITreeContext {
    BehaviorTree Tree { get; init; }
}
public class TreeContext : ITreeContext {
    public BehaviorTree Tree { get; init; }

    public TreeContext(BehaviorTree tree) {
        Tree = tree;
    }
}
