// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Input
{
    public enum TextInputEventType
    {
        /// <summary>
        /// When new text is entered
        /// </summary>
        Input,

        /// <summary>
        /// This the current text that has not yet been entered but is still being edited
        /// </summary>
        Composition,
    }
}