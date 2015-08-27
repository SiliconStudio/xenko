using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace System.Windows.Controls.DragNDrop
{
    using System.Linq;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Threading;

    class DragNDropController : InputSubscriberBase, IDisposable
    {
        private AutoScroller autoScroller;

        private List<TreeViewExItem> draggableItems;

        private Cursor initialCursor;

        Stopwatch stopWatch;

        DragAdorner dragAdorner;

        InsertAdorner insertAdorner;

        const int dragAreaSize = 5;

        public DragNDropController(AutoScroller autoScroller)
        {
            this.autoScroller = autoScroller;
        }

        internal override void Initialized()
        {
            base.Initialized();
            TreeView.AllowDrop = true;

            TreeView.Drop += OnDrop;
            TreeView.DragOver += OnDragOver;
            TreeView.DragLeave += OnDragLeave;
            TreeView.GiveFeedback += OnGiveFeedBack;
        }

        internal override void Detached()
        {
            base.Detached();
            TreeView.AllowDrop = false;

            TreeView.Drop -= OnDrop;
            TreeView.DragOver -= OnDragOver;
            TreeView.DragLeave -= OnDragLeave;
            TreeView.GiveFeedback -= OnGiveFeedBack;
        }

        void OnDragLeave(object sender, DragEventArgs e)
        {
            if (!Enabled)
                return;

            if (!IsMouseOverTreeView(e.GetPosition(TreeView)))
            {
                CleanUpAdorners();
            }
        }

        private bool IsMouseOverTreeView(Point pos)
        {
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(TreeView, pos);
            if (hitTestResult == null || hitTestResult.VisualHit == null) return false;

            return true;
        }

        public bool Enabled { get; set; }
        private bool CanDrag { get { return draggableItems != null && draggableItems.Count > 0; } }
        internal override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (CheckOverScrollBar(e.GetPosition(TreeView))) return;

            // initalize draggable items on click. Doing that in mouse move results in drag operations,
            // when the border is visible.
            draggableItems = GetDraggableItems(e.GetPosition(TreeView));
            //if (CanDrag)
            //{
                //e.Handled = true;
            //}
        }

        internal override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            // otherwise drops are triggered even if no node was selected in drop
            draggableItems = null;
        }
        internal override void OnMouseMove(MouseEventArgs e)
        {
            if (!IsLeftButtonDown ||CheckOverScrollBar(e.GetPosition(TreeView)))
            {
                CleanUpAdorners();

                return;
            }

            if (!CanDrag) return;

            DragContent dragData = new DragContent();
            foreach (var item in draggableItems)
            {
                dragData.Add(item.Drag());
            }

            DragStart(dragData);
            DragDo(dragData);
            DragEnd();
            e.Handled = true;
        }

        private void CleanUpAdorners()
        {
            if (dragAdorner != null)
            {
                // on external drop no cleanup method is called.
                dragAdorner.Dispose();
                dragAdorner = null;
            }

            if (insertAdorner != null)
            {
                insertAdorner.Dispose();
                insertAdorner = null;
            }
        }

        /// <summary>
        /// Scrolls if mouse is pressed and over scroll border. 
        /// </summary>
        /// <param name="position">Mouse position relative to treeView control.</param>
        /// <returns>Returns true if over scroll border, otherwise false.</returns>
        internal bool TryScroll(Point position)
        {
            if (!IsLeftButtonDown) return false;

            double scrollDelta;
            if (position.Y < AutoScroller.scrollBorderSize)
            {
                //scroll down
                scrollDelta = -AutoScroller.scrollDelta;
            }
            else if ((TreeView.RenderSize.Height - position.Y) < AutoScroller.scrollBorderSize)
            {
                //scroll up
                scrollDelta = AutoScroller.scrollDelta;
            }
            else
            {
                stopWatch = null;
                return false;
            }

            if (stopWatch == null || stopWatch.ElapsedMilliseconds > AutoScroller.scrollDelay)
            {
                autoScroller.Scroll(scrollDelta);
                stopWatch = new Stopwatch();
                stopWatch.Start();
            }

            return true;
        }

        private void DragDo(DragContent dragData)
        {
            DragDrop.DoDragDrop(TreeView, new DataObject(dragData), DragDropEffects.All);
        }

        private void DragEnd()
        {
            DragDrop.RemoveGiveFeedbackHandler(TreeView, OnGiveFeedBack);
         
            TreeView.Cursor = initialCursor;
            autoScroller.IsEnabled = false;

            // Remove the drag adorner from the adorner layer.
            CleanUpAdorners();

            if (insertAdorner != null)
            {
                insertAdorner.Dispose();
                insertAdorner = null;
            }

            if (itemMouseIsOver != null)
            {
                itemMouseIsOver.IsCurrentDropTarget = false;
                itemMouseIsOver = null;
            }
        }

        private void DragStart(DragContent dragData)
        {
            initialCursor = TreeView.Cursor;
            autoScroller.IsEnabled = true;
            if (dragAdorner == null)
            {
                dragAdorner = new DragAdorner(TreeView, dragData);
            }

        }

        private void OnGiveFeedBack(object sender, GiveFeedbackEventArgs e)
        {
            if (!Enabled)
                return;

            // disable switching to default cursors
            e.UseDefaultCursors = false;
            TreeView.Cursor = Cursors.Arrow;
            e.Handled = true;
        }

        private CanInsertReturn CanInsert(TreeViewExItem item, Func<UIElement, Point> getPositionDelegate, IDataObject data)
        {
            TreeViewExItem parentItem = item.ParentTreeViewItem;
            if (parentItem == null)
            {
                return null;
            }

            if (parentItem.Insert == null)
            {
                return null;
            }

            // get position over element
            Size size = item.RenderSize;
            Point positionRelativeToItem = getPositionDelegate(item);

            // decide whether to insert before or after item
            bool after = true;
            if (positionRelativeToItem.Y > dragAreaSize)
            {
                if (size.Height - positionRelativeToItem.Y > dragAreaSize)
                {
                    return null;
                }
            }
            else
            {
                after = false;
            }

            // get index, where to insert
            int index = parentItem.ItemContainerGenerator.IndexFromContainer(item);
            if (after)
            {
                // dont allow insertion after item, if item has children
                if (item.HasItems)
                {
                    return null;
                }
                index++;
            }

            // ask for all formats, if insertion is allowed
            foreach (string f in data.GetFormats())
            {
                if (parentItem.CanInsertFormat == null || parentItem.CanInsertFormat(index, f))
                {
                    if (parentItem.CanInsert == null || parentItem.CanInsert(index, data.GetData(f)))
                    {
                        return new CanInsertReturn(f, index, !after); ;
                    }
                }
            }

            return null;
        }


        private string CanDrop(TreeViewExItem item, IDataObject data)
        {
            if (item == null) return null;
            if (item.DropAction == null) return null;

            foreach (string f in data.GetFormats())
            {
                if (item.CanDropFormat == null || item.CanDropFormat(f))
                {
                    if (item.CanDrop == null || item.CanDrop(data.GetData(f)))
                    {
                        return f;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (!Enabled)
                return;

            TreeViewExItem item = GetTreeViewItemUnderMouse(e.GetPosition(TreeView));
            if (item == null)
            {
                CleanUpAdorners();
                return;
            }

            CanInsertReturn canInsertReturn = CanInsert(item, e.GetPosition, e.Data);
            if (canInsertReturn != null)
            {
                // insert and return

                item.ParentTreeViewItem.Insert(canInsertReturn.Index, e.Data.GetData(canInsertReturn.Format));
                CleanUpAdorners();
                return;
            }

            // check if drop is possible
            string dropformat = CanDrop(item, e.Data);
            if (dropformat != null)
            {
                // drop and return
                object data = e.Data.GetData(dropformat);
                item.DropAction(data);
            }

            CleanUpAdorners();
        }

        TreeViewExItem itemMouseIsOver;
        void OnDragOver(object sender, DragEventArgs e)
        {
            if (!Enabled)
                return;

            // drag over is the only event which returns the position
            // GiveFeedback returns nonsense even from Mouse.GetPosition
            Point point = e.GetPosition(TreeView);

            if (TryScroll(point)) return;

            if (dragAdorner == null)//external drop
            {
                var content = new DragContent();
                content.Add(e.Data);
                dragAdorner = new DragAdorner(TreeView, content);
            }

            dragAdorner.UpdatePosition(point);
            if (IsMouseOverAdorner(point)) return;
            var itemsPresenter = TreeView.ScrollViewer.Content as ItemsPresenter;
            if (itemsPresenter.InputHitTest(e.GetPosition(itemsPresenter)) == null)
            {
                dragAdorner.Content.CanDrop = false;
                dragAdorner.Content.CanInsert = false;
                //dragAdorner.Content.InsertIndex = -1;
                if (insertAdorner != null) insertAdorner.Dispose();
                return;
            }

            if (itemMouseIsOver != null)
            {
                itemMouseIsOver.IsCurrentDropTarget = false;
            }

            itemMouseIsOver = GetTreeViewItemUnderMouse(point);
            if (itemMouseIsOver == null) return;            
            CanInsertReturn canInsertReturn = CanInsert(itemMouseIsOver, e.GetPosition, e.Data);
            if (canInsertReturn != null)
            {
                dragAdorner.Content.CanDrop = false;
                dragAdorner.Content.CanInsert = true;
                //dragAdorner.Content.InsertIndex = canInsertReturn.Index;

                if (insertAdorner == null)
                {
                    insertAdorner = new InsertAdorner(itemMouseIsOver, new InsertContent { Before = canInsertReturn.Before });
                }
                else
                {
                    insertAdorner.Dispose();
                    insertAdorner = new InsertAdorner(itemMouseIsOver, new InsertContent { Before = canInsertReturn.Before });
                }

                itemMouseIsOver.IsCurrentDropTarget = false;
            }
            else
            {
                dragAdorner.Content.CanInsert = false;
                //dragAdorner.Content.InsertIndex = -1;
                if (insertAdorner != null)
                {
                    insertAdorner.Dispose();
                    insertAdorner = null;
                }

                dragAdorner.Content.CanDrop = CanDrop(itemMouseIsOver, e.Data) != null;
                if (itemMouseIsOver != null)
                {
                    itemMouseIsOver.IsCurrentDropTarget = true;
                }
            }
        }

        private bool CheckOverScrollBar(Point positionRelativeToTree)
        {
            if (TreeView.ScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible
                && positionRelativeToTree.X > TreeView.RenderSize.Width - SystemParameters.ScrollWidth)
            {
                return true;
            }

            if (TreeView.ScrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible
                && positionRelativeToTree.Y > TreeView.RenderSize.Height - SystemParameters.ScrollHeight)
            {
                return true;
            }
          
            return false;
        }

        private List<TreeViewExItem> GetDraggableItems(Point mousePositionRelativeToTree)
        {
            List<TreeViewExItem> items = TreeView.GetTreeViewItemsFor(TreeView.SelectedItems).ToList();
            TreeViewExItem itemUnderMouse = GetTreeViewItemUnderMouse(mousePositionRelativeToTree);
            if(itemUnderMouse == null) return new List<TreeViewExItem>();

            if (items.Contains(itemUnderMouse))
            {
                foreach (var item in items)
                {
                    if (item.Drag == null || item.CanDrag == null || !item.CanDrag())
                    {
                        // if one item is not draggable, nothing can be dragged
                        return new List<TreeViewExItem>();
                    }
                }

                return items;
            }

            //mouse is not over an selected item. We have to check if it is over the content. In this case we have to select and start drag n drop.
            var contentPresenter = itemUnderMouse.Template.FindName("content", itemUnderMouse) as ContentPresenter;
            if (contentPresenter.IsMouseOver)
            {
                if (itemUnderMouse.Drag == null || itemUnderMouse.CanDrag == null || !itemUnderMouse.CanDrag())
                {
                    // if one item is not draggable, nothing can be dragged
                    return new List<TreeViewExItem>();
                }

                return new List<TreeViewExItem> { itemUnderMouse };
            }

            return new List<TreeViewExItem>();
        }

        public void Dispose()
        {
            if (TreeView != null)
            {
                TreeView.Drop -= OnDrop;
                TreeView.DragOver -= OnDragOver;
                TreeView.DragLeave -= OnDragLeave;
                TreeView.GiveFeedback -= OnGiveFeedBack;
            }

            if (itemMouseIsOver != null)
            {
                itemMouseIsOver.IsCurrentDropTarget = false;
                itemMouseIsOver = null;
            }
        }
    }
}
