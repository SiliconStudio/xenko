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
        public SelectionSingle(TreeViewEx treeViewEx) : base(treeViewEx)
        {
        }

        private void ToggleItem(TreeViewExItem item)
        {
            ModifySelection(TreeViewEx.SelectedItem == item.DataContext ? null : item.DataContext);
        }

        private void ModifySelection(object itemToSelect)
        {
            TreeViewEx.SelectedItem = itemToSelect;
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
	}
}
