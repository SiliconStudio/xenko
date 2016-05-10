// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// This behavior allows more convenient editing of the value of a char using a TextBox.
    /// </summary>
    public class CharInputBehavior : Behavior<TextBox>
    {
        private bool updatingText;

        protected override void OnAttached()
        {
            AssociatedObject.TextChanged += TextChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.TextChanged -= TextChanged;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (updatingText)
                return;

            char newChar = default(char);
            foreach (var change in e.Changes.Where(change => change.AddedLength > 0))
            {
                newChar = AssociatedObject.Text[change.Offset + change.AddedLength - 1];
            }
            if (newChar != default(char))
            {
                updatingText = true;
                AssociatedObject.Text = newChar.ToString(CultureInfo.InvariantCulture);
                updatingText = false;
            }

            // Update the binding source on each change
            BindingExpression expression = AssociatedObject.GetBindingExpression(TextBox.TextProperty);
            expression?.UpdateSource();
        }
    }
}
