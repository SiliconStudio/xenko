// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Behaviors;
using SiliconStudio.Presentation.Commands;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Legacy
{
    [TemplatePart(Name = "PART_ItemsPresenter", Type = typeof(ItemsPresenter))]
    public class EditableListBox : ListBox
    {
        public static readonly RoutedCommand AddNewItemRoutedCommand;
        public static readonly RoutedCommand RemoveSelectedItemsRoutedCommand;
        public static readonly RoutedCommand ToggleEditRoutedCommand = new RoutedCommand("ToggleEdit", typeof(EditableListBox));

        static EditableListBox()
        {
            var inputCollection = new InputGestureCollection();
            inputCollection.Add(new KeyGesture(Key.Add));
            AddNewItemRoutedCommand = new RoutedCommand("AddNewItem", typeof(EditableListBox), inputCollection);

            inputCollection = new InputGestureCollection();
            inputCollection.Add(new KeyGesture(Key.Delete));
            RemoveSelectedItemsRoutedCommand = new RoutedCommand("RemoveSelectedItems", typeof(EditableListBox), inputCollection);

            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditableListBox),
                new FrameworkPropertyMetadata(typeof(EditableListBox)));
        }

        public EditableListBox()
        {
            CommandBindings.Add(new CommandBinding(AddNewItemRoutedCommand, (s, e) => AddNewItem(e.Parameter)));
            CommandBindings.Add(new CommandBinding(RemoveSelectedItemsRoutedCommand, (s, e) => RemoveSelectedItems(e.Parameter)));
            CommandBindings.Add(new CommandBinding(ToggleEditRoutedCommand, (s, e) => IsEditing = !IsEditing));

            Interaction.GetBehaviors(this).Changed += (s, e) => TransferDropBehaviorsFromRootToPresenter();
        }

        private void TransferDropBehaviorsFromRootToPresenter()
        {
            if (itemsPresenter == null)
                return; // no presenter yet

            var rootBehaviorsCollection = Interaction.GetBehaviors(this);

            var rootDropBehaviors = rootBehaviorsCollection
                .OfType<DropBehavior>() // filter out DropBehaviors only
                .ToArray(); // make it a cold list

            if (rootDropBehaviors.Length == 0)
                return; // avoid stack overflow

            var presenterBehaviorsCollection = Interaction.GetBehaviors(itemsPresenter);

            foreach (var behavior in rootDropBehaviors)
            {
                rootBehaviorsCollection.Remove(behavior); // detach from root
                presenterBehaviorsCollection.Add(behavior); // attach to presenter
            }
        }

        private ItemsPresenter itemsPresenter;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (itemsPresenter != null)
                Interaction.GetBehaviors(itemsPresenter).Clear();

            itemsPresenter = FrameworkElementExtensions.CheckTemplatePart<ItemsPresenter>(GetTemplateChild("PART_ItemsPresenter"));

            if (itemsPresenter != null)
            {
                Interaction.GetBehaviors(itemsPresenter).Add(new DropBehavior
                {
                    DataType = typeof(EditableListBoxItem).FullName,
                    // TODO: not maintained, commented out
                    //Command = new AnonymousCommand(ReorderItem),
                });

                TransferDropBehaviorsFromRootToPresenter();
            }
        }

        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            if (newValue == null)
                IsEditableDataSource = true;
            else
                IsEditableDataSource = !newValue.IsReadOnly();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is EditableListBoxItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new EditableListBoxItem();
        }

        // === Reorder commanding ===========================================================================

        public ICommand ReorderItemCommand
        {
            get { return (ICommand)GetValue(ReorderItemCommandProperty); }
            set { SetValue(ReorderItemCommandProperty, value); }
        }

        public static readonly DependencyProperty ReorderItemCommandProperty = DependencyProperty.Register(
            "ReorderItemCommand",
            typeof(ICommand),
            typeof(EditableListBox));

        private void ReorderItem(object parameter)
        {
            if (IsEditing == false)
                return;

            var reorderCommand = ReorderItemCommand;

            if (reorderCommand != null)
            {
                if (reorderCommand.CanExecute(parameter))
                    reorderCommand.Execute(parameter);
            }
        }

        // === AddNewItem commanding ========================================================================

        public ICommand AddNewItemCommand
        {
            get { return (ICommand)GetValue(AddNewItemCommandProperty); }
            set { SetValue(AddNewItemCommandProperty, value); }
        }

        public static readonly DependencyProperty AddNewItemCommandProperty = DependencyProperty.Register(
            "AddNewItemCommand",
            typeof(ICommand),
            typeof(EditableListBox));

        private void AddNewItem(object parameter)
        {
            if (IsEditing == false)
                return;

            var addCommand = AddNewItemCommand;

            if (addCommand != null)
            {
                if (addCommand.CanExecute(null))
                    addCommand.Execute(null);
            }
        }

        // === RemoveSelectedItems commanding ========================================================================

        public ICommand RemoveSelectedItemsCommand
        {
            get { return (ICommand)GetValue(RemoveSelectedItemsCommandProperty); }
            set { SetValue(RemoveSelectedItemsCommandProperty, value); }
        }

        public static readonly DependencyProperty RemoveSelectedItemsCommandProperty = DependencyProperty.Register(
            "RemoveSelectedItemsCommand",
            typeof(ICommand),
            typeof(EditableListBox));

        private void RemoveSelectedItems(object parameter)
        {
            if (IsEditing == false)
                return;

            var removeCommand = RemoveSelectedItemsCommand;
            if (removeCommand == null)
                return;

            var dataSource = ItemsSource ?? Items.SourceCollection;

            var garbage = new List<object>();

            foreach (var item in dataSource)
            {
                var container = (EditableListBoxItem)ItemContainerGenerator.ContainerFromItem(item);
                if (container.IsSelected)
                    garbage.Add(item);
            }

            if (garbage.Count == 0)
                return;

            var param = garbage.ToArray();

            if (removeCommand.CanExecute(param))
                removeCommand.Execute(param);
        }

        // === IsEditableDataSource ============================================================================

        public bool IsEditableDataSource
        {
            get { return (bool)GetValue(IsEditableDataSourceProperty); }
            private set { SetValue(IsEditableDataSourcePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsEditableDataSourcePropertyKey = DependencyProperty.RegisterReadOnly(
            "IsEditableDataSource",
            typeof(bool),
            typeof(EditableListBox),
            new PropertyMetadata(true));
        public static readonly DependencyProperty IsEditableDataSourceProperty = IsEditableDataSourcePropertyKey.DependencyProperty;

        // === IsEditing ================================================================================

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(
            "IsEditing",
            typeof(bool),
            typeof(EditableListBox),
            new PropertyMetadata(false, OnIsEditingPropertyChanged));

        public event RoutedPropertyChangedEventHandler<bool> IsEditingChanged
        {
            add { AddHandler(IsEditingChangedEvent, value); }
            remove { RemoveHandler(IsEditingChangedEvent, value); }
        }

        public static readonly RoutedEvent IsEditingChangedEvent = EventManager.RegisterRoutedEvent(
            "IsEditingChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<bool>),
            typeof(EditableListBox));

        private static void OnIsEditingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((EditableListBox)sender).OnIsEditingChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIsEditingChanged(bool oldValue, bool newValue)
        {
            RaiseEvent(new RoutedPropertyChangedEventArgs<bool>(oldValue, newValue, IsEditingChangedEvent));
        }
    }
}
