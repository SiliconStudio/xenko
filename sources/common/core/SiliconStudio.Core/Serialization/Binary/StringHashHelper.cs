// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiliconStudio.Core.Serialization
{
    internal static class StringHashHelper
    {
        public static uint GetSerializerHashCode(this string param)
        {
            uint result = 0;
            foreach (char c in param)
            {
                result ^= result << 4;
                result ^= result << 24;
                result ^= c;
            }
            return result;
        }
    }
}
