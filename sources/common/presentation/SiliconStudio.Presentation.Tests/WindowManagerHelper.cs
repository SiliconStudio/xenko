using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using NUnit.Framework;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Presentation.Interop;
using SiliconStudio.Presentation.Windows;

namespace SiliconStudio.Presentation.Tests
{
    internal static class WindowManagerHelper
    {
        private const int TimeoutDelay = 10000;
        private static bool forwardingToConsole;

        public static Task Timeout => !Debugger.IsAttached ? Task.Delay(TimeoutDelay) : new TaskCompletionSource<int>().Task;

        public static Task<Dispatcher> CreateUIThread()
        {
            var tcs = new TaskCompletionSource<Dispatcher>();
            var thread = new Thread(() =>
            {
                tcs.SetResult(Dispatcher.CurrentDispatcher);
                Dispatcher.Run();
            })
            {
                Name = "Test UI thread",
                IsBackground = true
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        public static void ShutdownUIThread(Dispatcher dispatcher)
        {
            // Wait a bit to make sure everything window-related has been executed before shutting down
            Thread.Sleep(100);
            Thread thread = null;
            dispatcher.Invoke(() => thread = Thread.CurrentThread);
            dispatcher.InvokeShutdown();
            thread.Join();
        }

        public static async Task TaskWithTimeout(Task task)
        {
            await Task.WhenAny(task, Timeout);
            if (task.Exception != null)
                ExceptionDispatchInfo.Capture(task.Exception.InnerException).Throw();

            Assert.True(task.IsCompleted, "Test timed out");
        }

        public static LoggerResult CreateLoggerResult(Logger logger)
        {
            var loggerResult = new LoggerResult();
            logger.MessageLogged += (sender, e) => loggerResult.Log(e.Message);
            if (!forwardingToConsole)
            {
                logger.MessageLogged += (sender, e) => Console.WriteLine(e.Message);
                forwardingToConsole = true;
            }
            return loggerResult;
        }

        public static IDisposable InitWindowManager(Dispatcher dispatcher, out LoggerResult loggerResult)
        {
            var manager = new WindowManagerWrapper(dispatcher);
            loggerResult = CreateLoggerResult(WindowManager.Logger);
            return manager;
        }

        private class WindowManagerWrapper : IDisposable
        {
            private readonly WindowManager manager;
            private readonly Dispatcher localDispatcher;

            public WindowManagerWrapper(Dispatcher dispatcher)
            {
                localDispatcher = Dispatcher.CurrentDispatcher;
                manager = localDispatcher.Invoke(() => new WindowManager(dispatcher));
            }

            public void Dispose()
            {
                localDispatcher.Invoke(() => manager.Dispose());
            }
        }
    }
}
