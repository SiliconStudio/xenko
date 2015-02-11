// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// Defines a graphics layer input coming from the output of another layer.
    /// </summary>
    [DataContract("SceneGraphicsLayerInputLayer")]
    [Display("Layer")]
    public sealed class SceneGraphicsLayerInputLayer : ISceneGraphicsLayerInput, IEquatable<SceneGraphicsLayerInputLayer>
    {
        /// <summary>
        /// Gets a previous layer.
        /// </summary>
        public static SceneGraphicsLayerInputLayer PreviousLayer()
        {
            return new SceneGraphicsLayerInputLayer(-1);   
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneGraphicsLayerInputLayer"/> class.
        /// </summary>
        public SceneGraphicsLayerInputLayer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneGraphicsLayerInputLayer"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        public SceneGraphicsLayerInputLayer(int index)
        {
            Index = index;
        }

        /// <summary>
        /// Gets or sets the layer index from the <see cref="SceneGraphicsLayerCollection"/>
        /// </summary>
        /// <value>The layer index.</value>
        [DataMember(0)]
        public int Index { get; set; }

        public bool Equals(SceneGraphicsLayerInputLayer other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SceneGraphicsLayerInputLayer)obj);
        }

        public override int GetHashCode()
        {
            return Index;
        }
    }
}