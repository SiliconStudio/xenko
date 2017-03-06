// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.Extensions
{
    public static class ItemsControlExtensions
    {
        [CanBeNull]
        public static ItemsControl GetParentContainer([NotNull] this ItemsControl itemsControl)
        {
            var parent = VisualTreeHelper.GetParent(itemsControl);

            while (parent != null && (parent is ItemsControl) == false)
                parent = VisualTreeHelper.GetParent(parent);

            return parent as ItemsControl;
        }

        public static IEnumerable<ItemsControl> GetChildContainers([NotNull] this ItemsControl itemsControl)
        {
            var gen = itemsControl.ItemContainerGenerator;

            foreach (var item in gen.Items)
            {
                var container = gen.ContainerFromItem(item) as ItemsControl;
                if (container != null)
                    yield return container;
            }
        }
    }
}
