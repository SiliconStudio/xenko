using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Presentation.Interop;

namespace SiliconStudio.Presentation.Windows
{
    /// <summary>
    /// A container object for windows and their related information.
    /// </summary>
    public class WindowInfo : IEquatable<WindowInfo>, IEquatable<Window>, IEquatable<IntPtr>
    {
        private IntPtr hwnd;
        private bool isShown;
        private static readonly FieldInfo ShowingAsDialogField;

        static WindowInfo()
        {
            ShowingAsDialogField = typeof(Window).GetField("_showingAsDialog", BindingFlags.NonPublic | BindingFlags.Instance);
            if (ShowingAsDialogField == null)
                throw new NotSupportedException("_showingAsDialog in the Window class. This program is running on an unidentified version of the .NET Framework.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowInfo"/> class.
        /// </summary>
        /// <param name="window">The window represented by this object.</param>
        public WindowInfo([NotNull] Window window)
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

                if (HwndHelper.HasStyleFlag(Hwnd, NativeHelper.WS_CHILD))
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
        [CanBeNull]
        public WindowInfo Owner
        {
            get
            {
                if (!IsShown)
                    return null;

                if (Window?.Owner != null)
                {
                    return WindowManager.Find(ToHwnd(Window.Owner));
                }

                var owner = HwndHelper.GetOwner(Hwnd);
                return owner != IntPtr.Zero ? (WindowManager.Find(owner) ?? new WindowInfo(owner)) : null;
            }
            internal set
            {
                if (value == Owner)
                    return;

                if (Window != null)
                {
                    var showingAsDialog = (bool)ShowingAsDialogField.GetValue(Window);
                    if (showingAsDialog)
                    {
                        // This is a workaround in case we are reparenting a window that was displayed using Window.ShowDialog().
                        // In this case, a private boolean field throws an exception if the owner of the window is changed.
                        // The reason seems to be because they didn't implement the logic of reparenting modal dialogs, which is
                        // what we are trying to implement here. Changing the Owner is a valid change if Window.Show() was used
                        // instead, so we assume this is a "safe hack".
                        ShowingAsDialogField.SetValue(Window, false);
                    }

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
                    if (showingAsDialog)
                    {
                        ShowingAsDialogField.SetValue(Window, true);
                    }
                }
                else
                {
                    HwndHelper.SetOwner(Hwnd, value?.Hwnd ?? IntPtr.Zero);
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
            return other != null && Equals(other.Window) && Equals(other.Hwnd);
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
