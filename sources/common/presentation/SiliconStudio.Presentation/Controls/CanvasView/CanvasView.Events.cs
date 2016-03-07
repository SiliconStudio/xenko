using System.Windows;
using System.Windows.Input;

namespace SiliconStudio.Presentation.Controls
{
    partial class CanvasView
    {
        private Point mouseDownPoint;

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Handled)
                return;

            e.Handled = Controller?.HandleKeyDown(this, e) ?? false;
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Handled)
                return;

            Focus();
            CaptureMouse();
            mouseDownPoint = e.GetPosition(this);

            e.Handled = Controller?.HandleMouseDown(this, e) ?? false;
        }

        /// <inheritdoc/>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (e.Handled)
                return;

            e.Handled = Controller?.HandleMouseEnter(this, e) ?? false;
        }

        /// <inheritdoc/>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (e.Handled)
                return;

            e.Handled = Controller?.HandleMouseLeave(this, e) ?? false;
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Handled)
                return;

            e.Handled = Controller?.HandleMouseMove(this, e) ?? false;
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Handled)
                return;

            ReleaseMouseCapture();
            e.Handled = Controller?.HandleMouseUp(this, e) ?? false;

            // Open the context menu
            var position = e.GetPosition(this);
            if (ContextMenu != null)
            {
                //if (e.ChangedButton == MouseButton.Right && (position - mouseDownPoint).LengthSquared < 1)
                if (!e.Handled && e.ChangedButton == MouseButton.Right)
                {
                    ContextMenu.Visibility = Visibility.Visible;
                    ContextMenu.IsOpen = true;
                }
                else
                {
                    ContextMenu.Visibility = Visibility.Collapsed;
                    ContextMenu.IsOpen = false;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Handled)
                return;

            e.Handled = Controller?.HandleMouseWheel(this, e) ?? false;
        }
    }
}
