using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;

namespace SiliconStudio.Presentation.Extensions
{
    public static class ClipboardMonitor
    {
        private static IntPtr hWndNextViewer;
        private static readonly ConditionalWeakTable<Window, HwndSource> Listeners = new ConditionalWeakTable<Window, HwndSource>();

        public static event EventHandler<EventArgs> ClipboardTextChanged;

        public static void RegisterListener(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            HwndSource hWndSource;
            if (Listeners.TryGetValue(window, out hWndSource))
                throw new InvalidOperationException($"The given {window} is already registered as a clipboard listener.");

            hWndSource = GetHwndSource(window);
            if (hWndSource == null)
                return;

            Listeners.Add(window, hWndSource);

            window.Dispatcher.Invoke(() =>
            {
                // start processing window messages
                hWndSource.AddHook(WinProc);
                // set the window as a viewer
                hWndNextViewer = NativeHelper.SetClipboardViewer(hWndSource.Handle);
            });
        }

        public static void UnregisterListener(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            HwndSource hWndSource;
            if (!Listeners.TryGetValue(window, out hWndSource))
                throw new InvalidOperationException($"The given {window} is not registered as a clipboard listener.");

            window.Dispatcher.Invoke(() =>
            {
                // stop processing window messages
                hWndSource.RemoveHook(WinProc);
                // restore the chain
                NativeHelper.ChangeClipboardChain(hWndSource.Handle, hWndNextViewer);
            });
        }

        private static HwndSource GetHwndSource(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            return handle != IntPtr.Zero ? HwndSource.FromHwnd(handle) : null;
        }

        private static void OnClipboardContentChanged(IntPtr hwnd)
        {
            HwndSource.FromHwnd(hwnd)?.Dispatcher.InvokeAsync(() =>
            {
                if (Clipboard.ContainsText())
                {
                    ClipboardTextChanged?.Invoke(null, EventArgs.Empty);
                }
            });
        }

        private static IntPtr WinProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case NativeHelper.WM_CHANGECBCHAIN:
                    if (wParam == hWndNextViewer)
                    {
                        // clipboard viewer chain changed, need to fix it. 
                        hWndNextViewer = lParam;
                    }
                    else if (hWndNextViewer != IntPtr.Zero)
                    {
                        // pass the message to the next viewer. 
                        NativeHelper.SendMessage(hWndNextViewer, msg, wParam, lParam);
                    }
                    break;

                case NativeHelper.WM_DRAWCLIPBOARD:
                    // clipboard content changed 
                    OnClipboardContentChanged(hwnd);
                    // pass the message to the next viewer. 
                    NativeHelper.SendMessage(hWndNextViewer, msg, wParam, lParam);
                    break;
            }

            return IntPtr.Zero;
        }
    }
}
