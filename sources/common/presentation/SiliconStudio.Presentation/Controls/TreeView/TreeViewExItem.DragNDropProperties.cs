namespace System.Windows.Controls
{
    #region

    using System.Windows.Automation.Peers;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Linq;
    using System.ComponentModel;
    using System.Windows.Controls.DragNDrop;
    #endregion

    public partial class TreeViewExItem
    {
        #region Constants and Fields

        public static DependencyProperty CanDragProperty = DependencyProperty.Register(
            "CanDrag",
            typeof(Func<bool>),
            typeof(TreeViewExItem),
            new FrameworkPropertyMetadata(null, null));

        public static DependencyProperty DragProperty = DependencyProperty.Register(
            "Drag",
            typeof(Func<object>),
            typeof(TreeViewExItem),
            new FrameworkPropertyMetadata(null, null));

        public static DependencyProperty CanInsertFormatProperty = DependencyProperty.Register(
            "CanInsertFormat",
            typeof(Func<int, string, bool>),
            typeof(TreeViewExItem),
            new FrameworkPropertyMetadata(null, null));

        public static DependencyProperty InsertProperty = DependencyProperty.Register(
            "Insert",
            typeof(Action<int, object>),
            typeof(TreeViewExItem),
            new FrameworkPropertyMetadata(null, null));

        public static DependencyProperty CanInsertProperty = DependencyProperty.Register(
            "CanInsert",
            typeof(Func<int, object, bool>),
            typeof(TreeViewExItem),
            new FrameworkPropertyMetadata(null, null));

        public static DependencyProperty DropActionProperty = DependencyProperty.Register(
            "DropAction",
            typeof(Action<object>),
            typeof(TreeViewExItem),
            new FrameworkPropertyMetadata(null, null));

        public static DependencyProperty CanDropProperty = DependencyProperty.Register(
            "CanDrop",
            typeof(Func<object, bool>),
            typeof(TreeViewExItem),
            new FrameworkPropertyMetadata(null, null));

        public static DependencyProperty CanDropFormatProperty = DependencyProperty.Register(
            "CanDropFormat",
            typeof(Func<string, bool>),
            typeof(TreeViewExItem),
            new FrameworkPropertyMetadata(null, null));

        #endregion

        #region Public Properties

        public Func<bool> CanDrag
        {
            get { return (Func<bool>)GetValue(CanDragProperty); }
            set { SetValue(CanDragProperty, value); }
        }

        public Func<object> Drag
        {
            get { return (Func<object>)GetValue(DragProperty); }
            set { SetValue(DragProperty, value); }
        }

        public Action<int, object> Insert
        {
            get { return (Action<int, object>)GetValue(InsertProperty); }
            set { SetValue(InsertProperty, value); }
        }

        public Func<int, string, bool> CanInsertFormat
        {
            get { return (Func<int, string, bool>)GetValue(CanInsertFormatProperty); }
            set { SetValue(CanInsertFormatProperty, value); }
        }

        public Func<int, object, bool> CanInsert
        {
            get { return (Func<int, object, bool>)GetValue(CanInsertProperty); }
            set { SetValue(CanInsertProperty, value); }
        }

        public Action<object> DropAction
        {
            get { return (Action<object>)GetValue(DropActionProperty); }
            set { SetValue(DropActionProperty, value); }
        }

        public Func<string, bool> CanDropFormat
        {
            get { return (Func<string, bool>)GetValue(CanDropFormatProperty); }
            set { SetValue(CanDropFormatProperty, value); }
        }

        public Func<object, bool> CanDrop
        {
            get { return (Func<object, bool>)GetValue(CanDropProperty); }
            set { SetValue(CanDropProperty, value); }
        }
        #endregion
    }
}