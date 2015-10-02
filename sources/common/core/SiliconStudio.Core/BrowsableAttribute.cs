// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME || SILICONSTUDIO_RUNTIME_CORECLR
namespace System.ComponentModel
{
    public class BrowsableAttribute : Attribute
    {
        public BrowsableAttribute(bool browsable)
        {
            Browsable = browsable;
        }

        public bool Browsable { get; private set; }
    }
}
#endif