using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using SiliconStudio.Presentation.Collections;

namespace System.Windows.Controls
{

    /// <summary>
    /// Logic for the multiple selection
    /// </summary>
    internal class SelectionMultiple : SelectionStrategyBase
    {
        private readonly TreeViewEx treeViewEx;

        private object lastShiftRoot;
        private bool mouseDown;

        public SelectionMultiple(TreeViewEx treeViewEx) : base(treeViewEx)
        {
            this.treeViewEx = treeViewEx;
            BorderSelectionLogic = new BorderSelectionLogic(treeViewEx, TreeViewElementFinder.FindAll(treeViewEx, false));
        }

        internal BorderSelectionLogic BorderSelectionLogic { get; private set; }

        #region Private modify selection methods

        private void ToggleItem(TreeViewExItem item)
        {
            if (treeViewEx.SelectedItems.Contains(item.DataContext))
            {
                ModifySelection(null, item.DataContext);
            }
            else
            {
                ModifySelection(item.DataContext, null);
            }
        }

        private bool ModifySelection(object itemToSelect, List<object> itemsToUnselect)
        {
            var itemsToSelect = new List<object>(1) { itemToSelect };
            if (itemsToUnselect == null) itemsToUnselect = new List<object>();
            return ModifySelection(itemsToSelect, itemsToUnselect);
        }

        private bool ModifySelection(List<object> itemsToSelect, object itemToUnselect)
        {
            if (itemsToSelect == null) itemsToSelect = new List<object>();

            List<object> itemsToUnselect = new List<object>();
            if (itemToUnselect != null) itemsToUnselect.Add(itemToUnselect);

            return ModifySelection(itemsToSelect, itemsToUnselect);
        }

        private bool ModifySelection(List<object> itemsToSelect, List<object> itemsToUnselect)
        {
            //clean up any duplicate or unnecessery input
            OptimizeModifySelection(itemsToSelect, itemsToUnselect);

            //check if there's anything to do.
            if (itemsToSelect.Count == 0 && itemsToUnselect.Count == 0)
            {
                return false;
            }

            // notify listeners what is about to change.
            // Let them cancel and/or handle the selection list themself
            bool allowed = treeViewEx.CheckSelectionAllowed(itemsToSelect, itemsToUnselect);
            if (!allowed) return false;

            // Unselect and then select items
            foreach (object itemToUnSelect in itemsToUnselect)
            {
                treeViewEx.SelectedItems.Remove(itemToUnSelect);
            }

            ((NonGenericObservableListWrapper<object>)treeViewEx.SelectedItems).AddRange(itemsToSelect);

            object lastSelectedItem = itemsToSelect.LastOrDefault();

            if (itemsToUnselect.Contains(lastShiftRoot)) lastShiftRoot = null;
            if (!(TreeView.SelectedItems.Contains(lastShiftRoot) && IsShiftKeyDown)) lastShiftRoot = lastSelectedItem;

            return true;
        }

        private void OptimizeModifySelection(List<object> itemsToSelect, List<object> itemsToUnselect)
        {
            // check for items in both lists and remove them in unselect list
            List<object> biggerList;
            List<object> smallerList;
            if (itemsToSelect.Count > itemsToUnselect.Count)
            {
                biggerList = itemsToSelect;
                smallerList = itemsToUnselect;
            }
            else
            {
                smallerList = itemsToUnselect;
                biggerList = itemsToSelect;
            }

            List<object> temporaryList = new List<object>();
            foreach (object item in biggerList)
            {
                if (smallerList.Contains(item))
                {
                    temporaryList.Add(item);
                }
            }

            foreach (var item in temporaryList)
            {
                itemsToUnselect.Remove(item);
            }

            // check for itemsToSelect allready in treeViewEx.SelectedItems
            temporaryList.Clear();
            foreach (object item in itemsToSelect)
            {
                if (treeViewEx.SelectedItems.Contains(item))
                {
                    temporaryList.Add(item);
                }
            }

            foreach (var item in temporaryList)
            {
                itemsToSelect.Remove(item);
            }

            // check for itemsToUnSelect not in treeViewEx.SelectedItems
            temporaryList.Clear();
            foreach (object item in itemsToUnselect)
            {
                if (!treeViewEx.SelectedItems.Contains(item))
                {
                    temporaryList.Add(item);
                }
            }

            foreach (var item in temporaryList)
            {
                itemsToUnselect.Remove(item);
            }
        }

        protected override void SelectSingleItem(TreeViewExItem item)
        {

            // selection with SHIFT is not working in virtualized mode. Thats because the Items are not visible.
            // Therefor the children cannot be found/selected.
            if (IsShiftKeyDown && treeViewEx.SelectedItems.Count > 0 && !treeViewEx.IsVirtualizing)
            {
                SelectWithShift(item);
            }
            else if (IsControlKeyDown)
            {
                ToggleItem(item);
            }
            else
            {
                treeViewEx.SelectedItems.Clear();
                ModifySelection(item.DataContext, null);
            }

        }
        #endregion

        #region Overrides InputSubscriberBase
        internal override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            mouseDown = e.ChangedButton == MouseButton.Left;

            TreeViewExItem item = GetTreeViewItemUnderMouse(e.GetPosition(treeViewEx));
            if (item == null) return;
            if (e.ChangedButton != MouseButton.Right || item.ContextMenu == null) return;            
            if (item.IsEditing) return;

            SelectSingleItem(item);

            FocusHelper.Focus(item);
        }

        internal override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (mouseDown)
            {
                TreeViewExItem item = GetTreeViewItemUnderMouse(e.GetPosition(treeViewEx));
                if (item == null) return;
                if (e.ChangedButton != MouseButton.Left) return;
                if (item.IsEditing) return;

                SelectSingleItem(item);

                FocusHelper.Focus(item);
            }
            mouseDown = false;
        }

        private void SelectWithShift(TreeViewExItem item)
        {
            object firstSelectedItem;
            if (lastShiftRoot != null)
            {
                firstSelectedItem = lastShiftRoot;
            }
            else
            {
                firstSelectedItem = treeViewEx.SelectedItems.Count > 0 ? treeViewEx.SelectedItems[0] : null;
            }

            TreeViewExItem shiftRootItem = treeViewEx.GetTreeViewItemsFor(new List<object> { firstSelectedItem }).First();

            List<object> itemsToSelect = treeViewEx.GetNodesToSelectBetween(shiftRootItem, item).Select(x => x.DataContext).ToList();
            List<object> itemsToUnSelect = ((IEnumerable<object>)treeViewEx.SelectedItems).ToList();

            ModifySelection(itemsToSelect, itemsToUnSelect);
        }
        #endregion

        #region Methods called by BorderSelection

        internal void UnSelectByRectangle(TreeViewExItem item)
        {
            if (!treeViewEx.CheckSelectionAllowed(item.DataContext, false)) return;

            treeViewEx.SelectedItems.Remove(item.DataContext);
            if (item.DataContext == lastShiftRoot)
            {
                lastShiftRoot = null;
            }
        }

        internal void SelectByRectangle(List<object> itemsToSelect, List<object> itemsToUnselect)
        {
            if (itemsToSelect == null) itemsToSelect = new List<object>();
            if (itemsToUnselect == null) itemsToUnselect = new List<object>();

            ModifySelection(itemsToSelect, itemsToUnselect);
        }

        #endregion

        public override void SelectFromProperty(TreeViewExItem item, bool isSelected)
        {
            if (isSelected)
            {
                lastShiftRoot = item.DataContext;
            }
            base.SelectFromProperty(item, isSelected);
        }

        public override void ClearObsoleteItems(IList items)
        {
            base.ClearObsoleteItems(items);
            if (items.Contains(lastShiftRoot))
                lastShiftRoot = null;
        }
    }
}
