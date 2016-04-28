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
using MessageBox = System.Windows.MessageBox;

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
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.mainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
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
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.InvokeAsync(() => WindowManager.ShowTopModal(modalWindow));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow);
                });

                // Close the modal window
                var modalWindowInfo = WindowManager.modalWindows[0];
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed();
                dispatcher.Invoke(() => modalWindow.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindowClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindowInfo);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.mainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async void TestMainWindowThenModalBoxCloseMain()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBoxCloseMain);
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
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.InvokeAsync(() => WindowManager.ShowTopModal(modalWindow));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow);
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
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowClosed(modalWindowInfo);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async void TestMainWindowThenTwoModalBoxes()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenTwoModalBoxes);
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
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a first modal window
                var modalWindow1 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindow1Opened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.InvokeAsync(() => WindowManager.ShowTopModal(modalWindow1));
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Opened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow1);
                });

                // Open a second modal window
                var modalWindow2 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindow2Opened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.InvokeAsync(() => WindowManager.ShowTopModal(modalWindow2));
                await WindowManagerHelper.TaskWithTimeout(modalWindow2Opened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow1, modalWindow2);
                });

                // Close the second modal window
                var modalWindow2Info = WindowManager.modalWindows[1];
                var modalWindow2Closed = WindowManagerHelper.NextModalWindowClosed();
                dispatcher.Invoke(() => modalWindow2.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindow2Closed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindow2Info);
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow1);
                });

                // Close the first modal window
                var modalWindow1Info = WindowManager.modalWindows[0];
                var modalWindow1Closed = WindowManagerHelper.NextModalWindowClosed();
                dispatcher.Invoke(() => modalWindow1.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Closed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindow1Info);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.mainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async void TestMainWindowThenTwoModalBoxesReverseClose()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenTwoModalBoxesReverseClose);
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
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a first modal window
                var modalWindow1 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindow1Opened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.InvokeAsync(() => WindowManager.ShowTopModal(modalWindow1));
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Opened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow1);
                });

                // Open a second modal window
                var modalWindow2 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindow2Opened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.InvokeAsync(() => WindowManager.ShowTopModal(modalWindow2));
                await WindowManagerHelper.TaskWithTimeout(modalWindow2Opened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow1, modalWindow2);
                });

                // Close the first modal window
                var modalWindow1Info = WindowManager.modalWindows[0];
                var modalWindow1Closed = WindowManagerHelper.NextModalWindowClosed();
                dispatcher.Invoke(() => modalWindow1.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Closed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindow1Info);
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow2);
                });

                // Close the second modal window
                var modalWindow2Info = WindowManager.modalWindows[0];
                var modalWindow2Closed = WindowManagerHelper.NextModalWindowClosed();
                dispatcher.Invoke(() => modalWindow2.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindow2Closed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindow2Info);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.mainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async void TestMainWindowThenModalBoxThenBackgroundModal()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBoxThenBackgroundModal);
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
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a first modal window
                var modalWindow1 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindow1Opened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.InvokeAsync(() => WindowManager.ShowTopModal(modalWindow1));
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Opened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow1);
                });

                // Open a second modal window in background
                var modalWindow2 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindow2Opened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.InvokeAsync(() => WindowManager.ShowBackgroundModal(modalWindow2));
                await WindowManagerHelper.TaskWithTimeout(modalWindow2Opened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow2, modalWindow1);
                });

                // Close the first modal window
                var modalWindow1Info = WindowManager.modalWindows[1];
                var modalWindow1Closed = WindowManagerHelper.NextModalWindowClosed();
                dispatcher.Invoke(() => modalWindow1.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Closed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindow1Info);
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow2);
                });

                // Close the second modal window
                var modalWindow2Info = WindowManager.modalWindows[0];
                var modalWindow2Closed = WindowManagerHelper.NextModalWindowClosed();
                dispatcher.Invoke(() => modalWindow2.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindow2Closed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindow2Info);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.mainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async void TestMainWindowThenMessageBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenMessageBox);
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
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a message box
                var messageBoxOpened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.InvokeAsync(() => MessageBox.Show("Test", messageBoxName));
                await WindowManagerHelper.TaskWithTimeout(messageBoxOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, null);
                });

                // Close the message box
                var messageBoxInfo = WindowManager.modalWindows[0];
                var messageBoxClosed = WindowManagerHelper.NextModalWindowClosed();
                WindowManagerHelper.KillWindow(messageBoxInfo.Hwnd);
                await WindowManagerHelper.TaskWithTimeout(messageBoxClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(messageBoxInfo);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.mainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async void TestMainWindowThenModalBoxThenMessageBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBoxThenMessageBox);
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
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a modal window
                var modalWindows = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.InvokeAsync(() => WindowManager.ShowTopModal(modalWindows));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindows);
                });

                // Open a message box
                var messageBoxOpened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.InvokeAsync(() => MessageBox.Show("Test", messageBoxName));
                await WindowManagerHelper.TaskWithTimeout(messageBoxOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindows, null);
                });

                // Close the messageBox
                var messageBoxInfo = WindowManager.modalWindows[1];
                var messageBoxClosed = WindowManagerHelper.NextModalWindowClosed();
                WindowManagerHelper.KillWindow(messageBoxInfo.Hwnd);
                await WindowManagerHelper.TaskWithTimeout(messageBoxClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(messageBoxInfo);
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindows);
                });

                // Close the modal window
                var modalWindowInfo = WindowManager.modalWindows[0];
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed();
                dispatcher.Invoke(() => modalWindows.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindowClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindowInfo);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.mainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async void TestMainWindowThenMessageBoxThenBackgroundModalBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenMessageBoxThenBackgroundModalBox);
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
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a message box
                var messageBoxOpened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.InvokeAsync(() => MessageBox.Show("Test", messageBoxName));
                await WindowManagerHelper.TaskWithTimeout(messageBoxOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, null);
                });

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.InvokeAsync(() => WindowManager.ShowBackgroundModal(modalWindow));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow, null);
                });

                // Close the messageBox
                var messageBoxInfo = WindowManager.modalWindows[1];
                var messageBoxClosed = WindowManagerHelper.NextModalWindowClosed();
                WindowManagerHelper.KillWindow(messageBoxInfo.Hwnd);
                await WindowManagerHelper.TaskWithTimeout(messageBoxClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(messageBoxInfo);
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow);
                });

                // Close the modal window
                var modalWindowInfo = WindowManager.modalWindows[0];
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed();
                dispatcher.Invoke(() => modalWindow.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindowClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindowInfo);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.mainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async void TestMainWindowThenMessageBoxThenBackgroundModalBoxReverseClose()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenMessageBoxThenBackgroundModalBoxReverseClose);
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
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a message box
                var messageBoxOpened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.InvokeAsync(() => MessageBox.Show("Test", messageBoxName));
                await WindowManagerHelper.TaskWithTimeout(messageBoxOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, null);
                });

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened();
                dispatcher.InvokeAsync(() => WindowManager.ShowBackgroundModal(modalWindow));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow, null);
                });

                // Close the modal window
                var modalWindowInfo = WindowManager.modalWindows[0];
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed();
                dispatcher.Invoke(() => modalWindow.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindowClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindowInfo);
                    WindowManagerHelper.AssertWindowsStatus(window, null);
                });

                // Close the messageBox
                var messageBoxInfo = WindowManager.modalWindows[0];
                var messageBoxClosed = WindowManagerHelper.NextModalWindowClosed();
                WindowManagerHelper.KillWindow(messageBoxInfo.Hwnd);
                await WindowManagerHelper.TaskWithTimeout(messageBoxClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(messageBoxInfo);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.mainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged();
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            dispatcher.InvokeShutdown();
        }


    }
}
