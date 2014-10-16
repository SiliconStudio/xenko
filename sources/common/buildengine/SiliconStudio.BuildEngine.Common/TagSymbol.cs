// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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