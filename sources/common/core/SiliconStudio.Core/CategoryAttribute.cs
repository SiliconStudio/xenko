// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_PLATFORM_UWP 
namespace System.ComponentModel
{
    public class CategoryAttribute : Attribute
    {
        public CategoryAttribute()
        {
        }

        public CategoryAttribute(string category)
        {
            this.Category = category;
        }

        public string Category { get; private set; }
    }
}
#endif
