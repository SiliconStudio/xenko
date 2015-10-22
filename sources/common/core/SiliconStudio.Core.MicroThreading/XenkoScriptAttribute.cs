// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.MicroThreading
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ParadoxScriptAttribute : Attribute
    {
        public ParadoxScriptAttribute(ScriptFlags flags = ScriptFlags.None)
        {
            this.Flags = flags;
        }

        public ScriptFlags Flags { get; set; }
    }
}