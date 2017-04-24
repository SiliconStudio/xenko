// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Handle used to query logical group information.
    /// </summary>
    public struct LogicalGroupReference
    {
        public static readonly LogicalGroupReference Invalid = new LogicalGroupReference(-1);

        internal int Index;

        internal LogicalGroupReference(int index)
        {
            Index = index;
        }
    }
}
