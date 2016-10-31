// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for a mapping of anonymous gamepad objects to a common controller layout, as used with <see cref="GamePadState"/>. Derive from this type to create custom layouts
    /// </summary>
    public abstract class GamePadLayout
    {
        /// <summary>
        /// Checks if a device matches this gamepad layout, and thus should use this when mapping it to a <see cref="GamePadState"/>
        /// </summary>
        /// <param name="device"></param>
        public abstract bool MatchDevice(IGamePadDevice device);

        /// <summary>
        /// Gets a <see cref="GamePadState"/> from a <see cref="IGamePadDevice"/> using this mapping
        /// </summary>
        /// <param name="device">The gamepad</param>
        /// <param name="state">The state that should be filled</param>
        /// <returns>The current state that this gamepad maps to</returns>
        public abstract void GetState(IGamePadDevice device, ref GamePadState state);
    }
}