// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// Flags that can be applied to a <see cref="IModelNode"/>.
    /// </summary>
    [Flags]
    public enum ModelNodeFlags
    {
        None = 0,

        /// <summary>
        /// If <see cref="IModelNode"/> is a root, it won't be cached by <see cref="ModelContainer.GetOrCreateModelNode"/>.
        /// </summary>
        DoNotCache = 1,

        /// <summary>
        /// <see cref="DefaultModelBuilder"/> won't attempt to build children nodes out of member.
        /// </summary>
        DoNotVisitMembers = 2,
    }
}