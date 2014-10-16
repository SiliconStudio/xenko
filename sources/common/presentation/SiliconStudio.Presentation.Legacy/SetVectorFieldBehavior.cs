// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Windows.Interactivity;

namespace SiliconStudio.Presentation.Legacy
{
    public class SetVectorFieldBehavior : Behavior<VectorEditor>
    {
        public string Field { get; set; }
        public float Value { get; set; }

        protected override void OnAttached()
        {
            AssociatedObject.VectorSourceChanged += OnHostVectorSourceChanged;
        }

        private void OnHostVectorSourceChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null)
                return;

            var fieldInfo = AssociatedObject.VectorSource.GetType().GetField(Field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(AssociatedObject.VectorSource, Value);
            }
            else
            {
                var propertyInfo = AssociatedObject.VectorSource.GetType().GetProperty(Field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (propertyInfo != null)
                    propertyInfo.SetValue(AssociatedObject.VectorSource, Value, null);
            }

            AssociatedObject.VectorSourceChanged -= OnHostVectorSourceChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.VectorSourceChanged -= OnHostVectorSourceChanged;
        }
    }
}
