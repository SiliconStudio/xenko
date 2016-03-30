using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace SiliconStudio.Presentation.Behaviors
{
    public abstract class MouseMoveCaptureBehaviorBase<TElement> : Behavior<TElement>
        where TElement : UIElement
    {
        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(MouseMoveCaptureBehaviorBase<TElement>), new PropertyMetadata(true, IsEnabledChanged));

        /// <summary>
        /// Identifies the <see cref="IsInProgress"/> dependency property key.
        /// </summary>
        protected static readonly DependencyPropertyKey IsInProgressPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsInProgress), typeof(bool), typeof(MouseMoveCaptureBehaviorBase<TElement>), new PropertyMetadata(false));
        /// <summary>
        /// Identifies the <see cref="IsInProgress"/> dependency property.
        /// </summary>
        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        public static readonly DependencyProperty IsInProgressProperty = IsInProgressPropertyKey.DependencyProperty;

        /// <summary>
        /// Identifies the <see cref="Modifiers"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ModifiersProperty =
            DependencyProperty.Register(nameof(Modifiers), typeof(ModifierKeys?), typeof(MouseMoveCaptureBehaviorBase<TElement>), new PropertyMetadata(null));

        public bool IsEnabled { get { return (bool)GetValue(IsEnabledProperty); } set { SetValue(IsEnabledProperty, value); } }
        
        /// <summary>
        /// True if an operation is in progress, False otherwise.
        /// </summary>
        public bool IsInProgress { get { return (bool)GetValue(IsInProgressProperty); } protected set { SetValue(IsInProgressPropertyKey, value); } }

        public ModifierKeys? Modifiers { get { return (ModifierKeys?)GetValue(ModifiersProperty); } set { SetValue(ModifiersProperty, value); } }

        private static void IsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (MouseMoveCaptureBehaviorBase<TElement>)d;
            if ((bool)e.NewValue != true)
            {
                behavior.Cancel();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool AreModifiersValid()
        {
            if (Modifiers == null)
                return true;
            return Modifiers == ModifierKeys.None ? Keyboard.Modifiers == ModifierKeys.None : Keyboard.Modifiers.HasFlag(Modifiers);
        }

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
        protected override void OnAttached()
        {
            AssociatedObject.MouseDown += MouseDown;
            AssociatedObject.MouseMove += MouseMove;
            AssociatedObject.PreviewMouseUp += MouseUp;
            AssociatedObject.LostMouseCapture += OnLostMouseCapture;
        }

        ///  <inheritdoc/>
        protected override void OnDetaching()
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
            if (!IsEnabled || IsInProgress)
                return;

            OnMouseDown(e);
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsEnabled || !IsInProgress)
                return;

            OnMouseMove(e);
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled || !IsInProgress || !AssociatedObject.IsMouseCaptured)
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
