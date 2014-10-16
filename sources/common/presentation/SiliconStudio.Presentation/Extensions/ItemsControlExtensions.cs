// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SiliconStudio.Presentation.Extensions
{
    public static class ItemsControlExtensions
    {
        public static ItemsControl GetParentContainer(this ItemsControl itemsControl)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(itemsControl);

            while (parent != null && (parent is ItemsControl) == false)
                parent = VisualTreeHelper.GetParent(parent);

            return parent as ItemsControl;
        }

        public static IEnumerable<ItemsControl> GetChildContainers(this ItemsControl itemsControl)
        {
            ItemContainerGenerator gen = itemsControl.ItemContainerGenerator;

            foreach (var item in gen.Items)
            {
                var container = gen.ContainerFromItem(item) as ItemsControl;
                if (container != null)
                    yield return container;
            }
        }
    }
}
