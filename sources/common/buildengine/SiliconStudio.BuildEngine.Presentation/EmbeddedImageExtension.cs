using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

// TODO: This can be moved into SiliconStudio.Core.Presentation once it is stabilized

namespace SiliconStudio.BuildEngine.Presentation
{
    /// <summary>
    /// This markup extension allows to reference an image embedded in an assembly. It can be constructed dorectly from a string, or from a Binding.
    /// The string must be the name of the resource without assembly path and without extension.
    /// </summary>
    public class EmbeddedImageExtension : MarkupExtension
    {
        private class ValueUpdater : DependencyObject
        {
            private static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(DependencyObject), new PropertyMetadata(null, PropertyChanged));

            private readonly object targetObject;
            private readonly object targetProperty;

            public ValueUpdater(object targetObject, object targetProperty)
            {
                this.targetObject = targetObject;
                this.targetProperty = targetProperty;
            }

            public object Value { get { return GetValue(ValueProperty); } set { SetCurrentValue(ValueProperty, value); UpdateValue(value); } }

            public void SetBinding(BindingBase binding)
            {
                BindingOperations.SetBinding(this, ValueProperty, binding);
            }

            private void UpdateValue(object value)
            {
                value = ApplyValueFunc(value);
                if (targetObject != null)
                {
                    if (targetProperty is DependencyProperty)
                    {
                        var obj = targetObject as DependencyObject;
                        var prop = targetProperty as DependencyProperty;

                        if (obj != null)
                        {
                            if (obj.CheckAccess())
                            {
                                obj.SetValue(prop, value);
                            }
                            else
                            {
                                obj.Dispatcher.Invoke(() => obj.SetValue(prop, value));
                            }
                        }
                    }
                    else
                    {
                        var prop = targetProperty as PropertyInfo;
                        if (prop != null && targetObject != null)
                        {
                            prop.SetValue(targetObject, value, null);
                        }
                    }
                }
            }

            public Func<object, object> ApplyValueFunc = x => x;

            private static void PropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
            {
                var val = depObj.GetValue(ValueProperty);
                var valueUpdater = ((ValueUpdater)depObj);
                valueUpdater.UpdateValue(val);
            }
        }

        public object FallbackValue { get; set; }

        private string CommandType { get; set; }

        private readonly Binding commandTypeBinding;

        private static readonly Dictionary<string, BitmapImage> LoadedResources = new Dictionary<string, BitmapImage>();

        public EmbeddedImageExtension(string commandType)
        {
            CommandType = commandType;
        }

        public EmbeddedImageExtension(Binding binding)
        {
            commandTypeBinding = binding;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (target != null)
            {
                var targetObject = target.TargetObject as FrameworkElement;
                var targetProperty = target.TargetProperty;
                if (targetObject != null)
                {
                    var updater = new ValueUpdater(targetObject, targetProperty)
                    {
                        ApplyValueFunc = x => SearchForResource(x != null ? x.ToString() : "") ?? FallbackValue,
                        Value = CommandType
                    };

                    if (commandTypeBinding != null)
                    {
                        commandTypeBinding.Source = targetObject.DataContext;
                        updater.SetBinding(commandTypeBinding);
                    }

                    return updater.ApplyValueFunc(updater.Value);
                }
            }

            return this;
        }

        private static object SearchForResource(string resourceName)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetTypes().Any(y => y.Name == resourceName));
            return SearchForResource(assembly, resourceName);
        }

        private static object SearchForResource(Assembly assembly, string resourceName)
        {
            BitmapImage image;
            if (resourceName != null && LoadedResources.TryGetValue(resourceName, out image))
                return image;

            if (assembly != null && resourceName != null)
            {
                var assemblyResourceNames = assembly.GetManifestResourceNames();

                foreach (string assemblyResourceName in assemblyResourceNames)
                {
                    string filename = Path.GetFileNameWithoutExtension(assemblyResourceName) ?? "";
                    if (filename.Contains('.'))
                        filename = filename.Substring(filename.LastIndexOf('.') + 1);

                    if (string.Compare(filename, resourceName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        using (var stream = assembly.GetManifestResourceStream(assemblyResourceName))
                        {
                            if (stream != null)
                            {
                                try
                                {
                                    image = new BitmapImage();
                                    image.BeginInit();
                                    image.CacheOption = BitmapCacheOption.OnLoad;
                                    image.StreamSource = stream;
                                    image.EndInit();
                                    image.Freeze();
                                    LoadedResources.Add(resourceName, image);
                                    return image;
                                }
                                // ReSharper disable EmptyGeneralCatchClause
                                catch { }
                                // ReSharper restore EmptyGeneralCatchClause
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
