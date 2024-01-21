using ColorCode;
using ColorCode.Common;
using ColorCode.Styling;
using GBT.Editor.Scripts.ColorCodeFormatters;
using Godot;
using Newtonsoft.Json;

[GlobalClass, Icon("res://Icons/json.svg")]
public partial class JsonRenderer : RichTextLabel {
    private string _rawJson = "";
    [Export]
    public string RawJson {
        get => _rawJson;
        set {
            if (_rawJson == value) return;
            _rawJson = value;
            UpdateJson(RawJson);
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

    private void UpdateJson(string raw) {
        var json = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(raw), Formatting.Indented);
        Clear();
        _formatter.Write(json, Languages.FindById(LanguageId.Json));
    }
}
