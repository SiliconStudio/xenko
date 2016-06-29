using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        // TODO: this list should be completely external
        private static readonly string[] DebugWindowTypeNames =
        {
            // WPF adorners introduced in Visual Studio 2015 Update 2
            "Microsoft.XamlDiagnostics.WpfTap",
            // WPF Inspector
            "ChristianMoser.WpfInspector",
            // Snoop
            "Snoop.SnoopUI",
        };

        private static readonly List<WindowInfo> ModalWindowsList = new List<WindowInfo>();
        private static readonly HashSet<WindowInfo> AllWindowsList = new HashSet<WindowInfo>();

        // This must remains a field to prevent garbage collection!
        private static NativeHelper.WinEventDelegate winEventProc;
        private static IntPtr hook;
        private static Dispatcher dispatcher;
        private static bool initialized;

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
            hook = NativeHelper.SetWinEventHook(NativeHelper.EVENT_OBJECT_SHOW, NativeHelper.EVENT_OBJECT_HIDE, IntPtr.Zero, winEventProc, processId, 0, NativeHelper.WINEVENT_OUTOFCONTEXT);
            if (hook == IntPtr.Zero)
                throw new InvalidOperationException("Unable to initialize the window manager.");

            Logger.Info($"{nameof(WindowManager)} initialized");
        }

#if DEBUG // Use a logger result for debugging
        public static Logger Logger { get; } = new LoggerResult();
#else
        public static Logger Logger { get; } = GlobalLogger.GetLogger(nameof(WindowManager));
#endif

        public static WindowInfo MainWindow { get; private set; }

        public static IReadOnlyList<WindowInfo> ModalWindows => ModalWindowsList;

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
            MainWindow = null;
            AllWindowsList.Clear();
            ModalWindowsList.Clear();

            Logger.Info($"{nameof(WindowManager)} disposed");
        }

        public static void ShowMainWindow(Window window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));
            CheckDispatcher();

            if (MainWindow != null)
            {
                var message = "This application already has a main window.";
                Logger.Error(message);
                throw new InvalidOperationException(message);
            }
            Logger.Info($"Main window showing. ({window})");

            MainWindow = new WindowInfo(window);
            AllWindowsList.Add(MainWindow);

            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Show();
        }

        public static Task ShowModal(Window window, WindowOwner windowOwner = WindowOwner.LastModal, WindowInitialPosition position = WindowInitialPosition.CenterOwner)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            CheckDispatcher();

            var windowInfo = new WindowInfo(window);
            if (ModalWindowsList.Contains(windowInfo))
                throw new InvalidOperationException("This window has already been shown as modal.");

            var owner = FindNextOwner(windowOwner);

            window.Owner = owner?.Window;
            SetStartupLocation(window, owner, position);

            // Set the owner now so the window can be recognized as modal when shown
            if (owner != null)
            {
                owner.IsDisabled = true;
            }

            AllWindowsList.Add(windowInfo);

            switch (windowOwner)
            {
                case WindowOwner.LastModal:
                    ModalWindowsList.Add(windowInfo);
                    break;
                case WindowOwner.MainWindow:
                    ModalWindowsList.Insert(0, windowInfo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(windowOwner), windowOwner, null);
            }

            // Update the hwnd on load in case the window is closed before being shown
            // We will receive EVENT_OBJECT_HIDE but not EVENT_OBJECT_SHOW in this case.
            window.Loaded += (sender, e) => windowInfo.ForceUpdateHwnd();

            Logger.Info($"Modal window showing. ({window})");
            window.Show();
            return windowInfo.WindowClosed.Task;
        }

        public static void ShowNonModal(Window window, WindowOwner windowOwner = WindowOwner.MainWindow, WindowInitialPosition position = WindowInitialPosition.CenterOwner)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            CheckDispatcher();

            var owner = FindNextOwner(windowOwner);

            window.Owner = owner?.Window;
            SetStartupLocation(window, owner, position);

            var windowInfo = new WindowInfo(window);
            AllWindowsList.Add(windowInfo);

            Logger.Info($"Non-modal window showing. ({window})");
            window.Show();
        }

        private static void SetStartupLocation(Window window, WindowInfo owner, WindowInitialPosition position)
        {
            switch (position)
            {
                case WindowInitialPosition.CenterOwner:
                    window.WindowStartupLocation = owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen;
                    break;
                case WindowInitialPosition.CenterScreen:
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    break;
                case WindowInitialPosition.MouseCursor:
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                    window.Loaded += PositionWindowToMouseCursor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }
        }

        private static void PositionWindowToMouseCursor(object sender, RoutedEventArgs e)
        {
            var window = (Window)sender;
            NativeHelper.POINT mousePosition;
            NativeHelper.GetCursorPos(out mousePosition);
            var monitor = WindowHelper.GetMonitorInfo(WindowInfo.ToHwnd(window));
            if (monitor != null)
            {
                bool expandRight = monitor.rcWork.Right > mousePosition.X + window.ActualWidth;
                bool expandBottom = monitor.rcWork.Bottom > mousePosition.Y + window.ActualHeight;
                window.Left = expandRight ? mousePosition.X : mousePosition.X - window.ActualWidth;
                window.Top = expandBottom ? mousePosition.Y : mousePosition.Y - window.ActualHeight;
            }

            window.Loaded -= PositionWindowToMouseCursor;
        }

        private static WindowInfo FindNextOwner(WindowOwner owner)
        {
            switch (owner)
            {
                case WindowOwner.LastModal:
                    // Skip non-visible window, they might be in the process of being closed.
                    return ModalWindows.FirstOrDefault(x => x.Hwnd == IntPtr.Zero || x.IsVisible) ?? MainWindow;
                case WindowOwner.MainWindow:
                    return MainWindow;
                default:
                    throw new ArgumentOutOfRangeException(nameof(owner), owner, null);
            }
        }

        private static void CheckDispatcher()
        {
            if (dispatcher.Thread != Thread.CurrentThread)
            {
                const string message = "This method must be invoked from the dispatcher thread";
                Logger.Error(message);
                throw new InvalidOperationException(message);
            }
        }

        private static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd == IntPtr.Zero)
                return;

            var rootHwnd = NativeHelper.GetAncestor(hwnd, NativeHelper.GetAncestorFlags.GetRoot);
            if (rootHwnd != IntPtr.Zero && rootHwnd != hwnd)
            {
                Logger.Debug($"Discarding non-root window ({hwnd}) - root: ({NativeHelper.GetAncestor(hwnd, NativeHelper.GetAncestorFlags.GetRoot)})");
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
            if (!HwndHelper.HasStyleFlag(hwnd, NativeHelper.WS_VISIBLE))
            {
                Logger.Debug($"Discarding non-visible window ({hwnd})");
                return;
            }

            Logger.Verbose($"Processing newly shown window ({hwnd})...");
            var windowInfo = Find(hwnd);
            if (windowInfo == null)
            {
                windowInfo = new WindowInfo(hwnd);

                if (Debugger.IsAttached)
                {
                    foreach (var debugWindowTypeName in DebugWindowTypeNames)
                    {
                        if (windowInfo.Window?.GetType().FullName.StartsWith(debugWindowTypeName) ?? false)
                        {
                            Logger.Debug($"Discarding debug/diagnostics window '{windowInfo.Window.GetType().FullName}' ({hwnd})");
                            return;
                        }
                    }
                }

                AllWindowsList.Add(windowInfo);
            }
            windowInfo.IsShown = true;

            if (windowInfo == MainWindow)
            {
                Logger.Info("Main window shown.");
                MainWindowChanged?.Invoke(null, new WindowManagerEventArgs(MainWindow));
            }
            else
            {
                if (windowInfo.IsModal)
                {
                    // If this window has not been shown using a WindowManager method, add it as a top-level modal window
                    if (!ModalWindows.Any(x => x.Equals(windowInfo)))
                    {
                        var lastModal = ModalWindows.LastOrDefault() ?? MainWindow;
                        if (lastModal != null)
                        {
                            windowInfo.Owner = lastModal;
                            lastModal.IsDisabled = true;
                        }
                        ModalWindowsList.Add(windowInfo);
                        Logger.Info($"Modal window shown. (standalone) ({hwnd})");
                    }
                    else
                    {
                        var index = ModalWindowsList.IndexOf(windowInfo);
                        var childModal = index < ModalWindows.Count - 1 ? ModalWindows[index + 1] : null;
                        var parentModal = index > 0 ? ModalWindows[index - 1] : MainWindow;
                        if (childModal != null)
                        {
                            childModal.Owner = windowInfo;
                            windowInfo.IsDisabled = true;
                            // We're placing another window on top of us, let's activate it so it comes to the foreground!
                            if (childModal.Hwnd != IntPtr.Zero)
                                NativeHelper.SetActiveWindow(childModal.Hwnd);
                        }
                        if (parentModal != null)
                        {
                            parentModal.IsDisabled = true;
                        }
                        Logger.Info($"Modal window shown. (with WindowManager) ({hwnd})");
                    }
                    ModalWindowOpened?.Invoke(null, new WindowManagerEventArgs(windowInfo));
                }
            }
        }

        private static void WindowHidden(IntPtr hwnd)
        {
            Logger.Verbose($"Processing newly hidden window ({hwnd})...");

            var windowInfo = Find(hwnd);
            if (windowInfo == null)
            {
                var message = $"This window was not handled by the {nameof(WindowManager)} ({hwnd})";
                Logger.Verbose(message);
                return;
            }

            windowInfo.IsShown = false;
            windowInfo.WindowClosed.SetResult(0);
            AllWindowsList.Remove(windowInfo);

            if (MainWindow != null && MainWindow.Equals(windowInfo))
            {
                Logger.Info($"Main window closed. ({hwnd})");
                MainWindow = null;
                MainWindowChanged?.Invoke(null, new WindowManagerEventArgs(MainWindow));
            }
            else
            {
                var index = ModalWindowsList.IndexOf(windowInfo);
                if (index >= 0)
                {
                    var childModal = index < ModalWindows.Count - 1 ? ModalWindows[index + 1] : null;
                    var parentModal = index > 0 ? ModalWindows[index - 1] : MainWindow;
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
                    ModalWindowsList.RemoveAt(index);

                    var nextWindow = ModalWindows.FirstOrDefault(x => x.Hwnd != IntPtr.Zero && x.IsVisible) ?? MainWindow;
                    if (nextWindow != null && nextWindow.Hwnd != IntPtr.Zero)
                        NativeHelper.SetActiveWindow(nextWindow.Hwnd);

                    Logger.Info($"Modal window closed. ({hwnd})");
                }
            }
        }

        internal static WindowInfo Find(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return null;

            var result = AllWindowsList.FirstOrDefault(x => x.Equals(hwnd));
            if (result != null)
                return result;

            var window = WindowInfo.FromHwnd(hwnd);
            return window != null ? AllWindowsList.FirstOrDefault(x => x.Equals(window)) : null;
        }
    }
}
