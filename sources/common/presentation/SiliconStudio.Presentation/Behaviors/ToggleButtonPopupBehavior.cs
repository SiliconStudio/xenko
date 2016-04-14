using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// This behavior will ensure that the associated toggle button can be toggled only when both mouse down and mouse up
    /// events are received, preventing to toggle it when a popup window is open. This behavior is useful when binding the
    /// <see cref="Popup.IsOpen"/> property of a popup to the <see cref="ToggleButton.IsChecked"/> of a toggle button.
    /// </summary>
    public class ToggleButtonPopupBehavior : Behavior<ToggleButton>
    {
        private bool mouseDownOccurred;

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseLeftButtonDown += MouseDown;
            AssociatedObject.PreviewMouseLeftButtonUp += MouseUp;
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!mouseDownOccurred)
            {
                // Discard the event if the mouse down didn't occur on this control.
                e.Handled = true;
            }
            mouseDownOccurred = false;
        }

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseDownOccurred = true;
        }
    }
}
