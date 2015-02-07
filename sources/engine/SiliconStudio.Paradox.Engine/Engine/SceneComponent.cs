// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// A component used internally to tag a Scene.
    /// </summary>
    public sealed class SceneComponent : EntityComponent
    {
        /// <summary>
        /// The key of this component.
        /// </summary>
        public static PropertyKey<SceneComponent> Key = new PropertyKey<SceneComponent>("Key", typeof(SceneComponent));

        public override PropertyKey DefaultKey
        {
            get
            {
                return Key;
            }
        }

        private static readonly Type[] DefaultProcessors = new Type[] { typeof(SceneProcessor) };
        protected internal override IEnumerable<Type> GetDefaultProcessors()
        {
            return DefaultProcessors;
        }
    }
}