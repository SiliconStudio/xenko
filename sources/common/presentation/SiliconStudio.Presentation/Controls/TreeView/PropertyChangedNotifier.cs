using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace System.Windows.Controls
{
    public sealed class PropertyChangedNotifier :
    DependencyObject,
    IDisposable
    {
        #region Member Variables
        private WeakReference propertySource;
        private Action<DependencyObject, object> propertyChangedAction;
        #endregion // Member Variables

        #region Constructor
        public PropertyChangedNotifier(DependencyObject propertySource, string path, Action<DependencyObject,object> propertyChangedAction)
            : this(propertySource, new PropertyPath(path), propertyChangedAction)
        {
        }

        public PropertyChangedNotifier(DependencyObject propertySource, DependencyProperty property, Action<DependencyObject, object> propertyChangedAction)
            : this(propertySource, new PropertyPath(property), propertyChangedAction)
        {
        }

        public PropertyChangedNotifier(DependencyObject propertySource, PropertyPath property, Action<DependencyObject, object> propertyChangedAction)
        {
            if (null == propertySource) throw new ArgumentNullException("propertySource");
            if (null == property) throw new ArgumentNullException("property");
            if (null == propertyChangedAction) throw new ArgumentNullException("propertyChangedAction");

            this.propertyChangedAction = propertyChangedAction;
            this.propertySource = new WeakReference(propertySource);

            Binding binding = new Binding();
            binding.Path = property;
            binding.Mode = BindingMode.OneWay;
            binding.Source = propertySource;
            BindingOperations.SetBinding(this, ValueProperty, binding);
        }
        #endregion // Constructor

        #region PropertySource
        public DependencyObject PropertySource
        {
            get
            {
                try
                {
                    // note, it is possible that accessing the target property
                    // will result in an exception so i’ve wrapped this check
                    // in a try catch
                    return this.propertySource.IsAlive ? this.propertySource.Target as DependencyObject : null;
                }
                catch
                {
                    return null;
                }
            }
        }
        #endregion // PropertySource

        #region Value
        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
        typeof(object), typeof(PropertyChangedNotifier), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PropertyChangedNotifier notifier = (PropertyChangedNotifier)d;
            if (notifier != null) notifier.propertyChangedAction(notifier.PropertySource, e.NewValue);
        }

        /// <summary>
        /// Returns/sets the value of the property
        /// </summary>
        /// <seealso cref="ValueProperty"/>
        [Description("Returns/sets the value of the property")]
        [Category("Behavior")]
        [Bindable(true)]
        public object Value
        {
            get
            {
                return (object)this.GetValue(PropertyChangedNotifier.ValueProperty);
            }
            set
            {
                this.SetValue(PropertyChangedNotifier.ValueProperty, value);
            }
        }
        #endregion //Value

        #region IDisposable Members
        public void Dispose()
        {
            BindingOperations.ClearBinding(this, ValueProperty);
        }
        #endregion
    }
}
