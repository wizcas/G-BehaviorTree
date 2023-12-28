using GBT.Sharp.Core.Nodes;
using System.Diagnostics.Tracing;

namespace GBT.Sharp.Core;

public record struct TreeLog(EventLevel Level, string Message, Node? Node = null);

public class TreeLogger {
    public Action<string, TreeLog>? Logging { get; set; }
    public void Log(EventLevel level, string message, Node? node) {
        var formatted = $"[{level}]({node}) {message}";
        Logging?.Invoke(formatted, new TreeLog(level, message, node));
    }
    public void Info(string message, Node? node) {
        Log(EventLevel.Informational, message, node);
    }
    public void Warn(string message, Node? node) {
        Log(EventLevel.Warning, message, node);
    }
    public void Error(string message, Node? node) {
        Log(EventLevel.Error, message, node);
    }
    public void Critical(string message, Node? node) {
        Log(EventLevel.Critical, message, node);
    }
    public void Verbose(string message, Node? node) {
        Log(EventLevel.Verbose, message, node);
    }
}