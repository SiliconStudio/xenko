// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.Legacy
{
    /// <summary>
    /// Enumerates child properties of a given IViewModel.
    /// </summary>
    public interface IChildrenPropertyEnumerator
    {
        /// <summary>
        /// Generates the child properties.
        /// If parameters are not supported, it should return null so that next IChildrenPropertyEnumerator are used.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="viewModelNode">The view model.</param>
        /// <param name="handled">if set to <c>true</c> [handled].</param>
        void GenerateChildren(ViewModelContext context, IViewModelNode viewModelNode, ref bool handled);
    }
}