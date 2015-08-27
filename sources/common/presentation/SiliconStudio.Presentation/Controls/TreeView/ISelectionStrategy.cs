using System.Collections.Generic;
namespace System.Windows.Controls
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    internal interface ISelectionStrategy
    {
        void SelectFromUiAutomation(TreeViewExItem item);

        void SelectPreviousFromKey();

        void SelectNextFromKey();

        void SelectCurrentBySpace();

        void SelectFromProperty(TreeViewExItem item, bool isSelected);

        void SelectFirst();

        void SelectLast();

        void ClearObsoleteItems(IEnumerable<object> items);
    }
}
