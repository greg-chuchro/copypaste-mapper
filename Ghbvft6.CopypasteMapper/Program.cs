using System.Text.RegularExpressions;
using System.Windows;

namespace Ghbvft6.CopypasteMapper
{
    public class Program {

        public static void Remap(Settings settings) {
            if (Clipboard.ContainsText()) {
                var clipboardText = Clipboard.GetText();
                foreach (var mapping in settings.mappings) {
                    var match = Regex.Match(clipboardText, mapping.MatchPattern);
                    if (match.Length == clipboardText.Length) {
                        var newText = Regex.Replace(clipboardText, mapping.ReplacementPattern, mapping.Replacement);
                        Clipboard.SetText(newText);
                    }
                }
            }
        }

        static void Main(string[] args) { }
    }
}
