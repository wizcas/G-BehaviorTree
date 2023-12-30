using GBT.Nodes;
using System.Diagnostics.Tracing;

namespace GBT;

public record struct TreeLog(EventLevel Level, string Message, GBTNode? Node = null);

public class TreeLogger {
    public Action<string, TreeLog>? Logging { get; set; }
    public void Log(EventLevel level, string message, GBTNode? node) {
        var formatted = $"[{level}]({node}) {message}";
        Logging?.Invoke(formatted, new TreeLog(level, message, node));
    }
    public void Info(string message, GBTNode? node) {
        Log(EventLevel.Informational, message, node);
    }
    public void Warn(string message, GBTNode? node) {
        Log(EventLevel.Warning, message, node);
    }
    public void Error(string message, GBTNode? node) {
        Log(EventLevel.Error, message, node);
    }
    public void Critical(string message, GBTNode? node) {
        Log(EventLevel.Critical, message, node);
    }
    public void Verbose(string message, GBTNode? node) {
        Log(EventLevel.Verbose, message, node);
    }
}