// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using NUnit.Framework;

using SiliconStudio.Xenko.UI.Panels;

namespace SiliconStudio.Xenko.UI.Tests.Layering
{
    /// <summary>
    /// A class that contains test functions for layering of the <see cref="GridBase"/> class.
    /// </summary>
    [TestFixture]
    [System.ComponentModel.Description("Tests for GridBase layering")]
    public class GridBaseTests : GridBase
    {
        /// <summary>
        /// Test the invalidations generated object property changes.
        /// </summary>
        [Test]
        public void TestBasicInvalidations()
        {
            var grid = new UniformGrid { Rows = 2, Columns = 2, Layers = 2 };
            var child = new UniformGrid();
            grid.Children.Add(child);

            // - test the properties that are not supposed to invalidate the object layout state
            UIElementLayeringTests.TestMeasureInvalidation(grid, () => child.DependencyProperties.Set(ColumnPropertyKey, 2));
            UIElementLayeringTests.TestMeasureInvalidation(grid, () => child.DependencyProperties.Set(RowPropertyKey, 2));
            UIElementLayeringTests.TestMeasureInvalidation(grid, () => child.DependencyProperties.Set(LayerPropertyKey, 2));
            UIElementLayeringTests.TestMeasureInvalidation(grid, () => child.DependencyProperties.Set(ColumnSpanPropertyKey, 2));
            UIElementLayeringTests.TestMeasureInvalidation(grid, () => child.DependencyProperties.Set(RowSpanPropertyKey, 2));
            UIElementLayeringTests.TestMeasureInvalidation(grid, () => child.DependencyProperties.Set(LayerSpanPropertyKey, 2));
        }
    }
}
