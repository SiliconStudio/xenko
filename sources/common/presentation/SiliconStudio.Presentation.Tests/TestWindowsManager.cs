using System;
using System.Threading;
using System.Threading.Tasks;
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
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async Task TestOpenCloseWindow()
        {
            LoggerResult loggerResult;
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new StandardWindow());

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
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async Task TestMainWindowThenModalBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new StandardWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window));

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened(modalWindow);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow));
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
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async Task TestMainWindowThenModalBoxCloseMain()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBoxCloseMain);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new StandardWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window));

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
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
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async Task TestSameModalTwice()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBoxCloseMain);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindowOpened = WindowManagerHelper.NextModalWindowOpened(modalWindow);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(null, modalWindow));

                // Try to open it again without having closed it
                Assert.Throws<InvalidOperationException>(() => dispatcher.Invoke(() => WindowManager.ShowModal(modalWindow)));
            }
            Assert.AreEqual(false, loggerResult.HasErrors);
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async Task TestMainWindowThenTwoModalBoxes()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenTwoModalBoxes);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new StandardWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a first modal window
                var modalWindow1 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindow1Opened = WindowManagerHelper.NextModalWindowOpened(modalWindow1);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow1));
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Opened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow1);
                });

                // Open a second modal window
                var modalWindow2 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
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
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async Task TestMainWindowThenTwoModalBoxesReverseClose()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenTwoModalBoxesReverseClose);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new StandardWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a first modal window
                var modalWindow1 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindow1Opened = WindowManagerHelper.NextModalWindowOpened(modalWindow1);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow1));
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Opened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow1);
                });

                // Open a second modal window
                var modalWindow2 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
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
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async Task TestMainWindowThenModalBoxThenBackgroundModal()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBoxThenBackgroundModal);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new StandardWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a first modal window
                var modalWindow1 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindow1Opened = WindowManagerHelper.NextModalWindowOpened(modalWindow1);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow1));
                await WindowManagerHelper.TaskWithTimeout(modalWindow1Opened);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window, modalWindow1);
                });

                // Open a second modal window in background
                var modalWindow2 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
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
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async Task TestMainWindowThenMessageBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenMessageBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new StandardWindow());

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
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async Task TestMainWindowThenModalBoxThenMessageBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBoxThenMessageBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new StandardWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() =>
                {
                    WindowManagerHelper.AssertWindowsStatus(window);
                });

                // Open a modal window
                var modalWindow = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
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
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async Task TestMainWindowThenMessageBoxThenBackgroundModalBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenMessageBoxThenBackgroundModalBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new StandardWindow());

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
                var modalWindow = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
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
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async Task TestMainWindowThenMessageBoxThenBackgroundModalBoxReverseClose()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenMessageBoxThenBackgroundModalBoxReverseClose);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new StandardWindow());

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
                var modalWindow = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
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
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async Task TestModalBoxThenCloseThenModalBox()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                // Open a modal window
                var modalWindow1 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
                var modalWindowOpened1 = WindowManagerHelper.NextModalWindowOpened(modalWindow1);
                dispatcher.InvokeAsync(() => WindowManager.ShowModal(modalWindow1));
                await WindowManagerHelper.TaskWithTimeout(modalWindowOpened1);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(null, modalWindow1));

                // Close the modal window and open another one immediately
                var modalWindow2 = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
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
            dispatcher.InvokeShutdown();
        }

        [Test, RequiresSTA]
        public async Task TestMainWindowThenModalBoxClosedBeforeShown()
        {
            LoggerResult loggerResult;
            const string messageBoxName = nameof(TestMainWindowThenModalBox);
            var dispatcher = await WindowManagerHelper.CreateUIThread();
            using (WindowManagerHelper.InitWindowManager(dispatcher, out loggerResult))
            {
                var window = dispatcher.Invoke(() => new StandardWindow());

                // Open the main window
                var shown = WindowManagerHelper.NextMainWindowChanged(window);
                dispatcher.Invoke(() => WindowManager.ShowMainWindow(window));
                await WindowManagerHelper.TaskWithTimeout(shown);
                dispatcher.Invoke(() => WindowManagerHelper.AssertWindowsStatus(window));

                // Open a modal window and close it before it has a chance to be shown
                var modalWindow = dispatcher.Invoke(() => new StandardWindow { Title = messageBoxName });
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
            dispatcher.InvokeShutdown();
        }
    }
}
