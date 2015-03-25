// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Controls
{
    /// <summary>
    /// An implementation of the <see cref="HwndHost"/> class adapted to embed a game engine viewport in a WPF application.
    /// </summary>
    public class GameEngineHwndHost : HwndHost
    {
        private readonly IntPtr childHandle;
        private readonly List<HwndSource> contextMenuSources = new List<HwndSource>();
        private int mouseMoveCount;
        private Point contextMenuPosition;
        private Rect lastBoundingBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameEngineHwndHost"/> class.
        /// </summary>
        /// <param name="childHandle">The hwnd of the child (hosted) window.</param>
        public GameEngineHwndHost(IntPtr childHandle)
        {
            this.childHandle = childHandle;
        }

        /// <inheritdoc/>
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            int style = NativeHelper.GetWindowLong(childHandle, NativeHelper.GWL_STYLE);
            // Removes Caption bar and the sizing border
            // Must be a child window to be hosted
            style |= NativeHelper.WS_CHILD;

            NativeHelper.SetWindowLong(childHandle, NativeHelper.GWL_STYLE, style);
            NativeHelper.ShowWindow(childHandle, NativeHelper.SW_HIDE);

            NativeHelper.SetParent(childHandle, hwndParent.Handle);

            var hwnd = new HandleRef(this, childHandle);
            return hwnd;
        }

        /// <inheritdoc/>
        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            NativeHelper.SetParent(childHandle, IntPtr.Zero);
            NativeHelper.DestroyWindow(hwnd.Handle);
        }

        protected override void OnWindowPositionChanged(Rect rcBoundingBox)
        {
            if (rcBoundingBox != lastBoundingBox)
            {
                lastBoundingBox = rcBoundingBox;
                base.OnWindowPositionChanged(rcBoundingBox);
            }
        }

        /// <summary>
        /// Forwards a message that comes from the hosted window to the WPF window. This method can be used for example to forward keyboard events.
        /// </summary>
        /// <param name="hwnd">The hwnd of the hosted window.</param>
        /// <param name="msg">The message identifier.</param>
        /// <param name="wParam">The word parameter of the message.</param>
        /// <param name="lParam">The long parameter of the message.</param>
        public void ForwardMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            DispatcherOperation task;
            switch (msg)
            {
                case NativeHelper.WM_RBUTTONDOWN:
                    mouseMoveCount = 0;
                    task = Dispatcher.BeginInvoke(new Action(() =>
                    {
                        RaiseMouseButtonEvent(Mouse.PreviewMouseDownEvent, MouseButton.Right);
                        RaiseMouseButtonEvent(Mouse.MouseDownEvent, MouseButton.Right);
                    }));
                    task.Wait(TimeSpan.FromSeconds(1.0f));
                    break;
                case NativeHelper.WM_RBUTTONUP:
                    task = Dispatcher.BeginInvoke(new Action(() =>
                        {
                            RaiseMouseButtonEvent(Mouse.PreviewMouseUpEvent, MouseButton.Right);
                            RaiseMouseButtonEvent(Mouse.MouseUpEvent, MouseButton.Right);
                        }));
                    task.Wait(TimeSpan.FromSeconds(1.0f));
                    break;
                case NativeHelper.WM_LBUTTONDOWN:
                    task = Dispatcher.BeginInvoke(new Action(() =>
                        {
                            RaiseMouseButtonEvent(Mouse.PreviewMouseDownEvent, MouseButton.Left);
                            RaiseMouseButtonEvent(Mouse.MouseDownEvent, MouseButton.Left);
                        }));
                    task.Wait(TimeSpan.FromSeconds(1.0f));
                    break;
                case NativeHelper.WM_LBUTTONUP:
                    task = Dispatcher.BeginInvoke(new Action(() =>
                        {
                            RaiseMouseButtonEvent(Mouse.PreviewMouseUpEvent, MouseButton.Left);
                            RaiseMouseButtonEvent(Mouse.MouseUpEvent, MouseButton.Left);
                        }));
                    task.Wait(TimeSpan.FromSeconds(1.0f));
                    break;
                case NativeHelper.WM_MOUSEMOVE:
                    ++mouseMoveCount;
                    break;
                case NativeHelper.WM_CONTEXTMENU:
                    // TODO: Tracking drag offset would be better, but might be difficult since we replace the mouse to its initial position each time it is moved.
                    if (mouseMoveCount < 3)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            DependencyObject dependencyObject = this;
                            while (dependencyObject != null)
                            {
                                var element = dependencyObject as FrameworkElement;
                                if (element != null && element.ContextMenu != null)
                                {
                                    element.Focus();
                                    // Data context will not be properly set if the popup is open this way, so let's set it ourselves
                                    element.ContextMenu.SetCurrentValue(DataContextProperty, element.DataContext);
                                    element.ContextMenu.IsOpen = true;
                                    var source = (HwndSource)PresentationSource.FromVisual(element.ContextMenu);
                                    if (source != null)
                                    {
                                        source.AddHook(ContextMenuWndProc);
                                        contextMenuPosition = Mouse.GetPosition(this);
                                        lock (contextMenuSources)
                                        {
                                            contextMenuSources.Add(source);
                                        }
                                    }
                                    break;
                                }
                                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
                            }
                        }));
                    }
                    break;
                default:
                    var parent = NativeHelper.GetParent(hwnd);
                    NativeHelper.PostMessage(parent, msg, wParam, lParam);
                    break;
            }
        }

        private void RaiseMouseButtonEvent(RoutedEvent routedEvent, MouseButton button)
        {
            RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button)
            {
                RoutedEvent = routedEvent,
                Source = this,
            });
        }

        private IntPtr ContextMenuWndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case NativeHelper.WM_LBUTTONDOWN:
                case NativeHelper.WM_RBUTTONDOWN:
                    // We need to change from the context menu coordinates to the HwndHost coordinates and re-encode lParam
                    var position = new Point(-(short)(lParam.ToInt64() & 0xFFFF), -((lParam.ToInt64() & 0xFFFF0000) >> 16));
                    var offset = contextMenuPosition - position;
                    lParam = new IntPtr((short)offset.X + (((short)offset.Y) << 16));
                    var threadId = NativeHelper.GetWindowThreadProcessId(childHandle, IntPtr.Zero);
                    NativeHelper.PostThreadMessage(threadId, msg, wParam, lParam);
                    break;
                case NativeHelper.WM_DESTROY:
                    lock (contextMenuSources)
                    {
                        var source = contextMenuSources.First(x => x.Handle == hwnd);
                        source.RemoveHook(ContextMenuWndProc);
                    }
                    break;
            }
            return IntPtr.Zero;
        }
    }
}
