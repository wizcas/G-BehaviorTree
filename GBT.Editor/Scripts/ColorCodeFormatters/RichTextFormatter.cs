using ColorCode;
using ColorCode.Common;
using ColorCode.Parsing;
using ColorCode.Styling;
using Godot;
using System.Collections.Generic;

namespace GBT.Editor.Scripts.ColorCodeFormatters;
public class RichTextFormatter : CodeColorizerBase {
    public record Instruction(int Index, Scope? Scope);

    private RichTextLabel _richTextLabel;
    public RichTextFormatter(RichTextLabel richTextLabel, StyleDictionary? Styles = null, ILanguageParser? languageParser = null) : base(Styles, languageParser) {
        _richTextLabel = richTextLabel;
    }

    public void Write(string sourceCode, ILanguage language) {
        languageParser.Parse(sourceCode, language, Write);
    }

    protected override void Write(string parsedSourceCode, IList<Scope> scopes) {
        var instructions = new List<Instruction>();

        foreach (Scope scope in scopes)
            GetStyleInsertionsForCapturedStyle(scope, instructions);

        instructions.SortStable((x, y) => x.Index.CompareTo(y.Index));

        var offset = 0;

        foreach (Instruction styleInsertion in instructions) {
            var text = parsedSourceCode.Substring(offset, styleInsertion.Index - offset);
            _richTextLabel.AppendText(text);
            if (styleInsertion.Scope != null)
                WriteStyle(styleInsertion.Scope);
            else
                _richTextLabel.Pop();
            offset = styleInsertion.Index;
        }
        _richTextLabel.AppendText(parsedSourceCode.Substring(offset));
    }

    private void GetStyleInsertionsForCapturedStyle(Scope scope, ICollection<Instruction> styleInsertions) {
        styleInsertions.Add(new Instruction(scope.Index, scope));

        foreach (Scope childScope in scope.Children)
            GetStyleInsertionsForCapturedStyle(childScope, styleInsertions);

        styleInsertions.Add(new Instruction(scope.Index + scope.Length, null));
    }

    private void WriteStyle(Scope scope) {
        var foreground = string.Empty;

        if (Styles.Contains(scope.Name)) {
            Style style = Styles[scope.Name];

            foreground = "#" + style.Foreground.Substring(3);
        }
        if (!string.IsNullOrEmpty(foreground)) {
            _richTextLabel.PushColor(new Color(foreground));
        }
    }
}

