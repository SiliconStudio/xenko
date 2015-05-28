// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine.Design;
using SiliconStudio.Paradox.Engine.Processors;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// A link to a scene that is rendered by a parent <see cref="Scene"/>.
    /// </summary>
    [DataContract("ChildSceneComponent")]
    [Display(112, "Child scene")]
    [DefaultEntityComponentProcessor(typeof(ChildSceneProcessor))]
    public sealed class ChildSceneComponent : EntityComponent
    {
        // Used by the ChildSceneProcessor
        [DataMemberIgnore]
        internal SceneInstance SceneInstance;
        
        public readonly static PropertyKey<ChildSceneComponent> Key = new PropertyKey<ChildSceneComponent>("Key", typeof(ChildSceneComponent));

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildSceneComponent"/> class.
        /// </summary>
        public ChildSceneComponent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildSceneComponent"/> class.
        /// </summary>
        /// <param name="scene">The scene.</param>
        public ChildSceneComponent(Scene scene)
        {
            Scene = scene;
        }

        /// <summary>
        /// Gets or sets the child scene.
        /// </summary>
        /// <value>The scene.</value>
        [DataMember(10)]
        public Scene Scene { get; set; }

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}