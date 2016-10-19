using System.Windows;
using System.Windows.Interactivity;
using SiliconStudio.Presentation.Behaviors;

namespace SiliconStudio.Presentation.Graph.Behaviors
{
    /// <summary>
    /// This behavior is mandatory on slots so that edges start/end positions can be computed.
    /// </summary>
    public sealed class ActiveConnectorBehavior : Behavior<FrameworkElement>
    {
        #region IDropHandler Interface
        /// <summary>
        /// 
        /// </summary>
        public interface IActiveConnectorHandler
        {
            void OnAttached(FrameworkElement slot);
            void OnDetached(FrameworkElement slot);
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty ActiveConnectorHandlerProperty = DependencyProperty.Register("ActiveConnectorHandler", typeof(IActiveConnectorHandler), typeof(ActiveConnectorBehavior), new PropertyMetadata(OnActiveConnectorHandlerChanged));
        public static DependencyProperty SlotProperty = DependencyProperty.Register("Slot", typeof(object), typeof(ActiveConnectorBehavior));
        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            ActiveConnectorHandler?.OnAttached(AssociatedObject);
        }

        protected override void OnDetaching()
        {
            ActiveConnectorHandler?.OnDetached(AssociatedObject);

            base.OnDetaching();
        }

        private static void OnActiveConnectorHandlerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (ActiveConnectorBehavior)d;

            // Was it already loaded?
            if (behavior.AssociatedObject != null)
            {
                // If yes, update
                ((IActiveConnectorHandler)e.OldValue)?.OnDetached(behavior.AssociatedObject);
                ((IActiveConnectorHandler)e.NewValue)?.OnAttached(behavior.AssociatedObject);
            }
        }

        #region Properties
        public IActiveConnectorHandler ActiveConnectorHandler { get { return (IActiveConnectorHandler)GetValue(ActiveConnectorHandlerProperty); } set { SetValue(ActiveConnectorHandlerProperty, value); } }
        public object Slot { get { return GetValue(SlotProperty); } set { SetValue(SlotProperty, value); } }
        #endregion
    }
}