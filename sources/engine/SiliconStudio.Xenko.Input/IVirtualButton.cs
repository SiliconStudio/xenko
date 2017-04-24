// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
