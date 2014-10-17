using System.Windows;
using System.Windows.Media;

namespace SiliconStudio.BuildEngine.Presentation
{
    /// <summary>
    /// This class is intended to provide an easy way to customize the treeview differently.
    /// </summary>
    /// <remarks>With the current (simple) style, these property are not actualy used</remarks>
    public sealed class BuildStepDecoration : DependencyObject
    {
        public static readonly DependencyProperty LeftBackgroundBrushProperty = DependencyProperty.RegisterAttached("LeftBackgroundBrush", typeof(Brush), typeof(BuildStepDecoration), new UIPropertyMetadata(null));
        public static readonly DependencyProperty TopBackgroundBrushProperty = DependencyProperty.RegisterAttached("TopBackgroundBrush", typeof(Brush), typeof(BuildStepDecoration), new UIPropertyMetadata(null));
        public static readonly DependencyProperty BottomBackgroundBrushProperty = DependencyProperty.RegisterAttached("BottomBackgroundBrush", typeof(Brush), typeof(BuildStepDecoration), new UIPropertyMetadata(null));

        public static Brush GetLeftBackgroundBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(LeftBackgroundBrushProperty);
        }

        public static void SetLeftBackgroundBrush(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(LeftBackgroundBrushProperty, value);
        }

        public static Brush GetTopBackgroundBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(TopBackgroundBrushProperty);
        }

        public static void SetTopBackgroundBrush(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(TopBackgroundBrushProperty, value);
        }

        public static Brush GetBottomBackgroundBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(BottomBackgroundBrushProperty);
        }

        public static void SetBottomBackgroundBrush(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(BottomBackgroundBrushProperty, value);
        }
    }
}
