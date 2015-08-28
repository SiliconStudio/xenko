using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using SiliconStudio.Presentation.Collections;

namespace System.Windows.Controls
{
    internal class Selection : InputSubscriberBase
    {
        protected TreeViewEx TreeViewEx;
        private bool mouseDown;
        private object lastShiftRoot;

        internal Selection(TreeViewEx treeViewEx)
        {
            TreeViewEx = treeViewEx;
        }

        internal static bool IsControlKeyDown => (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

        internal static bool IsShiftKeyDown => (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

        internal bool AllowMultipleSelection { get; set; }

        internal virtual void SelectFromUiAutomation(TreeViewExItem item)
        {
            SelectSingleItem(item);
            FocusHelper.Focus(item);
        }

        internal virtual void SelectPreviousFromKey()
        {
            List<TreeViewExItem> items = TreeViewElementFinder.FindAll(TreeViewEx, true).ToList();
            TreeViewExItem item = GetFocusedItem();
            item = TreeViewEx.GetPreviousItem(item, items);
            if (item == null) return;

            // if ctrl is pressed just focus it, so it can be selected by space. Otherwise select it.
            if (!IsControlKeyDown)
            {
                SelectSingleItem(item);
            }

            FocusHelper.Focus(item);
        }

        internal virtual void SelectNextFromKey()
        {
            TreeViewExItem item = GetFocusedItem();
            item = TreeViewElementFinder.FindNext(item, true);
            if (item == null) return;

            // if ctrl is pressed just focus it, so it can be selected by space. Otherwise select it.
            if (!IsControlKeyDown)
            {
                SelectSingleItem(item);
            }

            FocusHelper.Focus(item);
        }

        internal virtual void SelectCurrentBySpace()
        {
            TreeViewExItem item = GetFocusedItem();
            SelectSingleItem(item);
            FocusHelper.Focus(item);
        }

        internal virtual void SelectFromProperty(TreeViewExItem item, bool isSelected)
        {
            // we do not check if selection is allowed, because selecting on that way is no user action.
            // Hopefully the programmer knows what he does...
            if (isSelected)
            {
                if (AllowMultipleSelection)
                {
                    lastShiftRoot = item.DataContext;
                }
                TreeViewEx.SelectedItems.Add(item.DataContext);
                FocusHelper.Focus(item);
            }
            else
            {
                TreeViewEx.SelectedItems.Remove(item.DataContext);
            }
        }

        internal virtual void SelectFirst()
        {
            var item = TreeViewElementFinder.FindFirst(TreeViewEx, true);
            if (item != null)
            {
                SelectSingleItem(item);
            }

            FocusHelper.Focus(item);
        }

        internal virtual void SelectLast()
        {
            var item = TreeViewElementFinder.FindLast(TreeViewEx, true);
            if (item != null)
            {
                SelectSingleItem(item);
            }

            FocusHelper.Focus(item);
        }

        internal virtual void ClearObsoleteItems(IList items)
        {
            foreach (var itemToUnSelect in items)
            {
                TreeViewEx.SelectedItems.Remove(itemToUnSelect);
            }
            if (AllowMultipleSelection && items.Contains(lastShiftRoot))
                lastShiftRoot = null;
        }

        internal override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            mouseDown = e.ChangedButton == MouseButton.Left;

            TreeViewExItem item = GetTreeViewItemUnderMouse(e.GetPosition(TreeViewEx));
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
                TreeViewExItem item = GetTreeViewItemUnderMouse(e.GetPosition(TreeViewEx));
                if (item == null) return;
                if (e.ChangedButton != MouseButton.Left) return;
                if (item.IsEditing) return;

                SelectSingleItem(item);

                FocusHelper.Focus(item);
            }
            mouseDown = false;
        }

        protected void SelectSingleItem(TreeViewExItem item)
        {
            if (AllowMultipleSelection)
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
            else
            {
                if (IsControlKeyDown)
                {
                    ToggleItem(item);
                }
                else
                {
                    ModifySelection(item.DataContext);
                }
            }
        }

        protected TreeViewExItem GetFocusedItem()
        {
            return TreeViewElementFinder.FindAll(TreeViewEx, false).FirstOrDefault(x => x.IsFocused);
        }

        private void ToggleItem(TreeViewExItem item)
        {
            if (item.DataContext == null)
                return;

            if (AllowMultipleSelection)
            {
                if (TreeViewEx.SelectedItems.Contains(item.DataContext))
                {
                    ModifySelection(new List<object>(), new List<object>(1) { item.DataContext });
                }
                else
                {
                    ModifySelection(new List<object>(1) { item.DataContext }, new List<object>());
                }
            }
            else
            {
                ModifySelection(TreeViewEx.SelectedItem == item.DataContext ? null : item.DataContext);
            }
        }

        private void ModifySelection(object itemToSelect)
        {
            TreeViewEx.SelectedItem = itemToSelect;
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
    }
}