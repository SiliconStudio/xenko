// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Engine.Processors
{
    /// <summary>
    /// An event occurring when the <see cref="ChildSceneComponent.Scene"/> changed.
    /// </summary>
    public struct ChildSceneComponentChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChildSceneComponentChangedEventArgs"/> struct.
        /// </summary>
        /// <param name="component">The child component.</param>
        /// <param name="previousScene">The previous scene.</param>
        /// <param name="newScene">The new scene.</param>
        public ChildSceneComponentChangedEventArgs(ChildSceneComponent component, Scene previousScene, Scene newScene)
        {
            Component = component;
            PreviousScene = previousScene;
            NewScene = newScene;
        }

        /// <summary>
        /// The child component
        /// </summary>
        public readonly ChildSceneComponent Component;

        /// <summary>
        /// The previous scene
        /// </summary>
        public readonly Scene PreviousScene;

        /// <summary>
        /// The new scene
        /// </summary>
        public readonly Scene NewScene;
    }
}