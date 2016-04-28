using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using NUnit.Framework;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Presentation.Extensions;
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

        public static async Task TaskWithTimeout(Task task)
        {
            await Task.WhenAny(task, Timeout);
            Assert.True(task.IsCompleted, "Test timed out");
        }

        public static Task NextMainWindowChanged()
        {
            var tcs = new TaskCompletionSource<int>();
            WindowManager.MainWindowChanged += (s, e) => { tcs?.SetResult(0); tcs = null; };
            return tcs.Task;
        }

        public static Task NextModalWindowOpened()
        {
            var tcs = new TaskCompletionSource<int>();
            WindowManager.ModalWindowOpened += (s, e) => { tcs?.SetResult(0); tcs = null; };
            return tcs.Task;
        }

        public static Task NextModalWindowClosed()
        {
            var tcs = new TaskCompletionSource<int>();
            WindowManager.ModalWindowClosed += (s, e) => { tcs?.SetResult(0); tcs = null; };
            return tcs.Task;
        }

        public static IntPtr ToHwnd(this Window window, Dispatcher dispatcher)
        {
            return dispatcher.Invoke(() => new WindowInteropHelper(window).Handle);
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

        public static WindowManager InitWindowManager(Dispatcher dispatcher)
        {
            LoggerResult loggerResult;
            return InitWindowManager(dispatcher, out loggerResult);
        }

        public static WindowManager InitWindowManager(Dispatcher dispatcher, out LoggerResult loggerResult)
        {
            var manager = new WindowManager(dispatcher);
            loggerResult = CreateLoggerResult(manager.Logger);
            return manager;
        }

        public static void KillWindow(IntPtr hwnd)
        {
            NativeHelper.SendMessage(hwnd, NativeHelper.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public static void AssertWindowsStatus(Window mainWindow, params Window[] modalWindows)
        {
            if (modalWindows != null)
            {
                Assert.AreEqual(modalWindows.Length, WindowManager.modalWindows.Count);
                for (int i = modalWindows.Length - 1; i >= 0; --i)
                {
                    bool expectedIsDisabled = i < modalWindows.Length - 1;
                    WindowInfo expectedOwner = i > 0 ? WindowManager.modalWindows[i - 1] : WindowManager.mainWindow;
                    Assert.AreEqual(modalWindows[i], WindowManager.modalWindows[i].Window);
                    Assert.AreEqual(modalWindows[i]?.Owner, WindowManager.modalWindows[i].Window?.Owner);
                    Assert.AreEqual(expectedOwner, WindowManager.modalWindows[i].Owner);
                    Assert.AreEqual(true, WindowManager.modalWindows[i].IsModal);
                    Assert.AreEqual(expectedIsDisabled, WindowManager.modalWindows[i].IsDisabled);
                    Assert.AreEqual(true, WindowManager.modalWindows[i].IsShown);
                }
            }
            if (mainWindow != null)
            {
                Assert.NotNull(WindowManager.mainWindow);
                Assert.AreEqual(mainWindow, WindowManager.mainWindow.Window);
                Assert.AreEqual(null, WindowManager.mainWindow.Owner);
                Assert.AreEqual(null, WindowManager.mainWindow.Window.Owner);
                Assert.AreEqual(true, WindowManager.mainWindow.IsModal);
                Assert.AreEqual(WindowManager.modalWindows.Count > 0, WindowManager.mainWindow.IsDisabled);
                Assert.AreEqual(true, WindowManager.mainWindow.IsShown);
            }
            else
            {
                Assert.Null(WindowManager.mainWindow);
            }
        }

        public static void AssertWindowClosed(WindowInfo window)
        {
            Assert.AreEqual(null, window.Owner);
            //Assert.AreEqual(false, window.IsModal);
            Assert.AreEqual(false, window.IsDisabled);
            Assert.AreEqual(false, window.IsShown);
        }
    }
}
