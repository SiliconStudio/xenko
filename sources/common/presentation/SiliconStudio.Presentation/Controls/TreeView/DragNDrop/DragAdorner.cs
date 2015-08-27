using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Core;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace System.Windows.Controls.DragNDrop
{
    class DragAdorner : Adorner, IDisposable
    {
        Point position;
        AdornerLayer layer;
        ContentPresenter contentPresenter;

        // Be sure to call the base class constructor.
        public DragAdorner(TreeViewEx treeViewEx, DragContent content)
            : base(treeViewEx)
        {
            layer = AdornerLayer.GetAdornerLayer(treeViewEx);
            layer.Add(this);

            contentPresenter = new ContentPresenter();
            contentPresenter.Content = content;

            Binding b = new Binding("DragTemplate");
            b.Source = treeViewEx;
            b.Mode = BindingMode.OneWay;
            contentPresenter.SetBinding(ContentPresenter.ContentTemplateProperty, b);
        }

        public void UpdatePosition(Point position)
        {
            this.position = position;
            layer.Update();
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(position.X, position.Y));
            return result;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return contentPresenter;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            contentPresenter.Measure(AdornedElement.RenderSize);
            return contentPresenter.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            contentPresenter.Arrange(new Rect(finalSize));
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

        internal DragContent Content
        {
            get { return (DragContent)contentPresenter.Content; }
        }
    }
}
