// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine
{
    [DataContract("ModelNodeLinkComponent")]
    [Display("Model Node Link")]
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

        protected internal override PropertyKey DefaultKey
        {
            get { return Key; }
        }

        private static readonly Type[] DefaultProcessors = new Type[] { typeof(ModelNodeLinkProcessor) };
        protected internal override IEnumerable<Type> GetDefaultProcessors()
        {
            return DefaultProcessors;
        }
    }
}