// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Material with multiple layers that will be mixed at runtime in the shader
    /// </summary>
    [ContentSerializer(typeof(DataContentSerializer<LayeredMaterial>))]
    [DataContract("LayeredMaterial")]
    public class LayeredMaterial
    {
        /// <summary>
        /// List of the layers
        /// </summary>
        public List<MaterialDescription> Layers { get; set; }

        /// <summary>
        /// Number of layers
        /// </summary>
        public int LayerCount
        {
            get
            {
                return Layers.Count;
            }
        }

        public LayeredMaterial()
        {
            Layers = new List<MaterialDescription>();
        }
    }
}
