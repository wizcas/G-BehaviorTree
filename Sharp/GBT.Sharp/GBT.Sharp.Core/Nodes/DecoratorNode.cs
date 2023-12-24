namespace GBT.Sharp.Core.Nodes;

public interface IDecoratorNode<TContext> : INode<TContext>
{
    INode<TContext>? Child { get; }
}
public class DecoratorNode
{
}
