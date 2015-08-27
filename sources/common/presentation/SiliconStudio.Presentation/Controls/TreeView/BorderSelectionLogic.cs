// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BorderSelectionLogic.cs" company="Slompf Industries">
//   Copyright (c) Slompf Industries 2006 - 2012
// </copyright>
// <summary>
//   Defines the BorderSelectionLogic type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace System.Windows.Controls
{
    #region

    using System.Collections.Generic;
    using System.Windows.Input;
    using System.Windows.Media;

    #endregion

    internal class BorderSelectionLogic : InputSubscriberBase
    {
        #region Constants and Fields

        private readonly IEnumerable<TreeViewExItem> items;

        private BorderSelectionAdorner border;

        private bool isFirstMove;

        private bool mouseDown;

        private Point startPoint;

        #endregion

        #region Constructors and Destructors

        public BorderSelectionLogic(TreeViewEx treeView, IEnumerable<TreeViewExItem> items)
        {
            this.items = items;
            TreeView = treeView;
        }

        #endregion

        #region Methods

        internal override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                return;
            }

            mouseDown = true;
            startPoint = GetMousePositionRelativeToContent();
            isFirstMove = true;
        }

        internal override void OnMouseMove(MouseEventArgs e)
        {
            HandleInput(e);
        }

        internal override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (isFirstMove) return;

            if (startPoint == GetMousePositionRelativeToContent())
            {
                bool overItem = false;
                foreach (var item in items)
                {
                    if (item.IsEditing) continue;
                    Rect itemRect = GetPositionOf(item);
                    if (itemRect.Contains(startPoint))
                    {
                        overItem = true;
                        break;
                    }
                }

                if (!overItem)
                {
                    List<object> itemsToUnselect = new List<object>();
                    foreach (var item in TreeView.SelectedItems)
                    {
                        itemsToUnselect.Add(item);
                    }

                    var selectionMultiple = TreeView.Selection as SelectionMultiple;
                    if (selectionMultiple != null)
                    {
                        selectionMultiple.SelectByRectangle(new List<object>(), itemsToUnselect);
                    }
                }
            }

            mouseDown = false;
            if (border != null)
            {
                border.Visibility = Visibility.Collapsed;
                border.Dispose();
                border = null;
                e.Handled = true;
            }
        }

        internal override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            HandleInput(e);
        }

        private void HandleInput(RoutedEventArgs e)
        {
            if (mouseDown)
            {
                if (Mouse.LeftButton == MouseButtonState.Released)
                {
                    mouseDown = false;
                    if (border != null)
                    {
                        border.Visibility = Visibility.Collapsed;
                        border.Dispose();
                    }

                    return;
                }

                if (startPoint == GetMousePositionRelativeToContent()) return;

                List<object> itemsToSelect = new List<object>();
                List<object> itemsToUnSelect = new List<object>();

                // if the mouse position or the start point is outside the window, we trim it inside
                Point currentPoint = TrimPointToVisibleArea(GetMousePositionRelativeToContent());
                Point trimmedStartPoint = TrimPointToVisibleArea(startPoint);

                if (isFirstMove)
                {
                    isFirstMove = false;
                    border = new BorderSelectionAdorner(TreeView);
                }

                Rect selectionRect = new Rect(currentPoint, trimmedStartPoint);
                border.UpdatePosition(selectionRect);


                if (isFirstMove)
                {
                    if (!SelectionMultiple.IsControlKeyDown)
                    {
                        foreach (var item in TreeView.SelectedItems)
                        {
                            var treeViewItem = TreeView.GetTreeViewItemFor(item);
                            Rect itemRect = GetPositionOf(treeViewItem);

                            if (!selectionRect.IntersectsWith(itemRect))
                            {
                                itemsToUnSelect.Add(item);
                            }
                        }
                    }
                }

                foreach (var item in items)
                {
                    if (!item.IsVisible || item.IsEditing)
                    {
                        continue;
                    }

                    Rect itemRect = GetPositionOf(item);

                    if (selectionRect.IntersectsWith(itemRect))
                    {
                        if (isFirstMove)
                        {
                            itemsToSelect.Add(item.DataContext);
                        }
                        else
                        {
                            if (!TreeView.SelectedItems.Contains(item.DataContext))
                            {
                                itemsToSelect.Add(item.DataContext);
                            }
                        }
                    }
                    else
                    {
                        if (!SelectionMultiple.IsControlKeyDown && TreeView.SelectedItems.Contains(item.DataContext))
                        {
                            itemsToUnSelect.Add(item.DataContext);
                        }
                    }
                }

                var selectionMultiple = TreeView.Selection as SelectionMultiple;
                if (selectionMultiple != null)
                {
                    selectionMultiple.SelectByRectangle(itemsToSelect, itemsToUnSelect);
                }
                e.Handled = true;
            }
        }

        private Point TrimPointToVisibleArea(Point point)
        {
            return
               new Point(
                  Math.Max(
                     Math.Min(TreeView.ActualWidth + TreeView.ScrollViewer.ContentHorizontalOffset, point.X),
                     +TreeView.ScrollViewer.ContentHorizontalOffset),
                  Math.Max(
                     Math.Min(TreeView.ActualHeight + TreeView.ScrollViewer.ContentVerticalOffset, point.Y),
                     TreeView.ScrollViewer.ContentVerticalOffset));
        }

        private Rect GetPositionOf(TreeViewExItem treeViewItem)
        {
            FrameworkElement item = (FrameworkElement)treeViewItem.Template.FindName("border", treeViewItem);
            if (item == null)
            {
                throw new InvalidOperationException("Could not get content of item");
            }

            Point p = item.TransformToAncestor(TreeView).Transform(new Point());
            double itemLeft = p.X + TreeView.ScrollViewer.ContentHorizontalOffset;
            double itemTop = p.Y + TreeView.ScrollViewer.ContentVerticalOffset;

            return new Rect(itemLeft, itemTop, item.ActualWidth, item.ActualHeight);
        }
        #endregion
    }
}