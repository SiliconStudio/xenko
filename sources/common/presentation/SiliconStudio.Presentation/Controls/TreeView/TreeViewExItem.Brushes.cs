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
        #region Brushes properties

        #region Brushes focused
        public static DependencyProperty BackgroundFocusedProperty = DependencyProperty.Register(
                    "BackgroundFocused",
                    typeof(Brush),
                    typeof(TreeViewExItem),
                    new FrameworkPropertyMetadata(SystemColors.HighlightBrush, null));

        public static DependencyProperty BorderBrushFocusedProperty = DependencyProperty.Register(
                    "BorderBrushFocused",
                    typeof(Brush),
                    typeof(TreeViewExItem),
                    new FrameworkPropertyMetadata(Brushes.Transparent, null));

        public Brush BackgroundFocused
        {
            get
            {
                return (Brush)GetValue(BackgroundFocusedProperty);
            }

            set
            {
                SetValue(BackgroundFocusedProperty, value);
            }
        }

        public Brush BorderBrushFocused
        {
            get
            {
                return (Brush)GetValue(BorderBrushFocusedProperty);
            }

            set
            {
                SetValue(BorderBrushFocusedProperty, value);
            }
        }
        #endregion

        #region Brushes selected and focused
        public static DependencyProperty BackgroundFocusedSelectedProperty =
                    DependencyProperty.Register(
                    "BackgroundFocusedSelected",
                    typeof(Brush),
                    typeof(TreeViewExItem),
                    new FrameworkPropertyMetadata(Brushes.DarkGray, null));

        public static DependencyProperty BorderBrushFocusedSelectedProperty =
                    DependencyProperty.Register(
                    "BorderBrushFocusedSelected",
                    typeof(Brush),
                    typeof(TreeViewExItem),
                    new FrameworkPropertyMetadata(Brushes.Black, null));

        public Brush BackgroundFocusedSelected
        {
            get
            {
                return (Brush)GetValue(BackgroundFocusedSelectedProperty);
            }

            set
            {
                SetValue(BackgroundFocusedSelectedProperty, value);
            }
        }

        public Brush BorderBrushFocusedSelected
        {
            get
            {
                return (Brush)GetValue(BorderBrushFocusedSelectedProperty);
            }

            set
            {
                SetValue(BorderBrushFocusedSelectedProperty, value);
            }
        }
        #endregion

        #region Brushes hovered

        public static DependencyProperty BackgroundHoveredProperty = DependencyProperty.Register(
                    "BackgroundHovered",
                    typeof(Brush),
                    typeof(TreeViewExItem),
                    new FrameworkPropertyMetadata(Brushes.LightGray, null));

        public static DependencyProperty BorderBrushHoveredProperty = DependencyProperty.Register(
                    "BorderBrushHovered",
                    typeof(Brush),
                    typeof(TreeViewExItem),
                    new FrameworkPropertyMetadata(Brushes.Transparent, null));

        public Brush BackgroundHovered
        {
            get
            {
                return (Brush)GetValue(BackgroundHoveredProperty);
            }

            set
            {
                SetValue(BackgroundHoveredProperty, value);
            }
        }

        public Brush BorderBrushHovered
        {
            get
            {
                return (Brush)GetValue(BorderBrushHoveredProperty);
            }

            set
            {
                SetValue(BorderBrushHoveredProperty, value);
            }
        }
        #endregion

        #region Brushes selected and hovered
        public static DependencyProperty BackgroundSelectedHoveredProperty = DependencyProperty.Register(
                    "BackgroundSelectedHovered",
                    typeof(Brush),
                    typeof(TreeViewExItem),
                    new FrameworkPropertyMetadata(Brushes.DarkGray, null));

        public static DependencyProperty BorderBrushSelectedHoveredProperty =
                    DependencyProperty.Register(
                    "BorderBrushSelectedHovered",
                    typeof(Brush),
                    typeof(TreeViewExItem),
                    new FrameworkPropertyMetadata(Brushes.Black, null));

        public Brush BorderBrushSelectedHovered
        {
            get
            {
                return (Brush)GetValue(BorderBrushSelectedHoveredProperty);
            }
            set
            {
                SetValue(BorderBrushSelectedHoveredProperty, value);
            }
        }

        public Brush BackgroundSelectedHovered
        {
            get
            {
                return (Brush)GetValue(BackgroundSelectedHoveredProperty);
            }
            set
            {
                SetValue(BackgroundSelectedHoveredProperty, value);
            }
        }
        #endregion

        #region Brushes selected
        public static DependencyProperty BackgroundSelectedProperty = DependencyProperty.Register(
                    "BackgroundSelected",
                    typeof(Brush),
                    typeof(TreeViewExItem),
                    new FrameworkPropertyMetadata(Brushes.LightGray, null));

        public static DependencyProperty BorderBrushSelectedProperty = DependencyProperty.Register(
                    "BorderBrushSelected",
                    typeof(Brush),
                    typeof(TreeViewExItem),
                    new FrameworkPropertyMetadata(Brushes.Transparent, null));

        public Brush BackgroundSelected
        {
            get
            {
                return (Brush)GetValue(BackgroundSelectedProperty);
            }

            set
            {
                SetValue(BackgroundSelectedProperty, value);
            }
        }

        public Brush BorderBrushSelected
        {
            get
            {
                return (Brush)GetValue(BorderBrushSelectedProperty);
            }

            set
            {
                SetValue(BorderBrushSelectedProperty, value);
            }
        }
        #endregion

        #region Brushes disabled
        public static DependencyProperty BackgroundInactiveProperty = DependencyProperty.Register(
                    "BackgroundInactive",
                    typeof(Brush),
                    typeof(TreeViewExItem),
                    new FrameworkPropertyMetadata(Brushes.LightGray, null));

        public static DependencyProperty BorderBrushInactiveProperty =
                    DependencyProperty.Register(
                    "BorderBrushInactive",
                    typeof(Brush),
                    typeof(TreeViewExItem),
                    new FrameworkPropertyMetadata(Brushes.Black, null));

        public Brush BackgroundInactive
        {
            get
            {
                return (Brush)GetValue(BackgroundInactiveProperty);
            }

            set
            {
                SetValue(BackgroundInactiveProperty, value);
            }
        }

        public Brush BorderBrushInactive
        {
            get
            {
                return (Brush)GetValue(BorderBrushInactiveProperty);
            }

            set
            {
                SetValue(BorderBrushInactiveProperty, value);
            }
        }
        #endregion
        #endregion
    }
}