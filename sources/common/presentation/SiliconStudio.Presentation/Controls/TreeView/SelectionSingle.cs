using System.Windows.Input;

namespace System.Windows.Controls
{
    using Collections.Generic;
	using Linq;

    /// <summary>
    /// Logic for the single selection
    /// </summary>
    internal class SelectionSingle : InputSubscriberBase, ISelectionStrategy
	{
        #region Private fields and constructor

        private readonly TreeViewEx treeViewEx;
        private bool mouseDown;

        public SelectionSingle(TreeViewEx treeViewEx)
        {
            this.treeViewEx = treeViewEx;
        }
        #endregion

        #region Properties

        internal static bool IsControlKeyDown
        {
            get
            {
                return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            }
        }

        #endregion

        #region Private helper methods

        private TreeViewExItem GetFocusedItem()
        {
            foreach (var item in TreeViewElementFinder.FindAll(treeViewEx, false))
            {
                if (item.IsFocused) return item;
            }

            return null;
        }
        #endregion

        #region Private modify selection methods

        private void ToggleItem(TreeViewExItem item)
        {
            ModifySelection(treeViewEx.SelectedItem == item.DataContext ? null : item.DataContext);
        }

        private void ModifySelection(object itemToSelect)
        {
            // notify listeners what is about to change.
            // Let them cancel and/or handle the selection list themself
            bool allowed = treeViewEx.CheckSelectionAllowed(itemToSelect, treeViewEx.SelectedItem);
            if (!allowed)
                return;

            treeViewEx.SelectedItem = itemToSelect;
        }

	    private void SelectSingleItem(TreeViewExItem item)
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
        #endregion

        #region ISelectionStrategy Members

        public void SelectFromUiAutomation(TreeViewExItem item)
        {
            SelectSingleItem(item);

            FocusHelper.Focus(item);
        }

        public void SelectFromProperty(TreeViewExItem item, bool isSelected)
        {
            // we do not check if selection is allowed, because selecting on that way is no user action.
            // Hopefully the programmer knows what he does...
            if (isSelected)
            {
                treeViewEx.SelectedItems.Add(item.DataContext);
                FocusHelper.Focus(item);
            }
            else
            {
                treeViewEx.SelectedItems.Remove(item.DataContext);
            }
        }

        public void SelectFirst()
        {
            TreeViewExItem item = TreeViewElementFinder.FindFirst(treeViewEx, true);
            if (item != null)
            {
                SelectSingleItem(item);
            }

            FocusHelper.Focus(item);
        }

        public void SelectLast()
        {
            TreeViewExItem item = TreeViewElementFinder.FindLast(treeViewEx, true);
            if (item != null)
            {
                SelectSingleItem(item);
            }

            FocusHelper.Focus(item);
        }

        public void SelectNextFromKey()
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

        public void SelectPreviousFromKey()
        {
            List<TreeViewExItem> items = TreeViewElementFinder.FindAll(treeViewEx, true).ToList();
            TreeViewExItem item = GetFocusedItem();
            item = treeViewEx.GetPreviousItem(item, items);
            if (item == null) return;

            // if ctrl is pressed just focus it, so it can be selected by space. Otherwise select it.
            if (!IsControlKeyDown)
            {
                SelectSingleItem(item);
            }

            FocusHelper.Focus(item);
        }

        public void SelectCurrentBySpace()
        {
            TreeViewExItem item = GetFocusedItem();
            SelectSingleItem(item);
            FocusHelper.Focus(item);
        }

        public void ClearObsoleteItems(IEnumerable<object> items)
        {
            foreach (object itemToUnSelect in items)
            {
                if (treeViewEx.SelectedItems.Contains(itemToUnSelect)) treeViewEx.SelectedItems.Remove(itemToUnSelect);
            }
        }

        #endregion
	}
}
