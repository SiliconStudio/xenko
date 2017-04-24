// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.UI
{
    [Flags]
    public enum DependencyPropertyFlags
    {
        Default = 0,
        ReadOnly = 0x1,
        Attached = 0x2,
    }

    public class DependencyPropertyKeyMetadata : PropertyKeyMetadata
    {
        public static readonly DependencyPropertyKeyMetadata Attached = new DependencyPropertyKeyMetadata(DependencyPropertyFlags.Attached);

        public static readonly DependencyPropertyKeyMetadata AttachedReadOnly = new DependencyPropertyKeyMetadata(DependencyPropertyFlags.Attached | DependencyPropertyFlags.ReadOnly);

        public static readonly DependencyPropertyKeyMetadata Default = new DependencyPropertyKeyMetadata(DependencyPropertyFlags.Default);

        public static readonly DependencyPropertyKeyMetadata ReadOnly = new DependencyPropertyKeyMetadata(DependencyPropertyFlags.ReadOnly);

        internal DependencyPropertyKeyMetadata(DependencyPropertyFlags flags)
        {
            Flags = flags;
        }

        public DependencyPropertyFlags Flags { get; }
    }
}
