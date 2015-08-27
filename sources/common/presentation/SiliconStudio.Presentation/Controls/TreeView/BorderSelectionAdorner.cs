using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace System.Windows.Controls
{
    class BorderSelectionAdorner : Adorner, IDisposable
    {
        Point position;
        AdornerLayer layer;
        Border border;
        TreeViewEx treeViewEx;

        // Be sure to call the base class constructor.
        public BorderSelectionAdorner(TreeViewEx treeViewEx)
            : base(treeViewEx)
        {
            this.treeViewEx = treeViewEx;
            this.border = new Border { BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(1), Opacity = 0.5 };

            Binding brushBinding = new Binding("BorderBrushSelectionRectangle");
            brushBinding.Source = treeViewEx;
            border.SetBinding(Border.BorderBrushProperty, brushBinding);
            Binding backgroundBinding = new Binding("BackgroundSelectionRectangle");
            backgroundBinding.Source = treeViewEx;
            border.SetBinding(Border.BackgroundProperty, backgroundBinding);

            layer = AdornerLayer.GetAdornerLayer(treeViewEx);
            layer.Add(this);
        }

        public void UpdatePosition(Rect position)
        {
            this.Width = position.Width;
            this.Height = position.Height;
            this.position = position.Location;
            layer.Update();
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(
                position.X - treeViewEx.ScrollViewer.ContentHorizontalOffset, position.Y - treeViewEx.ScrollViewer.ContentVerticalOffset));
            return result;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return border;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            border.Measure(AdornedElement.RenderSize);
            return border.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            border.Arrange(new Rect(finalSize));
            return finalSize;
        }

        public void Dispose()
        {
            if (layer != null)
            {
                layer.Remove(this);
                layer = null;
            }
        }
    }
}
