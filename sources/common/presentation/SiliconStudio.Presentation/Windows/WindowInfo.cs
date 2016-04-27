using System;
using System.Windows;
using System.Windows.Interop;

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
        internal WindowInfo(Window window)
        {
            Window = window;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowInfo"/> class.
        /// </summary>
        /// <param name="hwnd">The hwnd of the window represented by this object.</param>
        internal WindowInfo(IntPtr hwnd)
        {
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
                if (Window != null)
                    hwnd = ToHwnd(Window);
            }
        }

        /// <summary>
        /// Gets whether the corresponding window is currently modal.
        /// </summary>
        public bool IsModal
        {
            get
            {
                if (Hwnd == IntPtr.Zero)
                    return false;

                var owner = Owner;
                return owner == null || owner.IsModal && owner.IsDisabled;
            }
        }

        /// <summary>
        /// Gets the owner of this window.
        /// </summary>
        public WindowInfo Owner
        {
            get
            {
                var owner = HwndHelper.GetOwner(Hwnd);
                return owner != IntPtr.Zero ? new WindowInfo(owner) : null;
            }
            internal set
            {
                var ownerHwnd = value?.Hwnd ?? IntPtr.Zero;
                HwndHelper.SetOwner(Hwnd, ownerHwnd);
            }
        }

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
            return Window.Equals(other.Window) && Hwnd == other.Hwnd;
        }

        /// <inheritdoc/>
        public bool Equals(Window other)
        {
            return Window.Equals(other);
        }

        /// <inheritdoc/>
        public bool Equals(IntPtr other)
        {
            return Hwnd.Equals(other);
        }

        private static IntPtr ToHwnd(Window window)
        {
            return new WindowInteropHelper(window).Handle;
        }

        private static Window FromHwnd(IntPtr hwnd)
        {
            return HwndSource.FromHwnd(hwnd)?.RootVisual as Window;
        }
    }
}
