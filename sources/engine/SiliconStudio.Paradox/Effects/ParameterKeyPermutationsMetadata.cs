// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects
{
    public class ParameterKeyPermutationsMetadata : PropertyKeyMetadata
    {
        public ParameterKeyPermutationsMetadata(params object[] values)
        {
            Values = values;
        }

        public object[] Values { get; private set; }
    }
}