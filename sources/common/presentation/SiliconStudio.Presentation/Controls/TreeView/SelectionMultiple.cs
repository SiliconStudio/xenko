using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Presentation.Collections;

namespace System.Windows.Controls
{
    /// <summary>
    /// Logic for the multiple selection
    /// </summary>
    internal class SelectionMultiple : SelectionStrategyBase
    {
        private object lastShiftRoot;

        public SelectionMultiple(TreeViewEx treeViewEx) : base(treeViewEx)
        {
        }

        private void ToggleItem(TreeViewExItem item)
        {
            if (item.DataContext == null)
                return;

            if (TreeViewEx.SelectedItems.Contains(item.DataContext))
            {
                ModifySelection(new List<object>(), new List<object>(1) { item.DataContext });
            }
            else
            {
                ModifySelection(new List<object>(1) { item.DataContext }, new List<object>());
            }
        }

        private void ModifySelection(List<object> itemsToSelect, List<object> itemsToUnselect)
        {
            //clean up any duplicate or unnecessery input
            // check for itemsToUnselect also in itemsToSelect
            foreach (var item in itemsToSelect)
            {
                itemsToUnselect.Remove(item);
            }

            // check for itemsToSelect already in SelectedItems
            foreach (var item in TreeViewEx.SelectedItems)
            {
                itemsToSelect.Remove(item);
            }

            // check for itemsToUnSelect not in SelectedItems
            foreach (var item in itemsToUnselect.Where(x => !TreeViewEx.SelectedItems.Contains(x)).ToList())
            {
                itemsToUnselect.Remove(item);
            }

            //check if there's anything to do.
            if (itemsToSelect.Count == 0 && itemsToUnselect.Count == 0)
                return;

            // Unselect and then select items
            foreach (var itemToUnSelect in itemsToUnselect)
            {
                TreeViewEx.SelectedItems.Remove(itemToUnSelect);
            }

            ((NonGenericObservableListWrapper<object>)TreeViewEx.SelectedItems).AddRange(itemsToSelect);

            if (itemsToUnselect.Contains(lastShiftRoot))
                lastShiftRoot = null;

            if (!(TreeView.SelectedItems.Contains(lastShiftRoot) && IsShiftKeyDown))
                lastShiftRoot = itemsToSelect.LastOrDefault();
        }

        protected override void SelectSingleItem(TreeViewExItem item)
        {

            // selection with SHIFT is not working in virtualized mode. Thats because the Items are not visible.
            // Therefor the children cannot be found/selected.
            if (IsShiftKeyDown && TreeViewEx.SelectedItems.Count > 0 && !TreeViewEx.IsVirtualizing)
            {
                SelectWithShift(item);
            }
            else if (IsControlKeyDown)
            {
                ToggleItem(item);
            }
            else
            {
                TreeViewEx.SelectedItems.Clear();
                ModifySelection(new List<object>(1) { item.DataContext }, new List<object>());
            }
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
                firstSelectedItem = TreeViewEx.SelectedItems.Count > 0 ? TreeViewEx.SelectedItems[0] : null;
            }

            var shiftRootItem = TreeViewEx.GetTreeViewItemsFor(new List<object> { firstSelectedItem }).First();

            var itemsToSelect = TreeViewEx.GetNodesToSelectBetween(shiftRootItem, item).Select(x => x.DataContext).ToList();
            var itemsToUnSelect = ((IEnumerable<object>)TreeViewEx.SelectedItems).ToList();

            ModifySelection(itemsToSelect, itemsToUnSelect);
        }

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
