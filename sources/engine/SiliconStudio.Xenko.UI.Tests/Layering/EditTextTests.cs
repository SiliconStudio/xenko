// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Layering
{
    /// <summary>
    /// A class that contains test functions for layering of the <see cref="EditText"/> class.
    /// </summary>
    [TestFixture]
    [System.ComponentModel.Description("Tests for EditText layering")]
    public class EditTextTests
    {
        /// <summary>
        /// Test the invalidations generated object property changes.
        /// </summary>
        [Test]
        public void TestBasicInvalidations()
        {
            var services = new ServiceRegistry();
            services.AddService(typeof(IGame), new Game());

            var edit = new EditText();
            edit.UIElementServices = new UIElementServices { Services = services };

            // - test the properties that are supposed to invalidate the object measurement
            UIElementLayeringTests.TestMeasureInvalidation(edit, () => edit.Font = null);
            UIElementLayeringTests.TestMeasureInvalidation(edit, () => edit.MaxLines = 34);
            UIElementLayeringTests.TestMeasureInvalidation(edit, () => edit.MinLines = 34);
            UIElementLayeringTests.TestMeasureInvalidation(edit, () => edit.SelectedText = "toto");
            UIElementLayeringTests.TestMeasureInvalidation(edit, () => edit.Text = "titi");
            UIElementLayeringTests.TestMeasureInvalidation(edit, () => edit.MaxLength = 3); // text is modified

            // - test the properties that are not supposed to invalidate the object layout state
            UIElementLayeringTests.TestNoInvalidation(edit, () => edit.IsReadOnly = !edit.IsReadOnly);
            UIElementLayeringTests.TestNoInvalidation(edit, () => edit.ActiveImage = (SpriteFromTexture)new Sprite());
            UIElementLayeringTests.TestNoInvalidation(edit, () => edit.InactiveImage = (SpriteFromTexture)new Sprite());
            UIElementLayeringTests.TestNoInvalidation(edit, () => edit.TextColor = new Color(1, 2, 3, 4));
            UIElementLayeringTests.TestNoInvalidation(edit, () => edit.SelectionColor = new Color(1, 2, 3, 4));
            UIElementLayeringTests.TestNoInvalidation(edit, () => edit.CaretColor = new Color(1, 7, 3, 4));
            UIElementLayeringTests.TestNoInvalidation(edit, () => edit.CaretPosition = 34);
            UIElementLayeringTests.TestNoInvalidation(edit, () => edit.SelectionLength = 2);
            UIElementLayeringTests.TestNoInvalidation(edit, () => edit.SelectionStart = 0);
            UIElementLayeringTests.TestNoInvalidation(edit, () => edit.MaxLength = 34); // text is not modified
        }
    }
}
