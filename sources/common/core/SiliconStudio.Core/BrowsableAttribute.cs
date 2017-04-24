// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_PLATFORM_UWP || SILICONSTUDIO_RUNTIME_CORECLR
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
