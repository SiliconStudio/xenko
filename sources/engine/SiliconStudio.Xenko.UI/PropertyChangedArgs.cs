// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// An argument class containing information about a property that changed.
    /// </summary>
    /// <typeparam name="T">The type of the property that changed</typeparam>
    public class PropertyChangedArgs<T>
    {
        public T OldValue { get; internal set; }
        public T NewValue { get; internal set; }
    }
}
