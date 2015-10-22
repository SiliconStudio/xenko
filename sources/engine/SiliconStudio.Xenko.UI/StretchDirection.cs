// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Describes how scaling applies to content and restricts scaling to named axis types.
    /// </summary>
    public enum StretchDirection
    {
        /// <summary>
        /// The content stretches to fit the parent according to the Stretch mode.
        /// </summary>
        Both,
        /// <summary>
        /// The content scales downward only when it is larger than the parent. If the content is smaller, no scaling upward is performed.
        /// </summary>
        DownOnly,
        /// <summary>
        /// The content scales upward only when it is smaller than the parent. If the content is larger, no scaling downward is performed.
        /// </summary>
        UpOnly,
    }
}