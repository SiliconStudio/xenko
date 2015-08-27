using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace System.Windows.Controls
{
    /// <summary>
    /// Logic for the single selection
    /// </summary>
    internal class SelectionSingle : SelectionStrategyBase
    {
        #region Private fields and constructor

        private readonly TreeViewEx treeViewEx;
        private bool mouseDown;

        public SelectionSingle(TreeViewEx treeViewEx) : base(treeViewEx)
        {
            this.treeViewEx = treeViewEx;
        }
        #endregion

        #region Properties

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

        protected override void SelectSingleItem(TreeViewExItem item)
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
	}
}
