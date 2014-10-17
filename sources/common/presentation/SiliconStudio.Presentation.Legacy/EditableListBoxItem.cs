// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace SiliconStudio.Presentation.Legacy
{
    public class EditableListBoxItem : ListBoxItem
    {
        private EditableListBox parent;

        // === IsEditing =================================================================================

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            private set { SetValue(IsEditingPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsEditingPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsEditing",
            typeof(bool),
            typeof(EditableListBoxItem),
            new PropertyMetadata());
        public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;

        // ====================================================================================================

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            if (parent != null)
                UnhookParentEvents();

            parent = (EditableListBox)ItemsControl.ItemsControlFromItemContainer(this);

            if (parent != null)
            {
                HookParentEvents();
                EvaluateParentValues();
            }

            base.OnVisualParentChanged(oldParent);
        }

        private void HookParentEvents()
        {
            parent.IsEditingChanged += OnParentIsEditingChanged;

            var item = parent.ItemContainerGenerator.ItemFromContainer(this);
            if (item != null)
            {
                Interaction.GetBehaviors(this).Add(new DragBehavior
                {
                    DataType = GetType().FullName,
                    DragData = item,
                });
            }
        }

        private void UnhookParentEvents()
        {
            parent.IsEditingChanged -= OnParentIsEditingChanged;

            Interaction.GetBehaviors(this).Clear();
        }

        private void OnParentIsEditingChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            EvaluateParentValues();
        }

        private void EvaluateParentValues()
        {
            IsEditing = parent.IsEditing;
            Interaction.GetBehaviors(this).Cast<DragBehavior>().First().IsDragEnabled = parent.IsEditing;
        }
    }
}
