// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Describes the lifetime state of a graphics resource.
    /// </summary>
    public enum GraphicsResourceLifetimeState
    {
        /// <summary>
        /// Resource is active and available for use.
        /// </summary>
        Active = 0,

        /// <summary>
        /// Resource is in a reduced state (partially or completely destroyed) because application is in the background.
        /// Context should still be alive.
        /// This is useful for freeing dynamic resources such as FBO, that could be easily restored when application is resumed.
        /// </summary>
        Paused = 1,

        /// <summary>
        /// Resource has been destroyed due to graphics device being destroyed.
        /// It will need to be recreated or reloaded when rendering resume.
        /// </summary>
        Destroyed = 2,

        // Not sure if this one will be useful yet (in case of async reloading?)
        // Reloading = 3,
    }
}