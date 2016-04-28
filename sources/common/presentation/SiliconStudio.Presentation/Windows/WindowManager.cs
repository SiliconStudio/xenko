using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Presentation.Extensions;
using SiliconStudio.Presentation.View;

namespace SiliconStudio.Presentation.Windows
{
    /// <summary>
    /// A singleton class to manage the windows of an application and their relation to each other.
    /// </summary>
    public class WindowManager : IDisposable
    {
        private static readonly Logger logger = GlobalLogger.GetLogger(nameof(WindowManager));
        // This must remains a field to prevent garbage collection!
        private static NativeHelper.WinEventDelegate winEventProc;
        internal static WindowInfo mainWindow;
        private static IntPtr hook;
        internal static List<WindowInfo> modalWindows = new List<WindowInfo>();
        private static Dispatcher dispatcher;
        private static bool initialized;
        private static List<WindowInfo> allWindows = new List<WindowInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowManager"/> class.
        /// </summary>
        public WindowManager(Dispatcher dispatcher)
        {
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
            if (initialized) throw new InvalidOperationException("An instance of WindowManager is already existing.");

            initialized = true;
            winEventProc = WinEventProc;
            WindowManager.dispatcher = dispatcher;
            uint processId = (uint)Process.GetCurrentProcess().Id;
            hook = NativeHelper.SetWinEventHook(NativeHelper.EVENT_OBJECT_SHOW, NativeHelper.EVENT_OBJECT_PARENTCHANGE, IntPtr.Zero, winEventProc, processId, 0, NativeHelper.WINEVENT_OUTOFCONTEXT);
            if (hook == IntPtr.Zero)
                throw new InvalidOperationException("Unable to initialize the window manager.");
        }

        public Logger Logger => logger;

        /// <summary>
        /// Raised when the main window has changed.
        /// </summary>
        public static event EventHandler<WindowManagerEventArgs> MainWindowChanged;

        /// <summary>
        /// Raised when a modal window is opened.
        /// </summary>
        public static event EventHandler<WindowManagerEventArgs> ModalWindowOpened;

        /// <summary>
        /// Raised when a modal window is closed.
        /// </summary>
        public static event EventHandler<WindowManagerEventArgs> ModalWindowClosed;

        public void Dispose()
        {
            if (!NativeHelper.UnhookWinEvent(hook))
                throw new InvalidOperationException("An error occurred while disposing the window manager.");
            hook = IntPtr.Zero;

            winEventProc = null;
            initialized = false;
            dispatcher = null;
            mainWindow = null;
            allWindows.Clear();
            modalWindows.Clear();
        }

        public static void ShowTopModal(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            CheckDispatcher();
            var owner = modalWindows.FirstOrDefault() ?? mainWindow;
            window.Owner = owner?.Window;
            window.WindowStartupLocation = owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen;
            if (owner != null)
            {
                owner.IsDisabled = true;
            }
            var windowInfo = new WindowInfo(window);
            modalWindows.Add(windowInfo);
            allWindows.Add(windowInfo);
            window.Show();
        }

        //public static void ShowStandaloneModal(Action showDialog)
        //{
        //    if (showDialog == null) throw new ArgumentNullException(nameof(showDialog));
        //    CheckDispatcher();
        //    Dispatcher.PushFrame
        //    var owner = modalWindows.FirstOrDefault() ?? mainWindow;
        //    window.Owner = owner?.Window;
        //    window.WindowStartupLocation = owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen;
        //    if (owner != null)
        //    {
        //        owner.IsDisabled = true;
        //    }
        //    var windowInfo = new WindowInfo(window);
        //    modalWindows.Add(windowInfo);
        //    allWindows.Add(windowInfo);
        //    window.Show();
        //}

        public static void ShowBackgroundModal(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            CheckDispatcher();
            var owner = mainWindow;
            window.Owner = owner?.Window;
            window.WindowStartupLocation = owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen;
            if (owner != null)
            {
                owner.IsDisabled = true;
            }
            var windowInfo = new WindowInfo(window);
            modalWindows.Insert(0, windowInfo);
            allWindows.Add(windowInfo);
            window.Show();
        }

        public static void ShowMainWindow(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            CheckDispatcher();

            if (mainWindow != null)
            {
                var message = "This application already has a main window.";
                logger.Error(message);
                throw new InvalidOperationException(message);
            }
            logger.Info("Main window showing.");

            mainWindow = new WindowInfo(window);
            allWindows.Add(mainWindow);

            window.Show();
        }

        private static void CheckDispatcher()
        {
            if (dispatcher.Thread != Thread.CurrentThread)
            {
                const string message = "This method must be invoked from the dispatcher thread";
                logger.Error(message);
                throw new InvalidOperationException(message);
            }
        }

        private static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd == IntPtr.Zero)
                return;

            if (NativeHelper.GetAncestor(hwnd, NativeHelper.GetAncestorFlags.GetRoot) != hwnd)
            {
                logger.Debug("Discarding non-root window");
                return;
            }

            if (eventType == NativeHelper.EVENT_OBJECT_SHOW)
            {
                dispatcher.InvokeAsync(() => WindowShown(hwnd));
            }
            if (eventType == NativeHelper.EVENT_OBJECT_HIDE)
            {
                dispatcher.InvokeAsync(() => WindowHidden(hwnd));
            }
        }

        private static void WindowShown(IntPtr hwnd)
        {
            logger.Verbose("Processing newly shown window...");
            var windowInfo = Find(hwnd);
            if (windowInfo == null)
            {
                windowInfo = new WindowInfo(hwnd);
                allWindows.Add(windowInfo);
            }
            windowInfo.IsShown = true;

            if (windowInfo == mainWindow)
            {
                logger.Info("Main window shown.");
                MainWindowChanged?.Invoke(null, new WindowManagerEventArgs(mainWindow));
            }
            else
            {
                if (windowInfo.IsModal)
                {
                    // If this window has not been shown using a WindowManager method, add it as a top-level modal window
                    if (!modalWindows.Any(x => x.Equals(windowInfo)))
                    {
                        var lastModal = modalWindows.LastOrDefault() ?? mainWindow;
                        if (lastModal != null)
                        {
                            windowInfo.Owner = lastModal;
                            lastModal.IsDisabled = true;
                        }
                        modalWindows.Add(windowInfo);
                        logger.Info("Modal window shown. (standalone)");
                    }
                    else
                    {
                        var index = modalWindows.IndexOf(windowInfo);
                        var childModal = index < modalWindows.Count - 1 ? modalWindows[index + 1] : null;
                        var parentModal = index > 0 ? modalWindows[index - 1] : mainWindow;
                        if (childModal != null)
                        {
                            childModal.Owner = windowInfo;
                            windowInfo.IsDisabled = true;
                        }
                        if (parentModal != null)
                        {
                            parentModal.IsDisabled = true;
                        }
                        logger.Info("Modal window shown. (with WindowManager)");
                    }
                    ModalWindowOpened?.Invoke(null, new WindowManagerEventArgs(mainWindow));
                }
            }
        }

        private static void WindowHidden(IntPtr hwnd)
        {
            logger.Verbose("Processing newly hidden window...");

            var windowInfo = Find(hwnd);
            if (windowInfo == null)
            {
                var message = $"A window has been closed but was not handled by the {nameof(WindowManager)}.";
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            windowInfo.IsShown = false;
            allWindows.Remove(windowInfo);

            if (mainWindow != null && mainWindow.Equals(windowInfo))
            {
                logger.Info("Main window closed.");
                mainWindow = null;
                MainWindowChanged?.Invoke(null, new WindowManagerEventArgs(mainWindow));
            }
            else
            {
                var index = modalWindows.IndexOf(windowInfo);
                if (index >= 0)
                {
                    var childModal = index < modalWindows.Count - 1 ? modalWindows[index + 1] : null;
                    var parentModal = index > 0 ? modalWindows[index - 1] : mainWindow;
                    if (childModal != null)
                    {
                        childModal.Owner = parentModal;
                        if (parentModal != null)
                            parentModal.IsDisabled = true;
                    }
                    else if (parentModal != null)
                    {
                        parentModal.IsDisabled = false;
                    }
                    ModalWindowClosed?.Invoke(null, new WindowManagerEventArgs(windowInfo));
                    modalWindows.RemoveAt(index);
                    logger.Info("Modal window closed.");
                }
            }
        }

        public static void ShowNonModal(Window window)
        {
            if (dispatcher == null)
                dispatcher = window.Dispatcher;

        }

        internal static WindowInfo Find(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return null;

            var result = allWindows.FirstOrDefault(x => x.Equals(hwnd));
            if (result != null)
                return result;

            var window = WindowInfo.FromHwnd(hwnd);
            return window != null ? allWindows.FirstOrDefault(x => x.Equals(window)) : null;
        }
    }
}
