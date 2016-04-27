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
using SiliconStudio.Presentation.Tests.WPF;
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

        public static void KillWindow(string title)
        {
            var windowHandle = NativeHelper.FindWindow(null, title);
            if (windowHandle == IntPtr.Zero)
                throw new InvalidOperationException("Unable to find a window with the given title");

            NativeHelper.SendMessage(windowHandle, NativeHelper.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
        public static void KillWindow(IntPtr hwnd)
        {
            NativeHelper.SendMessage(hwnd, NativeHelper.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
    }

    [TestFixture]
    public class TestWindowManager
    {
        private static readonly object LockObj = new object();

        [SetUp]
        protected virtual void Setup() => Monitor.Enter(LockObj);

        [TearDown]
        protected virtual void TearDown() => Monitor.Exit(LockObj);

        [Test]
        public async void TestInitDistroy()
        {
            LoggerResult loggerResult;
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
            }
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async void TestOpenCloseWindow()
        {
            LoggerResult loggerResult;
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new StandardWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() =>
                {
                    Assert.NotNull(WindowManager.mainWindow);
                    Assert.AreEqual(window, WindowManager.mainWindow.Window);
                    Assert.AreEqual(window.ToHwnd(dispatcher), WindowManager.mainWindow.Hwnd);
                    Assert.AreEqual(null, WindowManager.mainWindow.Owner);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsModal);
                    Assert.AreEqual(false, WindowManager.mainWindow.IsDisabled);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsShown);
                });

                // Close the main window
                var mainWindow = WindowManager.mainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    Assert.AreEqual(null, WindowManager.mainWindow);
                    Assert.AreEqual(null, mainWindow.Owner);
                    Assert.AreEqual(false, mainWindow.IsModal);
                    Assert.AreEqual(false, mainWindow.IsDisabled);
                    Assert.AreEqual(false, mainWindow.IsShown);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async void TestMainWindowThenModalBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new StandardWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.BeginInvoke(new Func<bool?>(() => modalWindow.ShowDialog()));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() =>
                {
                    Assert.AreEqual(window, WindowManager.mainWindow.Window);
                    Assert.AreEqual(window.ToHwnd(dispatcher), WindowManager.mainWindow.Hwnd);
                    Assert.AreEqual(null, WindowManager.mainWindow.Owner);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsModal);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsDisabled);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsShown);
                    Assert.AreEqual(1, WindowManager.modalWindows.Count);
                    Assert.AreEqual(modalWindow, WindowManager.modalWindows[0].Window);
                    Assert.AreEqual(WindowManager.mainWindow, WindowManager.modalWindows[0].Owner);
                    Assert.AreEqual(true, WindowManager.modalWindows[0].IsModal);
                    Assert.AreEqual(false, WindowManager.modalWindows[0].IsDisabled);
                    Assert.AreEqual(true, WindowManager.modalWindows[0].IsShown);
                });

                // Close the modal window
                var modalWindowInfo = WindowManager.modalWindows[0];
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed();
                dispatcher.Invoke(() => modalWindow.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindowClosed);
                dispatcher.Invoke(() =>
                {
                    Assert.AreEqual(window, WindowManager.mainWindow.Window);
                    Assert.AreEqual(window.ToHwnd(dispatcher), WindowManager.mainWindow.Hwnd);
                    Assert.AreEqual(null, WindowManager.mainWindow.Owner);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsModal);
                    Assert.AreEqual(false, WindowManager.mainWindow.IsDisabled);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsShown);
                    Assert.AreEqual(0, WindowManager.modalWindows.Count);
                    Assert.AreEqual(modalWindow, modalWindowInfo.Window);
                    Assert.AreEqual(null, modalWindowInfo.Owner);
                    Assert.AreEqual(false, modalWindowInfo.IsModal);
                    Assert.AreEqual(false, modalWindowInfo.IsDisabled);
                    Assert.AreEqual(false, modalWindowInfo.IsShown);
                });

                // Close the main window
                var mainWindow = WindowManager.mainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    Assert.AreEqual(null, WindowManager.mainWindow);
                    Assert.AreEqual(null, mainWindow.Owner);
                    Assert.AreEqual(false, mainWindow.IsModal);
                    Assert.AreEqual(false, mainWindow.IsDisabled);
                    Assert.AreEqual(false, mainWindow.IsShown);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async void TestMainWindowThenModalBoxCloseMain()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new StandardWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.BeginInvoke(new Func<bool?>(() => modalWindow.ShowDialog()));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() =>
                {
                    Assert.AreEqual(window, WindowManager.mainWindow.Window);
                    Assert.AreEqual(window.ToHwnd(dispatcher), WindowManager.mainWindow.Hwnd);
                    Assert.AreEqual(null, WindowManager.mainWindow.Owner);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsModal);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsDisabled);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsShown);
                    Assert.AreEqual(1, WindowManager.modalWindows.Count);
                    Assert.AreEqual(modalWindow, WindowManager.modalWindows[0].Window);
                    Assert.AreEqual(WindowManager.mainWindow, WindowManager.modalWindows[0].Owner);
                    Assert.AreEqual(true, WindowManager.modalWindows[0].IsModal);
                    Assert.AreEqual(false, WindowManager.modalWindows[0].IsDisabled);
                    Assert.AreEqual(true, WindowManager.modalWindows[0].IsShown);
                });

                // Close the main window - this should also close the modal window
                var mainWindow = WindowManager.mainWindow;
                var modalWindowInfo = WindowManager.modalWindows[0];
                var hidden = WindowManagerHelper.NextMainWindowChanged();
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed();
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(Task.WhenAll(hidden, modalWindowClosed));
                dispatcher.Invoke(() =>
                {
                    Assert.AreEqual(null, WindowManager.mainWindow);
                    Assert.AreEqual(null, mainWindow.Owner);
                    Assert.AreEqual(false, mainWindow.IsModal);
                    Assert.AreEqual(false, mainWindow.IsDisabled);
                    Assert.AreEqual(false, mainWindow.IsShown);
                    Assert.AreEqual(0, WindowManager.modalWindows.Count);
                    Assert.AreEqual(modalWindow, modalWindowInfo.Window);
                    Assert.AreEqual(null, modalWindowInfo.Owner);
                    Assert.AreEqual(false, modalWindowInfo.IsModal);
                    Assert.AreEqual(false, modalWindowInfo.IsDisabled);
                    Assert.AreEqual(false, modalWindowInfo.IsShown);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async void TestMainWindowThenTwoModalBoxes()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new StandardWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);

                // Open a first modal window
                var modalWindow1 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindow1Opened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.BeginInvoke(new Func<bool?>(() => modalWindow1.ShowDialog()));
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Opened);

                // Open a second modal window
                var modalWindow2 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindow2Opened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.BeginInvoke(new Func<bool?>(() => modalWindow2.ShowDialog()));
                await WindowManagerHelper.TaskWithTimeout(modalWindow2Opened);
                dispatcher.Invoke(() =>
                {
                    Assert.AreEqual(window, WindowManager.mainWindow.Window);
                    Assert.AreEqual(window.ToHwnd(dispatcher), WindowManager.mainWindow.Hwnd);
                    Assert.AreEqual(null, WindowManager.mainWindow.Owner);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsModal);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsDisabled);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsShown);
                    Assert.AreEqual(2, WindowManager.modalWindows.Count);
                    Assert.AreEqual(modalWindow1, WindowManager.modalWindows[0].Window);
                    Assert.AreEqual(WindowManager.mainWindow, WindowManager.modalWindows[0].Owner);
                    Assert.AreEqual(true, WindowManager.modalWindows[0].IsModal);
                    Assert.AreEqual(true, WindowManager.modalWindows[0].IsDisabled);
                    Assert.AreEqual(true, WindowManager.modalWindows[0].IsShown);
                    Assert.AreEqual(modalWindow2, WindowManager.modalWindows[1].Window);
                    Assert.AreEqual(WindowManager.modalWindows[0], WindowManager.modalWindows[1].Owner);
                    Assert.AreEqual(true, WindowManager.modalWindows[1].IsModal);
                    Assert.AreEqual(false, WindowManager.modalWindows[1].IsDisabled);
                    Assert.AreEqual(true, WindowManager.modalWindows[1].IsShown);
                });

                // Close the second modal window
                var modalWindow2Info = WindowManager.modalWindows[1];
                var modalWindow2Closed = WindowManagerHelper.NextModalWindowClosed();
                dispatcher.Invoke(() => modalWindow2.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindow2Closed);
                dispatcher.Invoke(() =>
                {
                    Assert.AreEqual(window, WindowManager.mainWindow.Window);
                    Assert.AreEqual(window.ToHwnd(dispatcher), WindowManager.mainWindow.Hwnd);
                    Assert.AreEqual(null, WindowManager.mainWindow.Owner);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsModal);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsDisabled);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsShown);
                    Assert.AreEqual(1, WindowManager.modalWindows.Count);
                    Assert.AreEqual(modalWindow1, WindowManager.modalWindows[0].Window);
                    Assert.AreEqual(WindowManager.mainWindow, WindowManager.modalWindows[0].Owner);
                    Assert.AreEqual(true, WindowManager.modalWindows[0].IsModal);
                    Assert.AreEqual(false, WindowManager.modalWindows[0].IsDisabled);
                    Assert.AreEqual(true, WindowManager.modalWindows[0].IsShown);
                    Assert.AreEqual(modalWindow2, modalWindow2Info.Window);
                    Assert.AreEqual(null, modalWindow2Info.Owner);
                    Assert.AreEqual(false, modalWindow2Info.IsModal);
                    Assert.AreEqual(false, modalWindow2Info.IsDisabled);
                    Assert.AreEqual(false, modalWindow2Info.IsShown);
                });

                // Close the first modal window
                var modalWindow1Info = WindowManager.modalWindows[0];
                var modalWindow1Closed = WindowManagerHelper.NextModalWindowClosed();
                dispatcher.Invoke(() => modalWindow1.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Closed);
                dispatcher.Invoke(() =>
                {
                    Assert.AreEqual(window, WindowManager.mainWindow.Window);
                    Assert.AreEqual(window.ToHwnd(dispatcher), WindowManager.mainWindow.Hwnd);
                    Assert.AreEqual(null, WindowManager.mainWindow.Owner);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsModal);
                    Assert.AreEqual(false, WindowManager.mainWindow.IsDisabled);
                    Assert.AreEqual(true, WindowManager.mainWindow.IsShown);
                    Assert.AreEqual(0, WindowManager.modalWindows.Count);
                    Assert.AreEqual(modalWindow1, modalWindow1Info.Window);
                    Assert.AreEqual(null, modalWindow1Info.Owner);
                    Assert.AreEqual(false, modalWindow1Info.IsModal);
                    Assert.AreEqual(false, modalWindow1Info.IsDisabled);
                    Assert.AreEqual(false, modalWindow1Info.IsShown);
                });

                // Close the main window
                var mainWindow = WindowManager.mainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    Assert.AreEqual(null, WindowManager.mainWindow);
                    Assert.AreEqual(null, mainWindow.Owner);
                    Assert.AreEqual(false, mainWindow.IsModal);
                    Assert.AreEqual(false, mainWindow.IsDisabled);
                    Assert.AreEqual(false, mainWindow.IsShown);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            dispatcher.InvokeShutdown();
        }

    }
}
