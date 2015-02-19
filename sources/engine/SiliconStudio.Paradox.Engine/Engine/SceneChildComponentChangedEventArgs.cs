// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// An event occurring when the <see cref="SceneChildComponent.Scene"/> changed.
    /// </summary>
    public struct SceneChildComponentChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneChildComponentChangedEventArgs"/> struct.
        /// </summary>
        /// <param name="childComponent">The child component.</param>
        /// <param name="previousScene">The previous scene.</param>
        /// <param name="newScene">The new scene.</param>
        public SceneChildComponentChangedEventArgs(SceneChildComponent childComponent, Scene previousScene, Scene newScene)
        {
            ChildComponent = childComponent;
            PreviousScene = previousScene;
            NewScene = newScene;
        }

        /// <summary>
        /// The child component
        /// </summary>
        public readonly SceneChildComponent ChildComponent;

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