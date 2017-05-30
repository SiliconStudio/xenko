// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.AssemblyProcessor
{
    [Flags]
    public enum ComplexTypeSerializerFlags
    {
        SerializePublicFields = 1,
        SerializePublicProperties = 2,

        /// <summary>
        /// If the member has DataMemberIgnore and DataMemberUpdatable, it will be included
        /// </summary>
        Updatable = 4,
    }
}
