// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Games
{
    /// <summary>
    /// Interface providing services to deal with the virtual resolution of the game.
    /// The virtual resolution enables the user to perform rendering independent from the screen resolution. 
    /// </summary>
    /// <remarks>This use mostly used for UI and 2D rendering.</remarks>
    public interface IVirtualResolution
    {
        /// <summary>
        /// Gets or sets the screen virtual resolution to use for this game.
        /// </summary>
        Vector3 VirtualResolution { get; set; }
        
        /// <summary>
        /// Occurs when the virtual resolution changed.
        /// </summary>
        event EventHandler<EventArgs> VirtualResolutionChanged;
    }
}