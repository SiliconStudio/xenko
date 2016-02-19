using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace SiliconStudio.Presentation.Behaviors
{
    public abstract class MouseMoveCaptureBehaviorBase<TElement> : DeferredBehaviorBase<TElement>
        where TElement : UIElement
    {
        /// <summary>
        /// Identifies the <see cref="IsInProgress"/> dependency property key.
        /// </summary>
        public static readonly DependencyPropertyKey IsInProgressPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsInProgress), typeof(bool), typeof(MouseMoveCaptureBehaviorBase<TElement>), new PropertyMetadata(false));
        /// <summary>
        /// Identifies the <see cref="IsInProgress"/> dependency property.
        /// </summary>
        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static readonly DependencyProperty IsInProgressProperty = IsInProgressPropertyKey.DependencyProperty;
        
        /// <summary>
        /// True if an operation is in progress, False otherwise.
        /// </summary>
        public bool IsInProgress { get { return (bool)GetValue(IsInProgressProperty); } protected set { SetValue(IsInProgressPropertyKey, value); } }
        
        protected void Cancel()
        {
            if (!IsInProgress)
                return;

            IsInProgress = false;
            if (AssociatedObject.IsMouseCaptured)
            {
                AssociatedObject.ReleaseMouseCapture();
            }
            CancelOverride();
        }

        protected virtual void CancelOverride()
        {
        }

        ///  <inheritdoc/>
        protected override void OnAttachedOverride()
        {
            AssociatedObject.MouseDown += MouseDown;
            AssociatedObject.MouseMove += MouseMove;
            AssociatedObject.PreviewMouseUp += MouseUp;
            AssociatedObject.LostMouseCapture += OnLostMouseCapture;
        }

        ///  <inheritdoc/>
        protected override void OnDetachingOverride()
        {
            AssociatedObject.MouseDown -= MouseDown;
            AssociatedObject.MouseMove -= MouseMove;
            AssociatedObject.PreviewMouseUp -= MouseUp;
            AssociatedObject.LostMouseCapture -= OnLostMouseCapture;
        }

        protected virtual void OnMouseDown(MouseButtonEventArgs e)
        {
        }

        protected virtual void OnMouseMove(MouseEventArgs e)
        {
        }

        protected virtual void OnMouseUp(MouseButtonEventArgs e)
        {
        }

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsInProgress)
                return;

            OnMouseDown(e);
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsInProgress)
                return;

            OnMouseMove(e);
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!AssociatedObject.IsMouseCaptured || !IsInProgress)
                return;

            OnMouseUp(e);
        }

        private void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            var obj = (UIElement)sender;

            if (!ReferenceEquals(Mouse.Captured, obj))
            {
                Cancel();
            }
        }
    }
}
