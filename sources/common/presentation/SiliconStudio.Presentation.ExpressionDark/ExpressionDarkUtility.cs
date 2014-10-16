// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.AvalonDock;

namespace SiliconStudio.Presentation.ExpressionDark
{
    public static class ExpressionDarkUtility
    {
        /// <summary>
        /// Applies a new theme, containing the origianl AvalonDock one plus overridden resources.
        /// </summary>
        /// <param name="dockingManager">The DockingManager instance to fix the theme.</param>
        public static void FixExpressionDarkTheme(this DockingManager dockingManager)
        {
            if (dockingManager == null)
                throw new ArgumentNullException("dockingManager");

            dockingManager.Theme = new FixedExpressionDarkTheme();
        }

        /// <summary>
        /// Applies a new theme, containing the original AvalonDock one plus overridden resources.
        /// </summary>
        /// <param name="window">The Window that contains the AvalonDock's DockingManager.</param>
        /// <returns>Returns true if the DockingManager has been found, false otherwise.</returns>
        public static bool FixExpressionDarkTheme(this Window window)
        {
            var dockingManager = FindDockingManager(window);

            if (dockingManager == null)
                return false;

            dockingManager.FixExpressionDarkTheme();

            return true;
        }

        /// <summary>
        /// Finds the DockingManager instance.
        /// </summary>
        /// <param name="root">The node from where to look for the DockingManager.</param>
        /// <returns>Returns the DockingManager instance, null otherwise.</returns>
        private static DockingManager FindDockingManager(DependencyObject root)
        {
            if (root == null)
                return null;

            if (root is DockingManager)
                return (DockingManager)root;

            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                var result = FindDockingManager(child as DependencyObject);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
