// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME 
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
