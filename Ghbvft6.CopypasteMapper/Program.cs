using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace Ghbvft6.CopypasteMapper {
    public class Program {

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

        const int CLIPBRD_E_CANT_OPEN = unchecked((int)0x800401D0);
        const uint CF_UNICODETEXT = 13;

        public static void SetText(string text) {
            OpenClipboard(default);
            try {
                var textBytes = Marshal.StringToHGlobalUni(text);
                if (SetClipboardData(CF_UNICODETEXT, textBytes) == default) {
                    Marshal.FreeHGlobal(textBytes);
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            } finally {
                CloseClipboard();
            }
        }

        public static void Remap(Settings settings) {
            if (Clipboard.ContainsText()) {
                var clipboardText = Clipboard.GetText();
                foreach (var mapping in settings.mappings) {
                    var match = Regex.Match(clipboardText, mapping.MatchPattern);
                    if (match.Length == clipboardText.Length) {
                        var newText = Regex.Replace(clipboardText, mapping.ReplacementPattern, mapping.Replacement);
                        SetText(newText);
                    }
                }
            }
        }

        private static void RemapLoop(Settings settings) {
            while (true) {
                try {
                    Remap(settings);
                } catch (COMException ex) when (ex.HResult == CLIPBRD_E_CANT_OPEN) { }
                Thread.Sleep(settings.delay);
            }
        }

        [STAThread]
        static void Main(string[] args) {
            var settings = new Settings();
            settings.delay = 100;
            settings.mappings = new[] {
                new Settings.Mapping {
                    MatchPattern = @"C:\\.*",
                    ReplacementPattern = @"\\",
                    Replacement = @"/",
                }
            };
            RemapLoop(settings);
        }
    }
}
