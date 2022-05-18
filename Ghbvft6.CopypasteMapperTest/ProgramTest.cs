using Ghbvft6.CopypasteMapper;
using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Threading;
using System.Windows;
using Xunit;

namespace Ghbvft6.CopypasteMapperTest {
    public class ProgramTest {

        private void ExecuteInSTAState(Action action) {
            ExceptionDispatchInfo exceptionInfo = null;
            var remapThread = new Thread(() => {
                try {
                    action();
                } catch (Exception ex) {
                    exceptionInfo = ExceptionDispatchInfo.Capture(ex);
                }
            });
            remapThread.SetApartmentState(ApartmentState.STA);
            remapThread.Start();
            remapThread.Join();

            exceptionInfo?.Throw();
        }

        [Fact]
        public void Test1() {
            var settings = new Settings();
            settings.mappings = new[] {
                new Settings.Mapping {
                    matchPattern = @"C:\\.*",
                    replacementPattern = @"\\",
                    replacement = @"/",
                }
            };

            ExecuteInSTAState(() => {
                Clipboard.SetText("C:\\test");
                Program.Remap(settings);
                Assert.Equal("C:/test", Clipboard.GetText());
            });
        }

        [Fact]
        public void Test2() {
            File.Copy("Ghbvft6.CopypasteMapper.Settings.json", $"{Program.ConfigDir.FullName}/Ghbvft6.CopypasteMapper.Settings.json", true);

            var path = Program.GetJsonPath<Settings>();
            var json = File.ReadAllText(path);
            var newSettings = JsonSerializer.Deserialize<Settings>(json, new JsonSerializerOptions {
                IncludeFields = true
            }) ?? throw new Exception($"Couldn't read {path}");
            var settings = new Settings() {
                delay = newSettings.delay,
                mappings = newSettings.mappings
            };

            ExecuteInSTAState(() => {
                Clipboard.SetText("C:\\test");
                Program.Remap(settings);
                Assert.Equal("C:/test", Clipboard.GetText());
            });
        }
    }
}
