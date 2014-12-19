// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// A composition material to blend different materials.
    /// </summary>
    [DataContract("MaterialBlendLayerStack")]
    [Display("Material Layers")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialBlendLayerStack : IMaterialComposition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBlendLayerStack"/> class.
        /// </summary>
        public MaterialBlendLayerStack()
        {
            Layers = new List<MaterialBlendLayer>();
        }

        /// <summary>
        /// Gets the layers.
        /// </summary>
        /// <value>The layers.</value>
        public List<MaterialBlendLayer> Layers { get; private set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                var stack = new MaterialBlendLayerStack();
                stack.Layers.Add(ObjectFactory.NewInstance<MaterialBlendLayer>());
                return stack;
            }
        }

        public virtual void GenerateShader(MaterialShaderGeneratorContext context)
        {
            foreach (var layer in Layers)
            {
                layer.GenerateShader(context);
            }
        }
    }
}