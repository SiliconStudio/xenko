// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Core.Settings
{
    // Extracted from SiliconStudio.Presentation.Core.Utils. We could move it to somewhere public or shared later if needed.
    static class Utils
    {
        /// <summary>
        /// Updates the given field to the given value. If the field changes, invoke the given action.
        /// </summary>
        /// <typeparam name="T">The type of the field and the value.</typeparam>
        /// <param name="field">The field to update.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="action">The action to invoke if the field has changed.</param>
        public static void SetAndInvokeIfChanged<T>(ref T field, T value, Action action)
        {
            if (action == null) throw new ArgumentNullException("action");
            bool changed = !Equals(field, value);
            if (changed)
            {
                field = value;
                action();
            }
        }
    }
}