// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Engine
{
    [DataConverter(AutoGenerate = true)]
    [DataContract("ModelNodeLinkComponent")]
    public sealed class ModelNodeLinkComponent : EntityComponent
    {
        public static PropertyKey<ModelNodeLinkComponent> Key = new PropertyKey<ModelNodeLinkComponent>("Key", typeof(ModelNodeLinkComponent));

        internal MeshProcessor.EntityLink EntityLink;
        internal ModelNodeLinkProcessor Processor;
        private ModelComponent target;
        private string nodeName;

        /// <summary>
        /// Gets or sets the model which contains the hierarchy to use.
        /// </summary>
        /// <value>
        /// The model which contains the hierarchy to use.
        /// </value>
        [DataMemberConvert]
        public ModelComponent Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
                UpdateDirty();
            }
        }

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        [DataMemberConvert]
        public string NodeName
        {
            get
            {
                return nodeName;
            }
            set
            {
                nodeName = value;
                UpdateDirty();
            }
        }

        private void UpdateDirty()
        {
            var processor = Processor;
            if (processor != null)
            {
                lock (processor.DirtyLinks)
                {
                    processor.DirtyLinks.Add(this);
                }
            }
        }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }
}