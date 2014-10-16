// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D
using System;
using SharpDX.Direct3D11;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
{
    internal partial class VertexArrayLayout
    {
        private static readonly Dictionary<VertexArrayLayout, VertexArrayLayout> RegisteredLayouts = new Dictionary<VertexArrayLayout, VertexArrayLayout>();

        private VertexArrayLayout()
        {
        }

        private readonly int hashCode;
        public readonly InputElement[] InputElements;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexArrayLayout"/> class.
        /// </summary>
        /// <param name="inputElements">The input elements.</param>
        /// <exception cref="System.ArgumentNullException">inputElements</exception>
        public VertexArrayLayout(InputElement[] inputElements)
        {
            if (inputElements == null) throw new ArgumentNullException("inputElements");

            this.InputElements = inputElements;
            hashCode = inputElements.Length;
            for (int i = 0; i < inputElements.Length; i++)
            {
                hashCode = (hashCode * 397) ^ inputElements[i].GetHashCode();
            }
        }

        /// <summary>
        /// Gets the original create layout.
        /// </summary>
        /// <param name="layout">The layout.</param>
        /// <returns>VertexArrayLayout.</returns>
        public static VertexArrayLayout GetOrCreateLayout(VertexArrayLayout layout)
        {
            VertexArrayLayout registeredLayout;
            lock (RegisteredLayouts)
            {
                if (!RegisteredLayouts.TryGetValue(layout, out registeredLayout))
                {
                    RegisteredLayouts.Add(layout, layout);
                    registeredLayout = layout;
                }
            }
            return registeredLayout;
        }

        public bool Equals(VertexArrayLayout other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return hashCode == other.hashCode && Utilities.Compare(InputElements, other.InputElements);
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VertexArrayLayout)obj);
        }

        public override string ToString()
        {
            var description = " Input Parameters: ";

            for (int i = 0; i < InputElements.Length; i++)
            {
                description += InputElements[i].SemanticName + InputElements[i].SemanticIndex;

                if (i != InputElements.Length - 1)
                    description += ", ";
            }

            return description;
        }
    }
}
#endif