using GBT.Sharp.Core.Nodes;
using System.Diagnostics.Tracing;

namespace GBT.Sharp.Core;

public readonly struct TreeLog {
    public EventLevel Level { get; init; }
    public string Message { get; init; }
    public INode? Node { get; init; }

    public TreeLog(EventLevel level, string message, INode? node = null) {
        Level = level;
        Message = message;
        Node = node;
    }
}

public static class TreeLogger {
    public static Action<string>? WriteLog { get; set; }
    public static void Log(EventLevel level, string message, INode? node) {
        // TODO: log to console, file, etc.
        var formatted = $"[{level}]({node}) {message}";
        WriteLog?.Invoke(formatted);
    }
    public static void Info(string message, INode? node) {
        Log(EventLevel.Informational, message, node);
    }
    public static void Warn(string message, INode? node) {
        Log(EventLevel.Warning, message, node);
    }
    public static void Error(string message, INode? node) {
        Log(EventLevel.Error, message, node);
    }
    public static void Critical(string message, INode? node) {
        Log(EventLevel.Critical, message, node);
    }
    public static void Verbose(string message, INode? node) {
        Log(EventLevel.Verbose, message, node);
    }
}