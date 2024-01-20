using GBT;
using Godot;
using System;
using System.IO;

public partial class GraphManager : Node {
    private const string EXTENSION = ".gbt";
    public event Action? RequestReloadTree;
    public static GraphManager GetInstance() {
        return ((SceneTree)Engine.GetMainLoop()).Root.GetNode<GraphManager>(nameof(GraphManager));
    }
    public BehaviorTree EditingTree { get; private set; } = new();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        Name = nameof(GraphManager);
    }

    public void New() {
        EditingTree = new();
        RequestReloadTree?.Invoke();
    }

    public void Save() {
        if (EditingTree.FilePath is null) {
            SaveAs();
        } else {
            DoSave(EditingTree.FilePath);
        }
    }
    public void SaveAs() {
        FileDialog dialog = CreateDialog();
        dialog.FileMode = FileDialog.FileModeEnum.SaveFile;
        dialog.FileSelected += DoSave;
        dialog.PopupCentered();
    }

    private void DoSave(string path) {
        if (!path.EndsWith(EXTENSION)) {
            path += EXTENSION;
        }
        EditingTree.FilePath = path;
        using (FileStream file = File.OpenWrite(path)) {
            EditingTree.Save(file);
        }
    }

    public void Open() {
        FileDialog dialog = CreateDialog();
        dialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        dialog.FileSelected += (string path) => {
            using (FileStream file = File.Open(path, FileMode.Open)) {
                EditingTree = new();
                EditingTree.Load(file);
                RequestReloadTree?.Invoke();
            }
        };
        dialog.PopupCentered();
    }

    private FileDialog CreateDialog() {
        var dlg = new FileDialog();
        dlg.Filters = new string[] { $"*{EXTENSION}" };
        dlg.CurrentDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        dlg.Access = FileDialog.AccessEnum.Filesystem;
        dlg.UseNativeDialog = true;
        AddChild(dlg);
        return dlg;
    }
}
