// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.ObjectModel;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// A collection of <see cref="ImageEffectStep"/>
    /// </summary>
    public class ImageEffectStepCollection : Collection<ImageEffectStep>
    {
        protected override void ClearItems()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                var element = base[i];
                if (!element.IsBuiltin)
                {
                    RemoveAt(i);
                }
            }
        }

        protected override void InsertItem(int index, ImageEffectStep item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "Cannot add a null ImageEffectStep");
            }
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            if (base[index].IsBuiltin)
            {
                throw new ArgumentException("Cannot remove builtin readonly effect step [{0}]".ToFormat(base[index]));
            }
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, ImageEffectStep item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "Cannot add a null ImageEffectStep");
            }
            if (base[index].IsBuiltin)
            {
                throw new ArgumentException("Cannot replace builtin readonly effect step [{0}]".ToFormat(base[index]));
            }
            base.SetItem(index, item);
        }
    }
}