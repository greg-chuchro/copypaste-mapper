using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
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

        public static DirectoryInfo ConfigDir { get; }
        private static Settings? settings;

        static Program() {
            var configPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/{Process.GetCurrentProcess().ProcessName}";
            ConfigDir = Directory.CreateDirectory(configPath);
        }

        public static string GetJsonPath<T>() {
            return $"{ConfigDir.FullName}/{typeof(T).FullName}.json";
        }

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
                    var match = Regex.Match(clipboardText, mapping.matchPattern);
                    if (match.Length == clipboardText.Length) {
                        var newText = Regex.Replace(clipboardText, mapping.replacementPattern, mapping.replacement);
                        SetText(newText);
                    }
                }
            }
        }

        private static void RemapLoop() {
            while (true) {
                try {
                    Remap(settings!);
                } catch (COMException ex) when (ex.HResult == CLIPBRD_E_CANT_OPEN) { }
                Thread.Sleep(settings!.delay);
            }
        }

        [STAThread]
        static void Main(string[] args) {
            new Thread(() => {
                while (true) {
                    var path = GetJsonPath<Settings>();
                    var json = File.ReadAllText(path);
                    var newSettings = JsonSerializer.Deserialize<Settings>(json, new JsonSerializerOptions {
                        IncludeFields = true
                    }) ?? throw new Exception($"Couldn't read {path}");
                    settings = new() {
                        delay = newSettings.delay,
                        mappings = newSettings.mappings
                    };

                    Thread.Sleep(1000);
                }
            }).Start();
            while (settings == null) {
                Thread.Sleep(100);
            }
            RemapLoop();
        }
    }
}
