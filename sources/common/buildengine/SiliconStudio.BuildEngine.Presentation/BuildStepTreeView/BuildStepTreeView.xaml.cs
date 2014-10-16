using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SiliconStudio.BuildEngine.Presentation
{
    /// <summary>
    /// Interaction logic for BuildStepTreeView.xaml
    /// </summary>
    public partial class BuildStepTreeView
    {
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(BuildStepTreeView));

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems", typeof(IList), typeof(BuildStepTreeView), new PropertyMetadata(new ObservableCollection<object>(), SelectedItemsPropertyChanged));

        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(BuildStepTreeView));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(BuildStepTreeView));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(BuildStepTreeView));

        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(BuildStepTreeView));

        public StyleSelector ItemContainerStyle
        {
            get { return (StyleSelector)GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }
        
        public static readonly DependencyProperty ItemContainerStyleSelectorProperty = DependencyProperty.Register("ItemContainerStyleSelector", typeof(StyleSelector), typeof(BuildStepTreeView));

        public StyleSelector ItemContainerStyleSelector
        {
            get { return (StyleSelector)GetValue(ItemContainerStyleSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }
        
        public static readonly DependencyProperty MultiSelectionProperty = DependencyProperty.Register("MultiSelection", typeof(bool), typeof(BuildStepTreeView), new PropertyMetadata(true));

        public bool MultiSelection
        {
            get { return (bool)GetValue(MultiSelectionProperty); }
            set { SetValue(MultiSelectionProperty, value); }
        }

        public static readonly DependencyProperty CanMoveItemsProperty = DependencyProperty.Register("CanMoveItems", typeof(bool), typeof(BuildStepTreeView), new PropertyMetadata(false, CanMoveItemsPropertyChangedCallback));

        public bool CanMoveItems
        {
            get { return (bool)GetValue(MultiSelectionProperty); }
            set { SetValue(MultiSelectionProperty, value); }
        }

        public BuildStepTreeView()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, ExecuteDelete, CanExecuteDelete));
        }

        private void CanExecuteDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedItems.Count > 0;
        }

        private void ExecuteDelete(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedItems = SelectedItems.Cast<dynamic>().ToArray();

            foreach (dynamic selectedItem in selectedItems)
            {
                try
                {
                    var command = (ICommand)selectedItem.ViewModel.Delete.TValue;
                    if (command != null && command.CanExecute(null))
                        command.Execute(null);
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch (Exception) { }
                // ReSharper restore EmptyGeneralCatchClause
            }
        }

        private static void SelectedItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var buildStepTreeView = (BuildStepTreeView)d;
            var observable = e.NewValue as INotifyCollectionChanged;
            if (observable != null)
                observable.CollectionChanged += buildStepTreeView.SelectedItemsChanged;
        }

        private void SelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
                SelectedItem = e.NewItems[e.NewItems.Count - 1];
            else
                SelectedItem = SelectedItems.Count > 0 ? SelectedItems[SelectedItems.Count - 1] : null;
        }

        private void SelectingTreeViewExItem(object sender, SelectionChangedCancelEventArgs e)
        {
            if (!MultiSelection)
            {
                var treeview = sender as TreeViewEx;
                if (treeview != null && treeview.SelectedItems.Count > 0)
                {
                    e.Cancel = true;
                }
            }
        }

        private static void CanMoveItemsPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var control = (BuildStepTreeView)dependencyObject;

            if ((bool)e.NewValue)
            {
                control.TreeView.DragTemplate = (DataTemplate)control.Resources["DragTemplate"];
            }
            else
            {
                control.TreeView.DragTemplate = null;
            }
        }
    }
}
