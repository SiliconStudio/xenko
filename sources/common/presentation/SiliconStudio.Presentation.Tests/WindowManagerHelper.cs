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
            if (task.Exception != null)
                ExceptionDispatchInfo.Capture(task.Exception.InnerException).Throw();

            Assert.True(task.IsCompleted, "Test timed out");
        }

        public static Task NextMainWindowChanged(Window newMainWindow)
        {
            var tcs = new TaskCompletionSource<int>();
            WindowManager.MainWindowChanged += (s, e) =>
            {
                if (tcs == null)
                    return;

                try
                {
                    Assert.AreEqual(newMainWindow, e.Window?.Window);
                    tcs?.SetResult(0);
                }
                catch (AssertionException ex)
                {
                    tcs.SetException(ex);
                }
                finally
                {
                    tcs = null;
                }
            };
            return tcs.Task;
        }

        public static Task NextMessageBoxOpened()
        {
            var tcs = new TaskCompletionSource<int>();
            WindowManager.ModalWindowOpened += (s, e) =>
            {
                if (tcs == null)
                    return;

                try
                {
                    Assert.IsNotNull(e.Window);
                    Assert.IsNull(e.Window.Window);
                    tcs?.SetResult(0);
                }
                catch (AssertionException ex)
                {
                    tcs.SetException(ex);
                }
                finally
                {
                    tcs = null;
                }
            };
            return tcs.Task;
        }

        public static Task NextMessageBoxClosed()
        {
            var tcs = new TaskCompletionSource<int>();
            WindowManager.ModalWindowClosed += (s, e) =>
            {
                if (tcs == null)
                    return;

                try
                {
                    Assert.IsNotNull(e.Window);
                    Assert.IsNull(e.Window.Window);
                    tcs?.SetResult(0);
                }
                catch (AssertionException ex)
                {
                    tcs.SetException(ex);
                }
                finally
                {
                    tcs = null;
                }
            };
            return tcs.Task;
        }

        public static Task NextModalWindowOpened(Window modalWindow)
        {
            var tcs = new TaskCompletionSource<int>();
            WindowManager.ModalWindowOpened += (s, e) =>
            {
                if (tcs == null)
                    return;

                try
                {
                    Assert.AreEqual(modalWindow, e.Window?.Window);
                    tcs?.SetResult(0);
                }
                catch (AssertionException ex)
                {
                    tcs.SetException(ex);
                }
                finally
                {
                    tcs = null;
                }
            };
            return tcs.Task;
        }

        public static Task NextModalWindowClosed(Window modalWindow)
        {
            var tcs = new TaskCompletionSource<int>();
            WindowManager.ModalWindowClosed += (s, e) =>
            {
                if (tcs == null)
                    return;

                try
                {
                    Assert.AreEqual(modalWindow, e.Window?.Window);
                    tcs?.SetResult(0);
                }
                catch (AssertionException ex)
                {
                    tcs.SetException(ex);
                }
                finally
                {
                    tcs = null;
                }
            };
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
            loggerResult = CreateLoggerResult(WindowManager.Logger);
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
                Assert.AreEqual(modalWindows.Length, WindowManager.ModalWindows.Count);
                for (var i = 0; i < modalWindows.Length; ++i)
                {
                    var expectedIsDisabled = i < modalWindows.Length - 1;
                    var expectedOwner = i > 0 ? WindowManager.ModalWindows[i - 1] : WindowManager.MainWindow;
                    Assert.AreEqual(modalWindows[i], WindowManager.ModalWindows[i].Window);
                    Assert.AreEqual(modalWindows[i]?.Owner, WindowManager.ModalWindows[i].Window?.Owner);
                    Assert.AreEqual(expectedOwner, WindowManager.ModalWindows[i].Owner);
                    Assert.AreEqual(true, WindowManager.ModalWindows[i].IsModal);
                    Assert.AreEqual(expectedIsDisabled, WindowManager.ModalWindows[i].IsDisabled);
                    Assert.AreEqual(true, WindowManager.ModalWindows[i].IsShown);
                    Assert.AreEqual(false, WindowManager.ModalWindows[i].WindowClosed.Task.IsCompleted);
                }
            }
            if (mainWindow != null)
            {
                Assert.NotNull(WindowManager.MainWindow);
                Assert.AreEqual(mainWindow, WindowManager.MainWindow.Window);
                Assert.AreEqual(null, WindowManager.MainWindow.Owner);
                Assert.AreEqual(null, WindowManager.MainWindow.Window.Owner);
                Assert.AreEqual(true, WindowManager.MainWindow.IsModal);
                Assert.AreEqual(WindowManager.ModalWindows.Count > 0, WindowManager.MainWindow.IsDisabled);
                Assert.AreEqual(true, WindowManager.MainWindow.IsShown);
                Assert.AreEqual(false, WindowManager.MainWindow.WindowClosed.Task.IsCompleted);
            }
            else
            {
                Assert.Null(WindowManager.MainWindow);
            }
        }

        public static void AssertWindowClosed(WindowInfo window)
        {
            Assert.AreEqual(null, window.Owner);
            //Assert.AreEqual(false, window.IsModal);
            Assert.AreEqual(false, window.IsDisabled);
            Assert.AreEqual(false, window.IsShown);
            Assert.AreEqual(true, window.WindowClosed.Task.IsCompleted);
        }
    }
}
