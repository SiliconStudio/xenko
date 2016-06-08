// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// An event occurring when an assembly is registered with <see cref="AssemblyRegistry"/>.
    /// </summary>
    public class AssemblyRegisteredEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyRegisteredEventArgs"/> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="categories">The categories.</param>
        public AssemblyRegisteredEventArgs(Assembly assembly, HashSet<string> categories)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (categories == null) throw new ArgumentNullException("categories");
            Assembly = assembly;
            Categories = categories;
        }

        /// <summary>
        /// Gets the assembly that has been registered.
        /// </summary>
        /// <value>The assembly.</value>
        public Assembly Assembly { get; private set; }

        /// <summary>
        /// Gets the new categories registered for the specified <see cref="Assembly"/>
        /// </summary>
        /// <value>The categories.</value>
        public HashSet<string> Categories { get; private set; }
    }
}
