// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Debugger.Target
{
    /// <summary>
    /// Represents a loaded assembly in the debug process.
    /// </summary>
    [Serializable]
    public struct DebugAssembly : IEquatable<DebugAssembly>
    {
        private int id;

        public static readonly DebugAssembly Empty = new DebugAssembly(0);

        internal DebugAssembly(int id)
        {
            this.id = id;
        }

        public bool Equals(DebugAssembly other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is DebugAssembly && Equals((DebugAssembly)obj);
        }

        public override int GetHashCode()
        {
            return id;
        }
    }
}
