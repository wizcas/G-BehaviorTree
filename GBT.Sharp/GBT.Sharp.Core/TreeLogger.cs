using GBT.Sharp.Core.Nodes;
using System.Diagnostics.Tracing;

namespace GBT.Sharp.Core;

public record struct TreeLog(EventLevel Level, string Message, INode? Node = null);

public class TreeLogger {
    public Action<string, TreeLog>? Logging { get; set; }
    public void Log(EventLevel level, string message, INode? node) {
        var formatted = $"[{level}]({node}) {message}";
        Logging?.Invoke(formatted, new TreeLog(level, message, node));
    }
    public void Info(string message, INode? node) {
        Log(EventLevel.Informational, message, node);
    }
    public void Warn(string message, INode? node) {
        Log(EventLevel.Warning, message, node);
    }
    public void Error(string message, INode? node) {
        Log(EventLevel.Error, message, node);
    }
    public void Critical(string message, INode? node) {
        Log(EventLevel.Critical, message, node);
    }
    public void Verbose(string message, INode? node) {
        Log(EventLevel.Verbose, message, node);
    }
}