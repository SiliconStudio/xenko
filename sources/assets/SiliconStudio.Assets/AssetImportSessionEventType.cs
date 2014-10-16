// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Assets
{
    /// <summary>
    /// The type of event <c>begin</c> or <c>end</c> published by <see cref="AssetImportSession.Progress"/>
    /// </summary>
    public enum AssetImportSessionEventType
    {
        /// <summary>
        /// A begin event.
        /// </summary>
        Begin,

        /// <summary>
        /// The end event
        /// </summary>
        End,
    }
}