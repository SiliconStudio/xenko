// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.BuildEngine
{
    public class TagSymbol
    {
        private string name;
        private Func<string> computeRealName;

        public string Name { get { return name; } }
        public string RealName { get { return computeRealName != null ? computeRealName() : name; } }

        public TagSymbol(string name, Func<string> computeRealName = null)
        {
            this.name = name;
            this.computeRealName = computeRealName;
        }
    }
}
