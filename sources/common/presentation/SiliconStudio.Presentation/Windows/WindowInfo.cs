using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Windows
{
    /// <summary>
    /// A container object for windows and their related information.
    /// </summary>
    public class WindowInfo : IEquatable<WindowInfo>, IEquatable<Window>, IEquatable<IntPtr>
    {
        private IntPtr hwnd;
        private bool isShown;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowInfo"/> class.
        /// </summary>
        /// <param name="window">The window represented by this object.</param>
        public WindowInfo(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            Window = window;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowInfo"/> class.
        /// </summary>
        /// <param name="hwnd">The hwnd of the window represented by this object.</param>
        internal WindowInfo(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) throw new ArgumentException(@"The hwnd cannot be null", nameof(hwnd));
            var window = FromHwnd(hwnd);
            Window = window;
            if (window == null)
                this.hwnd = hwnd;
        }

        /// <summary>
        /// Gets the <see cref="Window"/> represented by this object, if available.
        /// </summary>
        public Window Window { get; internal set; }

        /// <summary>
        /// Gets the hwnd of the window represented by this object, if available.
        /// </summary>
        public IntPtr Hwnd => hwnd == IntPtr.Zero && Window != null ? ToHwnd(Window) : hwnd;

        /// <summary>
        /// Gets whether the corresponding window is currently disabled.
        /// </summary>
        public bool IsDisabled { get { return HwndHelper.IsDisabled(Hwnd); } internal set { HwndHelper.SetDisabled(Hwnd, value); } }

        /// <summary>
        /// Gets whether the corresponding window is currently shown.
        /// </summary>
        public bool IsShown
        {
            get
            {
                return isShown;
            }
            internal set
            {
                isShown = value;
                ForceUpdateHwnd();
            }
        }

        /// <summary>
        /// Gets whether the corresponding window is currently modal.
        /// </summary>
        /// <remarks>
        /// This methods is heuristic, since there is no absolute flag under Windows indicating whether
        /// a window is modal. This method might need to be adjusted depending on the use cases.
        /// </remarks>
        public bool IsModal
        {
            get
            {
                if (Hwnd == IntPtr.Zero)
                    return false;

                if (HwndHelper.HasExStyleFlag(Hwnd, NativeHelper.WS_EX_TOOLWINDOW))
                    return false;

                var owner = Owner;
                return owner == null || owner.IsModal && owner.IsDisabled;
            }
        }

        /// <summary>
        /// Gets whether the corresponding window is currently visible
        /// </summary>
        public bool IsVisible => HwndHelper.HasStyleFlag(Hwnd, NativeHelper.WS_VISIBLE);

        /// <summary>
        /// Gets the owner of this window.
        /// </summary>
        public WindowInfo Owner
        {
            get
            {
                if (!IsShown)
                    return null;

                if (Window != null)
                    return WindowManager.Find(ToHwnd(Window.Owner));

                var owner = HwndHelper.GetOwner(Hwnd);
                return owner != IntPtr.Zero ? (WindowManager.Find(owner) ?? new WindowInfo(owner)) : null;
            }
            internal set
            {
                if (value == Owner)
                    return;

                //if (Window == null)
                //    throw new NotSupportedException("Cannot change the owner of this window because it is not a WPF window.");

                //if (value != null && value.Window == null)
                //    throw new NotSupportedException("Cannot change the owner of this window because the new owner is not a WPF window.");

                //if (ReferenceEquals(value?.Window, Window))
                //    throw new NotSupportedException("Cannot set a window to be its own owner.");

                if (Window != null)
                {
                    if (value?.Window == null)
                    {
                        Window.Owner = null;
                        if (value != null)
                        {
                            HwndHelper.SetOwner(Hwnd, value.Hwnd);
                        }
                    }
                    else
                    {
                        Window.Owner = value.Window;
                    }
                }
                else
                {
                    HwndHelper.SetOwner(Hwnd, value.Hwnd);
                }

                //Window.Owner = value?.Window;

                //// This code does not work unfortunately.
                //var ownerHwnd = value?.Hwnd ?? IntPtr.Zero;
                //HwndHelper.SetOwner(Hwnd, ownerHwnd);
            }
        }

        internal TaskCompletionSource<int> WindowClosed { get; } = new TaskCompletionSource<int>();

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var windowInfo = obj as WindowInfo;
            return windowInfo != null && Equals(windowInfo);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = hwnd.GetHashCode();
                hashCode = (hashCode*397) ^ isShown.GetHashCode();
                hashCode = (hashCode*397) ^ (Window?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        internal void ForceUpdateHwnd()
        {
            if (Window != null)
                hwnd = ToHwnd(Window);
        }

        /// <inheritdoc/>
        public static bool operator ==(WindowInfo left, WindowInfo right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc/>
        public static bool operator !=(WindowInfo left, WindowInfo right)
        {
            return !Equals(left, right);
        }

        /// <inheritdoc/>
        public bool Equals(WindowInfo other)
        {
            return Equals(other.Window) && Equals(other.Hwnd);
        }

        /// <inheritdoc/>
        public bool Equals(Window other)
        {
            return Equals(Window, other);
        }

        /// <inheritdoc/>
        public bool Equals(IntPtr other)
        {
            return Equals(Hwnd, other);
        }

        internal static IntPtr ToHwnd(Window window)
        {
            return window != null ? new WindowInteropHelper(window).Handle : IntPtr.Zero;
        }

        internal static Window FromHwnd(IntPtr hwnd)
        {
            return hwnd != IntPtr.Zero ? HwndSource.FromHwnd(hwnd)?.RootVisual as Window : null;
        }
    }
}
