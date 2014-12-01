// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Effects
{
    public delegate void UpdateMeshesDelegate(RenderPass currentRenderPass, ref FastList<RenderMesh> meshes);

    /// <summary>
    /// RenderPass is a hierarchy that defines how to collect and render meshes.
    /// </summary>
    public class RenderPass : ComponentBase
    {
        private readonly TrackingCollection<Renderer> renderers;
        private readonly TrackingCollection<RenderPass> children;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderPass"/> class.
        /// </summary>
        public RenderPass() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderPass"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public RenderPass(string name) : base(name)
        {
            Parameters = new ParameterCollection();
            Enabled = true;
            // create Renderers lists
            renderers = new TrackingCollection<Renderer>();
            renderers.CollectionChanged += (o, e) =>
            {
                var processor = (Renderer)e.Item;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        // Check consistency of Parent before setting it
                        if (processor.Pass != null)
                            throw new InvalidOperationException("Renderer.Pass is already attached to another pass.");
                        processor.Pass = this;
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        // Check consistency of Parent before setting it
                        if (processor.Pass != this)
                            throw new InvalidOperationException("Renderer.Pass is not attached to a this pass..");
                        //processor.Pass = null;
                        break;
                }
            };

            // Create children passes
            children = new TrackingCollection<RenderPass>();
            children.CollectionChanged += (o, e) =>
                {
                    var renderPass = (RenderPass)e.Item;
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            // Check consistency of Parent before setting it
                            if (renderPass.Parent != null)
                                throw new InvalidOperationException("Pass.Parent should be null.");
                            renderPass.Parent = this;
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            // Check consistency of Parent before setting it
                            if (renderPass.Parent != this)
                                throw new InvalidOperationException("Pass.Parent is not set properly.");
                            renderPass.Parent = null;
                            break;
                    }
                };
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RenderPass"/> is enabled for collection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled for collection; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets the parent pass.
        /// </summary>
        /// <value>
        /// The parent pass.
        /// </value>
        public RenderPass Parent { get; internal set; }

        /// <summary>
        /// Gets the pipeline (root node, which should be of type <see cref="RenderPipeline"/>).
        /// </summary>
        /// <value>
        /// The root pass.
        /// </value>
        public RenderPipeline Pipeline
        {
            get
            {
                var current = this;

                while (current.Parent != null)
                {
                    current = current.Parent;
                }

                // Not sure yet if we should throw an exception or return null?
                return current as RenderPipeline;
            }
        }

        /// <summary>
        /// The start action.
        /// </summary>
        public DelegateHolder<RenderContext> StartPass;

        /// <summary>
        /// The end action.
        /// </summary>
        public DelegateHolder<RenderContext> EndPass;

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public ParameterCollection Parameters { get; private set; }

        /// <summary>
        /// Gets the Renderers attached to this renderpass.
        /// </summary>
        /// <value>The Renderers.</value>
        public TrackingCollection<Renderer> Renderers
        {
            get
            {
                return renderers;
            }
        }

        /// <summary>
        /// Gets the sub render passes.
        /// </summary>
        /// <value>
        /// The sub render passes.
        /// </value>
        public TrackingCollection<RenderPass> Children
        {
            get { return children; }
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", GetType().Name, Name ?? "");
        }
    }
}