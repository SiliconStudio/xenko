// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine.Design;
using SiliconStudio.Paradox.Engine.Processors;

namespace SiliconStudio.Paradox.Engine
{
    [DataContract("ModelNodeLinkComponent")]
    [Display("Model Node Link")]
    [DefaultEntityComponentProcessor(typeof(ModelNodeLinkProcessor))]
    public sealed class ModelNodeLinkComponent : EntityComponent
    {
        public static PropertyKey<ModelNodeLinkComponent> Key = new PropertyKey<ModelNodeLinkComponent>("Key", typeof(ModelNodeLinkComponent));

        internal ModelProcessor.EntityLink EntityLink;
        internal ModelNodeLinkProcessor Processor;
        private ModelComponent target;
        private string nodeName;

        /// <summary>
        /// Gets or sets the model which contains the hierarchy to use.
        /// </summary>
        /// <value>
        /// The model which contains the hierarchy to use.
        /// </value>
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

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}