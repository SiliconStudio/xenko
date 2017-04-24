// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Core.MicroThreading
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class XenkoScriptAttribute : Attribute
    {
        public XenkoScriptAttribute(ScriptFlags flags = ScriptFlags.None)
        {
            this.Flags = flags;
        }

        public ScriptFlags Flags { get; set; }
    }
}
