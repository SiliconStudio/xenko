using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace System.Windows.Controls.DragNDrop
{
    class InsertAdorner : Adorner, IDisposable
    {
        AdornerLayer layer;
        internal TreeViewExItem treeViewItem;
        ContentPresenter contentPresenter;

        public InsertAdorner(TreeViewExItem treeViewItem, InsertContent content)
            : base(GetParentBorder(treeViewItem))
        {
            this.treeViewItem = treeViewItem;

            layer = AdornerLayer.GetAdornerLayer(AdornedElement);
            layer.Add(this);

            contentPresenter = new ContentPresenter();
            contentPresenter.HorizontalAlignment = HorizontalAlignment.Stretch;
            contentPresenter.Width = treeViewItem.ActualWidth;
            contentPresenter.Content = content;

            Binding b = new Binding("InsertTemplate");
            b.Source = treeViewItem.ParentTreeView;
            b.Mode = BindingMode.OneWay;
            contentPresenter.SetBinding(ContentPresenter.ContentTemplateProperty, b);

            content.InsertionMarkerBrush = treeViewItem.ParentTreeView.InsertionMarkerBrush;
            content.Item = treeViewItem;
        }

        public static Border GetParentBorder(TreeViewExItem item)
        {
            Border border = item.Template.FindName("border", item) as Border;
            return border;
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            Rect adornedElementRect = new Rect(this.AdornedElement.RenderSize);
            double positionX = adornedElementRect.Left;
            double positionY;
            if (Content.Before)
            {
                positionY = adornedElementRect.Top;
            }
            else
            {
                positionY = adornedElementRect.Bottom;
            }

            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(positionX, positionY - contentPresenter.ActualHeight/2));
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
            Rect adornedElementRect = new Rect(this.AdornedElement.RenderSize);
            contentPresenter.Measure(new Size(adornedElementRect.Width, constraint.Height));
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

        internal InsertContent Content
        {
            get { return (InsertContent)contentPresenter.Content; }
        }
    }
}
