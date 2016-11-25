// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Core.Reflection
{
    // This class exists only for backward compatibility with previous ~Id. It can be removed once we drop backward support
    public static class IdentifiableHelper
    {
        public static Guid GetId(object instance)
        {
            var shadow = ShadowId.GetOrCreate(instance);
            return shadow.GetId(instance);
        }

        public static void SetId(object instance, Guid id)
        {
            var shadow = ShadowId.GetOrCreate(instance);
            shadow?.SetId(instance, id);
        }
    }
}
