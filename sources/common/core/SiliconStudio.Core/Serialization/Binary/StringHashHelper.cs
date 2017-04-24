// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Serialization
{
    internal static class StringHashHelper
    {
        public static uint GetSerializerHashCode([NotNull] this string param)
        {
            uint result = 0;
            foreach (var c in param)
            {
                result ^= result << 4;
                result ^= result << 24;
                result ^= c;
            }
            return result;
        }
    }
}
