using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace System.Windows.Controls
{
    internal abstract class SelectionStrategyBase : InputSubscriberBase, ISelectionStrategy
    {
        protected TreeViewEx TreeViewEx;
        private bool mouseDown;

        protected SelectionStrategyBase(TreeViewEx treeViewEx)
        {
            TreeViewEx = treeViewEx;
        }

        internal static bool IsControlKeyDown => (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

        internal static bool IsShiftKeyDown => (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

        public virtual void SelectFromUiAutomation(TreeViewExItem item)
        {
            SelectSingleItem(item);
            FocusHelper.Focus(item);
        }

        public virtual void SelectPreviousFromKey()
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

        public virtual void SelectNextFromKey()
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

        public virtual void SelectCurrentBySpace()
        {
            TreeViewExItem item = GetFocusedItem();
            SelectSingleItem(item);
            FocusHelper.Focus(item);
        }

        public virtual void SelectFromProperty(TreeViewExItem item, bool isSelected)
        {
            // we do not check if selection is allowed, because selecting on that way is no user action.
            // Hopefully the programmer knows what he does...
            if (isSelected)
            {
                TreeViewEx.SelectedItems.Add(item.DataContext);
                FocusHelper.Focus(item);
            }
            else
            {
                TreeViewEx.SelectedItems.Remove(item.DataContext);
            }
        }

        public virtual void SelectFirst()
        {
            var item = TreeViewElementFinder.FindFirst(TreeViewEx, true);
            if (item != null)
            {
                SelectSingleItem(item);
            }

            FocusHelper.Focus(item);
        }

        public virtual void SelectLast()
        {
            var item = TreeViewElementFinder.FindLast(TreeViewEx, true);
            if (item != null)
            {
                SelectSingleItem(item);
            }

            FocusHelper.Focus(item);
        }

        public virtual void ClearObsoleteItems(IList items)
        {
            foreach (var itemToUnSelect in items)
            {
                TreeViewEx.SelectedItems.Remove(itemToUnSelect);
            }
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

        protected abstract void SelectSingleItem(TreeViewExItem item);

        protected TreeViewExItem GetFocusedItem()
        {
            return TreeViewElementFinder.FindAll(TreeViewEx, false).FirstOrDefault(x => x.IsFocused);
        }
    }
}