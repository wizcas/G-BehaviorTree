namespace GBT.Exceptions;

public class NodeNotFoundException : Exception {
    public NodeNotFoundException(
        string id,
        string message) : base($"{message}: node ({id}) not found") {
    }
}