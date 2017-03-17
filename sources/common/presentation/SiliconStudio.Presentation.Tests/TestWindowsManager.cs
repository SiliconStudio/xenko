using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using NUnit.Framework;
using NUnitAsync;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Presentation.Tests.WPF;
using SiliconStudio.Presentation.Windows;
using MessageBox = System.Windows.MessageBox;

namespace SiliconStudio.Presentation.Tests
{
    /// <summary>
    /// Test class for the <see cref="WindowManager"/>.
    /// </summary>
    /// <remarks>This class uses a monitor to run sequencially.</remarks>
    [TestFixture]
    public class TestWindowManager2
    {
        private static readonly object LockObj = new object();
        private StaSynchronizationContext syncContext;

        [SetUp]
        protected virtual void Setup()
        {
            Monitor.Enter(LockObj);
            syncContext = new StaSynchronizationContext();
        }

        [TearDown]
        protected virtual void TearDown()
        {
            syncContext.Dispose();
            syncContext = null;
            Monitor.Exit(LockObj);
        }

        [Test]
        public async Task TestInitDistroy()
        {
            LoggerResult loggerResult;
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
            }
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        public enum Step
        {
            ShowMain,
            ShowModal,
            ShowBlocking,
            HideMain,
            HideModal,
            HideBlocking,
        }

        [TestCase(Step.ShowMain, Step.HideMain, Step.ShowModal, Step.HideModal, Step.ShowBlocking, Step.HideBlocking, TestName = "ShowMain, HideMain, ShowModal, HideModal, ShowBlocking, HideBlocking")]
        [TestCase(Step.ShowMain, Step.HideMain, Step.ShowBlocking, Step.HideBlocking, Step.ShowModal, Step.HideModal, TestName = "ShowMain, HideMain, ShowBlocking, HideBlocking, ShowModal, HideModal")]
        [TestCase(Step.ShowMain, Step.HideMain, Step.ShowBlocking, Step.ShowModal, Step.HideBlocking, Step.HideModal, TestName = "ShowMain, HideMain, ShowBlocking, ShowModal, HideBlocking, HideModal")]
        [TestCase(Step.ShowMain, Step.HideMain, Step.ShowBlocking, Step.ShowModal, Step.HideModal, Step.HideBlocking, TestName = "ShowMain, HideMain, ShowBlocking, ShowModal, HideModal, HideBlocking")]
        [TestCase(Step.ShowMain, Step.HideMain, Step.ShowModal, Step.ShowBlocking, Step.HideModal, Step.HideBlocking, TestName = "ShowMain, HideMain, ShowModal, ShowBlocking, HideModal, HideBlocking")]
        [TestCase(Step.ShowMain, Step.HideMain, Step.ShowModal, Step.ShowBlocking, Step.HideBlocking, Step.HideModal, TestName = "ShowMain, HideMain, ShowModal, ShowBlocking, HideBlocking, HideModal")]
        [TestCase(Step.ShowModal, Step.HideModal, Step.ShowMain, Step.HideMain, Step.ShowBlocking, Step.HideBlocking, TestName = "ShowModal, HideModal, ShowMain, HideMain, ShowBlocking, HideBlocking")]
        [TestCase(Step.ShowModal, Step.HideModal, Step.ShowBlocking, Step.HideBlocking, Step.ShowMain, Step.HideMain, TestName = "ShowModal, HideModal, ShowBlocking, HideBlocking, ShowMain, HideMain")]
        [TestCase(Step.ShowModal, Step.HideModal, Step.ShowBlocking, Step.ShowMain, Step.HideBlocking, Step.HideMain, TestName = "ShowModal, HideModal, ShowBlocking, ShowMain, HideBlocking, HideMain")]
        [TestCase(Step.ShowModal, Step.HideModal, Step.ShowBlocking, Step.ShowMain, Step.HideMain, TestName = "ShowModal, HideModal, ShowBlocking, ShowMain, HideMain")] // NOTE: in this case Blocking is closed by Main.
        [TestCase(Step.ShowModal, Step.HideModal, Step.ShowMain, Step.ShowBlocking, Step.HideMain, TestName = "ShowModal, HideModal, ShowMain, ShowBlocking, HideMain")] // NOTE: in this case Blocking is closed by Main.
        [TestCase(Step.ShowModal, Step.HideModal, Step.ShowMain, Step.ShowBlocking, Step.HideBlocking, Step.HideMain, TestName = "ShowModal, HideModal, ShowMain, ShowBlocking, HideBlocking, HideMain")]
        [TestCase(Step.ShowBlocking, Step.HideBlocking, Step.ShowModal, Step.HideModal, Step.ShowMain, Step.HideMain, TestName = "ShowBlocking, HideBlocking, ShowModal, HideModal, ShowMain, HideMain")]
        [TestCase(Step.ShowBlocking, Step.HideBlocking, Step.ShowMain, Step.HideMain, Step.ShowModal, Step.HideModal, TestName = "ShowBlocking, HideBlocking, ShowMain, HideMain, ShowModal, HideModal")]
        [TestCase(Step.ShowBlocking, Step.HideBlocking, Step.ShowMain, Step.ShowModal, Step.HideMain, Step.HideModal, TestName = "ShowBlocking, HideBlocking, ShowMain, ShowModal, HideMain, HideModal")]
        [TestCase(Step.ShowBlocking, Step.HideBlocking, Step.ShowMain, Step.ShowModal, Step.HideModal, Step.HideMain, TestName = "ShowBlocking, HideBlocking, ShowMain, ShowModal, HideModal, HideMain")]
        [TestCase(Step.ShowBlocking, Step.HideBlocking, Step.ShowModal, Step.ShowMain, Step.HideModal, Step.HideMain, TestName = "ShowBlocking, HideBlocking, ShowModal, ShowMain, HideModal, HideMain")]
        [TestCase(Step.ShowBlocking, Step.HideBlocking, Step.ShowModal, Step.ShowMain, Step.HideMain, Step.HideModal, TestName = "ShowBlocking, HideBlocking, ShowModal, ShowMain, HideMain, HideModal")]
        public async Task TestBlockingWindow(params Step[] steps)
        {
            TestWindow mainWindow = null;
            TestWindow modalWindow = null;
            TestWindow blockingWindow = null;
            LoggerResult loggerResult;
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                foreach (var step in steps)
                {
                    TestWindow window;
                    var stepCompleted = new TaskCompletionSource<int>();
                    switch (step)
                    {
                        case Step.ShowMain:
                            Assert.IsNull(mainWindow);
                            window = dispatcher.Invoke(() => new TestWindow());
                            window.Shown += (sender, e) => { mainWindow = window; stepCompleted.SetResult(0); };
                            dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                            break;
                        case Step.ShowModal:
                            Assert.IsNull(modalWindow);
                            window = dispatcher.Invoke(() => new TestWindow());
                            window.Shown += (sender, e) => { modalWindow = window; stepCompleted.SetResult(0); };
                            dispatcher.InvokeAsync(() => window.ShowDialog());
                            break;
                        case Step.ShowBlocking:
                            Assert.IsNull(blockingWindow);
                            window = dispatcher.Invoke(() => new TestWindow());
                            window.Shown += (sender, e) => { blockingWindow = window; stepCompleted.SetResult(0); };
                            dispatcher.Invoke(() => WindowManager.ShowBlockingWindow(window));
                            break;
                        case Step.HideMain:
                            Assert.IsNotNull(mainWindow);
                            window = mainWindow;
                            window.Closed += (sender, e) => { mainWindow = null; blockingWindow = null; stepCompleted.SetResult(0); };
                            dispatcher.Invoke(() => window.Close());
                            break;
                        case Step.HideModal:
                            Assert.IsNotNull(modalWindow);
                            window = modalWindow;
                            window.Closed += (sender, e) => { modalWindow = null; stepCompleted.SetResult(0); };
                            dispatcher.Invoke(() => window.Close());
                            break;
                        case Step.HideBlocking:
                            Assert.IsNotNull(blockingWindow);
                            window = blockingWindow;
                            window.Closed += (sender, e) => { blockingWindow = null; stepCompleted.SetResult(0); };
                            dispatcher.Invoke(() => window.Close());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    await stepCompleted.Task;
                    await dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                    dispatcher.Invoke(() => AssertStep(step, mainWindow, modalWindow, blockingWindow));
                }
            }
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        private void AssertStep(Step step, Window mainWindow, Window modalWindow, Window blockingWindow)
        {
            switch (step)
            {
                case Step.ShowMain:
                    Assert.IsNotNull(mainWindow, step.ToString());
                    break;
                case Step.ShowModal:
                    Assert.IsNotNull(modalWindow, step.ToString());
                    break;
                case Step.ShowBlocking:
                    Assert.IsNotNull(blockingWindow, step.ToString());
                    break;
                case Step.HideMain:
                    Assert.IsNull(mainWindow, step.ToString());
                    break;
                case Step.HideModal:
                    Assert.IsNull(modalWindow, step.ToString());
                    break;
                case Step.HideBlocking:
                    Assert.IsNull(blockingWindow, step.ToString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(step), step, null);
            }

            if (mainWindow != null)
            {
                var winInfo = WindowManager.MainWindow;
                Assert.NotNull(winInfo, step.ToString());
                Assert.AreEqual(mainWindow, winInfo.Window, step.ToString());                
                Assert.True(winInfo.IsModal, step.ToString()); // TODO: should return false;
                Assert.AreEqual(modalWindow != null || blockingWindow != null, winInfo.IsDisabled, step.ToString());
            }
            else
            {
                Assert.AreEqual(null, WindowManager.MainWindow, step.ToString());
            }
            if (modalWindow != null)
            {
                Assert.AreEqual(1, WindowManager.ModalWindows.Count, step.ToString());
                var winInfo = WindowManager.ModalWindows[0];
                Assert.AreEqual(modalWindow, winInfo.Window, step.ToString());
                Assert.True(winInfo.IsModal, step.ToString());
                Assert.False(winInfo.IsDisabled, step.ToString());
            }
            else
            {
                Assert.AreEqual(0, WindowManager.ModalWindows.Count, step.ToString());
            }
            if (blockingWindow != null)
            {
                Assert.AreEqual(1, WindowManager.BlockingWindows.Count, step.ToString());
                var winInfo = WindowManager.BlockingWindows[0];
                Assert.AreEqual(blockingWindow, winInfo.Window, step.ToString());
                Assert.False(winInfo.IsModal, step.ToString());
                Assert.AreEqual(modalWindow != null, winInfo.IsDisabled, step.ToString());
                Assert.AreEqual(mainWindow, winInfo.Owner?.Window, step.ToString());
            }
            else
            {
                Assert.AreEqual(0, WindowManager.BlockingWindows.Count, step.ToString());
            }
        }

        [Test]
        public async Task TestOpenCloseWindow()
        {
            LoggerResult loggerResult;
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window));

                // Close the main window
                var mainWindow = WindowManager.MainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestMainWindowThenModalBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window));

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened(modalWindow);
                dispatcher.InvokeAsync(() => modalWindow.ShowDialog());
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window, modalWindow));

                // Close the modal window
                var modalWindowInfo = WindowManager.ModalWindows[0];
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed(modalWindow);
                dispatcher.Invoke(() => modalWindow.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindowClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindowInfo);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.MainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestMainWindowThenStandaloneModalBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window));

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened(modalWindow);
                dispatcher.InvokeAsync(() => modalWindow.ShowDialog());
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window, modalWindow));

                // Close the modal window
                var modalWindowInfo = WindowManager.ModalWindows[0];
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed(modalWindow);
                dispatcher.Invoke(() => modalWindow.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindowClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindowInfo);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.MainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestMainWindowThenModalBoxCloseMain()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBoxCloseMain);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window));

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened(modalWindow);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window, modalWindow));

                // Close the main window - this should also close the modal window
                var mainWindow = WindowManager.MainWindow;
                var modalWindowInfo = WindowManager.ModalWindows[0];
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed(modalWindow);
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
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }
    }

        /// <summary>
        /// Test class for the <see cref="WindowManager"/>.
        /// </summary>
        /// <remarks>This class uses a monitor to run sequencially.</remarks>
        [TestFixture]
    public class TestWindowManager
    {
        private static readonly object LockObj = new object();
        private StaSynchronizationContext syncContext;

        [SetUp]
        protected virtual void Setup()
        {
            Monitor.Enter(LockObj);
            syncContext = new StaSynchronizationContext();
        }

        [TearDown]
        protected virtual void TearDown()
        {
            syncContext.Dispose();
            syncContext = null;
            Monitor.Exit(LockObj);
        }

        [Test]
        public async Task TestInitDistroy()
        {
            LoggerResult loggerResult;
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
            }
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestOpenCloseWindow()
        {
            LoggerResult loggerResult;
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window));

                // Close the main window
                var mainWindow = WindowManager.MainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestMainWindowThenModalBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window));

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened(modalWindow);
                dispatcher.InvokeAsync(() => modalWindow.ShowDialog());
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window, modalWindow));

                // Close the modal window
                var modalWindowInfo = WindowManager.ModalWindows[0];
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed(modalWindow);
                dispatcher.Invoke(() => modalWindow.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindowClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindowInfo);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.MainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestMainWindowThenStandaloneModalBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window));

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened(modalWindow);
                dispatcher.InvokeAsync(() => modalWindow.ShowDialog());
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window, modalWindow));

                // Close the modal window
                var modalWindowInfo = WindowManager.ModalWindows[0];
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed(modalWindow);
                dispatcher.Invoke(() => modalWindow.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindowClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindowInfo);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.MainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestMainWindowThenModalBoxCloseMain()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBoxCloseMain);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window));

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened(modalWindow);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window, modalWindow));

                // Close the main window - this should also close the modal window
                var mainWindow = WindowManager.MainWindow;
                var modalWindowInfo = WindowManager.ModalWindows[0];
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed(modalWindow);
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
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestSameModalTwice()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBoxCloseMain);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened(modalWindow);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(null, modalWindow));

                // Try to open it again without having closed it
                Assert.Throws<InvalidOperationException>(() => dispatcher.Invoke(() => WindowManager.ShowModal(modalWindow)));
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestMainWindowThenTwoModalBoxes()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenTwoModalBoxes);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a first modal window
                var modalWindow1 = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindow1Opened = WindowManagerHelper.NextModalWindowOpened(modalWindow1);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow1));
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Opened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow1);
                });

                // Open a second modal window
                var modalWindow2 = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindow2Opened = WindowManagerHelper.NextModalWindowOpened(modalWindow2);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow2));
                await WindowManagerHelper.TaskWithTimeout(modalWindow2Opened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow1, modalWindow2);
                });

                // Close the second modal window
                var modalWindow2Info = WindowManager.ModalWindows[1];
                var modalWindow2Closed = WindowManagerHelper.NextModalWindowClosed(modalWindow2);
                dispatcher.Invoke(() => modalWindow2.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindow2Closed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindow2Info);
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow1);
                });

                // Close the first modal window
                var modalWindow1Info = WindowManager.ModalWindows[0];
                var modalWindow1Closed = WindowManagerHelper.NextModalWindowClosed(modalWindow1);
                dispatcher.Invoke(() => modalWindow1.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Closed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindow1Info);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.MainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestMainWindowThenTwoModalBoxesReverseClose()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenTwoModalBoxesReverseClose);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a first modal window
                var modalWindow1 = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindow1Opened = WindowManagerHelper.NextModalWindowOpened(modalWindow1);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow1));
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Opened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow1);
                });

                // Open a second modal window
                var modalWindow2 = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindow2Opened = WindowManagerHelper.NextModalWindowOpened(modalWindow2);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow2));
                await WindowManagerHelper.TaskWithTimeout(modalWindow2Opened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow1, modalWindow2);
                });

                // Close the first modal window
                var modalWindow1Info = WindowManager.ModalWindows[0];
                var modalWindow1Closed = WindowManagerHelper.NextModalWindowClosed(modalWindow1);
                dispatcher.Invoke(() => modalWindow1.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Closed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindow1Info);
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow2);
                });

                await Task.Delay(100);

                // Close the second modal window
                var modalWindow2Info = WindowManager.ModalWindows[0];
                var modalWindow2Closed = WindowManagerHelper.NextModalWindowClosed(modalWindow2);
                dispatcher.Invoke(() => modalWindow2.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindow2Closed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindow2Info);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.MainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestMainWindowThenModalBoxThenBackgroundModal()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBoxThenBackgroundModal);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a first modal window
                var modalWindow1 = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindow1Opened = WindowManagerHelper.NextModalWindowOpened(modalWindow1);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow1));
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Opened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow1);
                });

                // Open a second modal window in background
                var modalWindow2 = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindow2Opened = WindowManagerHelper.NextModalWindowOpened(modalWindow2);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow2, WindowOwner.MainWindow));
                await WindowManagerHelper.TaskWithTimeout(modalWindow2Opened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow2, modalWindow1);
                });

                // Close the first modal window
                var modalWindow1Info = WindowManager.ModalWindows[1];
                var modalWindow1Closed = WindowManagerHelper.NextModalWindowClosed(modalWindow1);
                dispatcher.Invoke(() => modalWindow1.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Closed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindow1Info);
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow2);
                });

                // Close the second modal window
                var modalWindow2Info = WindowManager.ModalWindows[0];
                var modalWindow2Closed = WindowManagerHelper.NextModalWindowClosed(modalWindow2);
                dispatcher.Invoke(() => modalWindow2.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindow2Closed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindow2Info);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.MainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestMainWindowThenMessageBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenMessageBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a message box
                var messageBoxOpened = WindowManagerHelper.NextMessageBoxOpened();
                dispatcher.InvokeAsync(() => MessageBox.Show("Test", messageBoxName));
                await WindowManagerHelper.TaskWithTimeout(messageBoxOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, null);
                });

                // Close the message box
                var messageBoxInfo = WindowManager.ModalWindows[0];
                var messageBoxClosed = WindowManagerHelper.NextMessageBoxClosed();
                WindowManagerHelper.KillWindow(messageBoxInfo.Hwnd);
                await WindowManagerHelper.TaskWithTimeout(messageBoxClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(messageBoxInfo);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.MainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestMainWindowThenModalBoxThenMessageBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBoxThenMessageBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened(modalWindow);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow);
                });

                // Open a message box
                var messageBoxOpened = WindowManagerHelper.NextMessageBoxOpened();
                dispatcher.InvokeAsync(() => MessageBox.Show("Test", messageBoxName));
                await WindowManagerHelper.TaskWithTimeout(messageBoxOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow, null);
                });

                // Close the messageBox
                var messageBoxInfo = WindowManager.ModalWindows[1];
                var messageBoxClosed = WindowManagerHelper.NextMessageBoxClosed();
                WindowManagerHelper.KillWindow(messageBoxInfo.Hwnd);
                await WindowManagerHelper.TaskWithTimeout(messageBoxClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(messageBoxInfo);
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow);
                });

                // Close the modal window
                var modalWindowInfo = WindowManager.ModalWindows[0];
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed(modalWindow);
                dispatcher.Invoke(() => modalWindow.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindowClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindowInfo);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.MainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestMainWindowThenMessageBoxThenBackgroundModalBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenMessageBoxThenBackgroundModalBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a message box
                var messageBoxOpened = WindowManagerHelper.NextMessageBoxOpened();
                dispatcher.InvokeAsync(() => MessageBox.Show("Test", messageBoxName));
                await WindowManagerHelper.TaskWithTimeout(messageBoxOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, null);
                });

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened(modalWindow);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow, WindowOwner.MainWindow));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow, null);
                });

                // Close the messageBox
                var messageBoxInfo = WindowManager.ModalWindows[1];
                var messageBoxClosed = WindowManagerHelper.NextMessageBoxClosed();
                WindowManagerHelper.KillWindow(messageBoxInfo.Hwnd);
                await WindowManagerHelper.TaskWithTimeout(messageBoxClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(messageBoxInfo);
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow);
                });

                // Close the modal window
                var modalWindowInfo = WindowManager.ModalWindows[0];
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed(modalWindow);
                dispatcher.Invoke(() => modalWindow.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindowClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindowInfo);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.MainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestMainWindowThenMessageBoxThenBackgroundModalBoxReverseClose()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenMessageBoxThenBackgroundModalBoxReverseClose);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a message box
                var messageBoxOpened = WindowManagerHelper.NextMessageBoxOpened();
                dispatcher.InvokeAsync(() => MessageBox.Show("Test", messageBoxName));
                await WindowManagerHelper.TaskWithTimeout(messageBoxOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, null);
                });

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened(modalWindow);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow, WindowOwner.MainWindow));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow, null);
                });

                // Close the modal window
                var modalWindowInfo = WindowManager.ModalWindows[0];
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed(modalWindow);
                dispatcher.Invoke(() => modalWindow.Close());
                await WindowManagerHelper.TaskWithTimeout(modalWindowClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(modalWindowInfo);
                    WindowManagerHelper.AssertWindowsStatus(window, null);
                });

                // Close the messageBox
                var messageBoxInfo = WindowManager.ModalWindows[0];
                var messageBoxClosed = WindowManagerHelper.NextMessageBoxClosed();
                WindowManagerHelper.KillWindow(messageBoxInfo.Hwnd);
                await WindowManagerHelper.TaskWithTimeout(messageBoxClosed);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(messageBoxInfo);
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Close the main window
                var mainWindow = WindowManager.MainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestModalBoxThenCloseThenModalBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                // Open a modal window
                var modalWindow1 = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindowOpened1 = WindowManagerHelper.NextModalWindowOpened(modalWindow1);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow1));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened1);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(null, modalWindow1));

                // Close the modal window and open another one immediately
                var modalWindow2 = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                //var hidden = WindowManagerHelper.NextMainWindowChanged();
                var modalWindowOpened2 = WindowManagerHelper.NextModalWindowOpened(modalWindow2);
                dispatcher.Invoke(() =>
                {
                    modalWindow1.Close();
                    // Since we're in the same frame the dispatcher thread didn't had the opportunity to execute the WindowHidden method of WindowManager yet.
                    // We ensure that showing the next window does not throw.
                    Assert.DoesNotThrow(() => WindowManager.ShowModal(modalWindow2));
                });

                // Then we verify that the second window is properly initialized
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened2);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(null, modalWindow2));

                // Close the modal window
                dispatcher.Invoke(() => modalWindow2.Close());
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }

        [Test]
        public async Task TestMainWindowThenModalBoxClosedBeforeShown()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new TestWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window));

                // Open a modal window and close it before it has a chance to be shown
                var modalWindow = dispatcher.Invoke(() => new TestWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened(modalWindow);
                var modalWindowClosed = WindowManagerHelper.NextModalWindowClosed(modalWindow);
                dispatcher.Invoke(() =>
                {
                    WindowManager.ShowModal(modalWindow);
                    modalWindow.Close();
                });

                await WindowManagerHelper.TaskWithTimeout(modalWindowClosed);
                // The window never shown, this task should not be completed.
                Assert.False(modalWindowOpened.IsCompleted);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window));

                // Close the main window
                var mainWindow = WindowManager.MainWindow;
                var hidden = WindowManagerHelper.NextMainWindowChanged(null);
                dispatcher.Invoke(() => window.Close());
                await WindowManagerHelper.TaskWithTimeout(hidden);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowClosed(mainWindow);
                    WindowManagerHelper.AssertWindowsStatus(null);
                });
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            WindowManagerHelper.ShutdownUIThread(dispatcher);
        }
    }
}
