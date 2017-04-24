// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Core.Serialization.Serializers
{
    [Flags]
    public enum ComplexTypeSerializerFlags
    {
        SerializePublicFields = 1,
        SerializeNonPublicFields = 2,
        SerializeFields = SerializePublicFields | SerializeNonPublicFields,
        SerializePublicProperties = 4,
    }
}
