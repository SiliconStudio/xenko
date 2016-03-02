// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A collection of <see cref="IGraphicsRenderer"/> that is itself a <see cref="IGraphicsRenderer"/> handling automatically
    /// <see cref="IGraphicsRenderer.Initialize"/> and <see cref="IGraphicsRenderer.Unload"/>.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="IGraphicsRenderer"/></typeparam>.
    [DataSerializer(typeof(ListAllSerializer<,>), Mode = DataSerializerGenericMode.TypeAndGenericArguments)]
    public abstract class GraphicsRendererCollectionBase<T> : RendererBase, IList<T> where T : class, IGraphicsRendererCore
    {
        private readonly HashSet<T> tempRenderers;

        private readonly List<T> currentRenderers;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsRendererCollection{T}"/> class.
        /// </summary>
        protected GraphicsRendererCollectionBase()
        {
            tempRenderers = new HashSet<T>(ReferenceEqualityComparer<T>.Default);
            currentRenderers = new List<T>();
            Profiling = false; // We don't generate a begin/end for a collection but let the collection be embedded in the parent
        }

        [DataMemberIgnore]
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
            }
        }

        public List<T>.Enumerator GetEnumerator()
        {
            return currentRenderers.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return currentRenderers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)currentRenderers).GetEnumerator();
        }

        public void Add(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            currentRenderers.Add(item);
        }

        public void Clear()
        {
            currentRenderers.Clear();
        }

        public bool Contains(T item)
        {
            return currentRenderers.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            currentRenderers.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return currentRenderers.Remove(item);
        }

        public int Count
        {
            get
            {
                return currentRenderers.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public int IndexOf(T item)
        {
            return currentRenderers.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            currentRenderers.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            currentRenderers.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                return currentRenderers[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                currentRenderers[index] = value;
            }
        }

        protected override void Unload()
        {
            // Unload renderers
            foreach (var renderer in currentRenderers)
            {
                renderer.Dispose();
            }
            currentRenderers.Clear();

            base.Unload();
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            InitializeRenderers(context.RenderContext);

            // Draw all renderers
            foreach (var renderer in currentRenderers)
            {
                if (renderer.Enabled)
                {
                    // Draw the renderer
                    DrawRenderer(context, renderer);
                }
            }
        }

        protected void InitializeRenderers(RenderContext context)
        {
            // Initialize all renderer first
            foreach (var renderer in currentRenderers)
            {
                // initialize the renderer if needed.
                if (!renderer.Initialized)
                    renderer.Initialize(context);
            }
        }

        protected abstract void DrawRenderer(RenderDrawContext context, T renderer);
    }
}