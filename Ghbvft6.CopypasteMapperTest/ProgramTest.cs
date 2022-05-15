using Ghbvft6.CopypasteMapper;
using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
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
                    MatchPattern = @"C:\\.*",
                    ReplacementPattern = @"\\",
                    Replacement = @"/",
                }
            };

            ExecuteInSTAState(() => {
                Clipboard.SetText("C:\\test");
                Program.Remap(settings);
                Assert.Equal("C:/test", Clipboard.GetText());
            });
        }
    }
}
