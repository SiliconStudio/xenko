// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Physics
{
    /// <summary>
    /// Extension methods for the <see cref="ScriptComponent"/> related to phystics
    /// </summary>
    public static class PhysicsScriptComponentExtensions
    {
        /// <summary>
        /// Gets the curent <see cref="Simulation"/>.
        /// </summary>
        /// <param name="scriptComponent">The script component to query physics from</param>
        /// <returns>The simulation object or null if there are no simulation running for the current scene.</returns>
        public static Simulation GetSimulation(this ScriptComponent scriptComponent)
        {
            return scriptComponent.SceneSystem.SceneInstance.GetProcessor<PhysicsProcessor>()?.Simulation;
        }
    }
}
