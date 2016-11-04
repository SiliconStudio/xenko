using System;
using System.Windows;
using System.Windows.Interop;

namespace SiliconStudio.Presentation.Extensions
{
    public class ClipboardTextChangedEventArgs : EventArgs
    {
        public ClipboardTextChangedEventArgs(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }

    public static class ClipboardMonitor
    {
        private static IntPtr hWndNextViewer;

        public static event EventHandler<ClipboardTextChangedEventArgs> ClipboardTextChanged;

        public static void RegisterListener(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            window.Dispatcher.Invoke(() =>
            {
                var hWndSource = GetHwndSource(window);
                if (hWndSource == null)
                    return;

                // start processing window messages
                hWndSource.AddHook(WinProc);
                // set the window as a viewer
                hWndNextViewer = NativeHelper.SetClipboardViewer(hWndSource.Handle);
            });
        }

        public static void UnregisterListener(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            window.Dispatcher.Invoke(() =>
            {
                var hWndSource = GetHwndSource(window);
                if (hWndSource == null)
                    return;

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

        private static void OnClipboardContentChanged()
        {
            if (Clipboard.ContainsText())
            {
                ClipboardTextChanged?.Invoke(null, new ClipboardTextChangedEventArgs(Clipboard.GetText()));
            }
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
                    OnClipboardContentChanged();
                    // pass the message to the next viewer. 
                    NativeHelper.SendMessage(hWndNextViewer, msg, wParam, lParam);
                    break;
            }

            return IntPtr.Zero;
        }
    }
}
