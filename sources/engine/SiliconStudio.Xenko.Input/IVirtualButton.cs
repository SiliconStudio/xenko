// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Interface IVirtualButton
    /// </summary>
    public interface IVirtualButton
    {
        /// <summary>
        /// Gets the value associated with this virtual button from an input manager.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <returns>System.Single.</returns>
        float GetValue(InputManager manager);
    }
}
