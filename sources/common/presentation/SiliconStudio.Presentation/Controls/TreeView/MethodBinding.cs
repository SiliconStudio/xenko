using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace System.Windows.Controls
{
    public class MethodBinding : Binding
    {
        class MethodConverter : IValueConverter
        {
            string methodName;

            public MethodConverter(string methodName)
            {
                this.methodName = methodName;
            }

            public object Convert(object value, Type targetType, object parameter, Globalization.CultureInfo culture)
            {
                // I do use reflection for this. Another way would be to force the viewModel to implement sth like
                // IDragSource and IDropTarget. Then I could cast and return the method. Would probably be more
                // performant, but we force the viewmodel to reference an controls assembly. Lets try this code
                // until someone dont likes it. ;o)
                var mi = value.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (mi == null)
                {
                    throw new InvalidOperationException("Could not find method with name '" + methodName + "' on type '" + value.GetType() + "'.");
                }

                if (targetType == typeof(Func<object, bool>))
                {
                    return new Func<object, bool>(x => (bool)mi.Invoke(value, new object[] { x }));
                }
                else if (targetType == typeof(Func<int, object, bool>))
                {
                    return new Func<int, object, bool>((i, x) => (bool)mi.Invoke(value, new object[] { i, x }));
                }
                else if (targetType == typeof(Func<string, bool>))
                {
                    return new Func<string, bool>(s => (bool)mi.Invoke(value, new object[] { s }));
                }
                else if (targetType == typeof(Func<int, string, bool>))
                {
                    return new Func<int, string, bool>((i, s) => (bool)mi.Invoke(value, new object[] { i, s }));
                }
                else if (targetType == typeof(Action<object>))
                {
                    return new Action<object>(x => mi.Invoke(value, new object[] { x }));
                }
                else if (targetType == typeof(Action<int, object>))
                {
                    return new Action<int, object>((i, x) => mi.Invoke(value, new object[] { i, x }));
                }
                else if (targetType == typeof(Func<bool>))
                {
                    return new Func<bool>(() => (bool)mi.Invoke(value, new object[] { }));
                }
                else if (targetType == typeof(Func<object>))
                {
                    return new Func<object>(() => mi.Invoke(value, null));
                }

                return null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public MethodBinding(string methodName)
        {
            Converter = new MethodConverter(methodName);
        }
    }
}
