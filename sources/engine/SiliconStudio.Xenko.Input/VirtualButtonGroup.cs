// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.ObjectModel;
using System.Text;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A combination of <see cref="IVirtualButton"/>, by default evaluated as the operator '&&' to produce a value if all buttons are pressed.
    /// </summary>
    public class VirtualButtonGroup : Collection<IVirtualButton>, IVirtualButton
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is determining the value as a disjunction ('||' operator between buttons), false by default ('&&' operator).
        /// </summary>
        /// <value><c>true</c> if this instance is disjunction; otherwise, <c>false</c>.</value>
        public bool IsDisjunction { get; set; }

        protected override void InsertItem(int index, IVirtualButton item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "Cannot set null instance of VirtualButton");
            }

            if (!Contains(item))
            {
                base.InsertItem(index, item);
            }
        }

        protected override void SetItem(int index, IVirtualButton item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "Cannot add null instance of VirtualButton");
            }

            if (!Contains(item))
            {
                base.SetItem(index, item);
            }
        }

        /// <summary>
        /// Implements the + operator.
        /// </summary>
        /// <param name="combination">The combination.</param>
        /// <param name="virtualButton">The virtual button.</param>
        /// <returns>The result of the operator.</returns>
        public static VirtualButtonGroup operator +(VirtualButtonGroup combination, IVirtualButton virtualButton)
        {
            combination.Add(virtualButton);
            return combination;
        }

        public virtual float GetValue(InputManager manager)
        {
            float value = 0.0f;
            foreach (var virtualButton in Items)
            {
                float newValue = virtualButton != null ? virtualButton.GetValue(manager) : 0.0f;

                // In case of a || (disjunction) set, we return the latest non-zero value.
                if (IsDisjunction)
                {
                    if (newValue != 0.0f)
                    {
                        value = newValue;
                    }
                }
                else
                {
                    // In case of a && (conjunction) set, we return the last non-zero value unless there is a zero value.
                    if (newValue == 0.0f)
                    {
                        return 0.0f;
                    }

                    value = newValue;
                }
            }
            return value;
        }

        public override string ToString()
        {
            var text = new StringBuilder();
            for (int i = 0; i < Items.Count; i++)
            {
                var virtualButton = Items[i];
                if (i > 0)
                {
                    text.Append(IsDisjunction ? " || " : " && ");
                }
                text.AppendFormat("{0}", virtualButton);
            }
            return text.ToString();
        }
    }
}
