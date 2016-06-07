// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// A key to identify a specific profile.
    /// </summary>
    public class ProfilingKey
    {
        internal static readonly HashSet<ProfilingKey> AllKeys = new HashSet<ProfilingKey>();
        internal bool Enabled;
        internal ProfilingKeyFlags Flags;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfilingKey" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ProfilingKey(string name, ProfilingKeyFlags flags = ProfilingKeyFlags.None)
        {
            if (name == null) throw new ArgumentNullException("name");
            Children = new List<ProfilingKey>();
            Name = name;
            Flags = flags;

            lock (AllKeys)
            {
                AllKeys.Add(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfilingKey" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="name">The name.</param>
        /// <exception cref="System.ArgumentNullException">parent</exception>
        public ProfilingKey(ProfilingKey parent, string name, ProfilingKeyFlags flags = ProfilingKeyFlags.None)
        {
            if (parent == null) throw new ArgumentNullException("parent");
            if (name == null) throw new ArgumentNullException("name");
            Children = new List<ProfilingKey>();
            Parent = parent;
            Name = $"{Parent}.{name}";
            Flags = flags;

            lock (AllKeys)
            {
                // Register ourself in parent's children.
                parent.Children?.Add(this);

                AllKeys.Add(this);
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the group.
        /// </summary>
        /// <value>The group.</value>
        public ProfilingKey Parent { get; private set; }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        public List<ProfilingKey> Children { get; private set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
