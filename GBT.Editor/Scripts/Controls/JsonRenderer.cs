using ColorCode;
using ColorCode.Common;
using ColorCode.Styling;
using GBT.Editor.Scripts.ColorCodeFormatters;
using Godot;
using Newtonsoft.Json;

[GlobalClass, Icon("res://Icons/json.svg")]
public partial class JsonRenderer : RichTextLabel {
    private string? _rawJson;
    [Export]
    public string? RawJson {
        get => _rawJson;
        set {
            if (_rawJson == value) return;
            _rawJson = value;
            UpdateText(RawJson);
        }
    }

    private RichTextFormatter _formatter;

    public JsonRenderer() : base() {
        _formatter = new(this, StyleDictionary.DefaultDark);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }

    public void SetError(string? errorMessage) {
        UpdateText(RawJson, errorMessage);
    }

    private void UpdateText(string? raw, string? err = null) {
        if (err == null && string.IsNullOrWhiteSpace(raw)) {
            err = "(no data)";
        }
        Clear();
        if (err == null) {
            var json = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(raw), Formatting.Indented);
            _formatter.Write(json, Languages.FindById(LanguageId.Json));
        } else {
            PushColor(Colors.LightGray);
            AppendText(err);
            Pop();
        }
    }
}
