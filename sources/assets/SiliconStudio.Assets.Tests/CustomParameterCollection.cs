// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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