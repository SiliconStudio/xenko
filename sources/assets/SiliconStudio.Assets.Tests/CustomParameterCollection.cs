// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Assets.Tests
{
    /// <summary>
    /// Fake a ParameterCollection using a dictionary of PropertyKey, object
    /// </summary>
    [DataContract]
    public class CustomParameterCollection : Dictionary<PropertyKey, object>
    {

        public void Set(PropertyKey key, object value)
        {
            this[key] = value;
        }
    }
}
