// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Windows.Controls;

namespace SiliconStudio.Presentation.Controls
{
    public static class TreeViewElementFinder
    {
        public static TreeViewItem FindNext(TreeViewItem treeViewItem, bool visibleOnly)
        {
            while (true)
            {
                // find first child
                if (treeViewItem.IsExpanded || !visibleOnly)
                {
                    var item = GetFirstVirtualizedItem(treeViewItem);
                    if (item != null)
                    {
                        if (item.IsEnabled && !visibleOnly || item.IsVisible)
                        {
                            return item;
                        }
                        treeViewItem = item;
                        continue;
                    }
                }

                // find next sibling
                var sibling = FindNextSiblingRecursive(treeViewItem) as TreeViewItem;
                return sibling != null ? (!visibleOnly || sibling.IsVisible ? sibling : null) : null;
            }
        }

        public static TreeViewItem GetFirstVirtualizedItem(TreeViewItem treeViewItem)
        {
            for (var i = 0; i < treeViewItem.Items.Count; i++)
            {
                var item = treeViewItem.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (item != null)
                    return item;
            }

            return null;
        }

        public static ItemsControl FindNextSibling(ItemsControl itemsControl)
        {
            var parentIc = ItemsControl.ItemsControlFromItemContainer(itemsControl);
            if (parentIc == null)
                return null;

            var index = parentIc.ItemContainerGenerator.IndexFromContainer(itemsControl);
            return parentIc.ItemContainerGenerator.ContainerFromIndex(index + 1) as ItemsControl; // returns null if index to large or nothing found
        }

        /// <summary>
        /// Returns the first item. If tree is virtualized, it is the first realized item.
        /// </summary>
        /// <param name="treeView">The tree.</param>
        /// <param name="visibleOnly">If true, returns the first visible item.</param>
        /// <returns>Returns a TreeViewItem.</returns>
        public static TreeViewItem FindFirst(TreeView treeView, bool visibleOnly)
        {
            for (var i = 0; i < treeView.Items.Count; i++)
            {
                var item = treeView.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (item == null) continue;
                if (!visibleOnly || item.IsVisible) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns the last item. If tree is virtualized, it is the last realized item.
        /// </summary>
        /// <param name="treeView">The tree.</param>
        /// <param name="visibleOnly">If true, returns the last visible item.</param>
        /// <returns>Returns a TreeViewItem.</returns>
        public static TreeViewItem FindLast(TreeView treeView, bool visibleOnly)
        {
            for (var i = treeView.Items.Count - 1; i >= 0; i--)
            {
                var item = treeView.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (item == null) continue;
                if (!visibleOnly || item.IsVisible) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns all items in tree recursively. If virtualization is enabled, only realized items are returned.
        /// </summary>
        /// <param name="treeView">The tree.</param>
        /// <param name="visibleOnly">True if only visible items should be returned.</param>
        /// <returns>Returns an enumerable of items.</returns>
        public static IEnumerable<TreeViewItem> FindAll(TreeView treeView, bool visibleOnly)
        {
            var currentItem = FindFirst(treeView, visibleOnly);
            while (currentItem != null)
            {
                if (!visibleOnly || currentItem.IsVisible) yield return currentItem;
                currentItem = FindNext(currentItem, visibleOnly);
            }
        }

        private static ItemsControl FindNextSiblingRecursive(ItemsControl itemsControl)
        {
            while (true)
            {
                var parentIc = ItemsControl.ItemsControlFromItemContainer(itemsControl);
                if (parentIc == null)
                    return null;
                var index = parentIc.ItemContainerGenerator.IndexFromContainer(itemsControl);
                if (index < parentIc.Items.Count - 1)
                {
                    return parentIc.ItemContainerGenerator.ContainerFromIndex(index + 1) as ItemsControl; // returns null if index to large or nothing found
                }

                itemsControl = parentIc;
            }
        }
    }
}
