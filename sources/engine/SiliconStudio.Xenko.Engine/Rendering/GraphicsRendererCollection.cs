// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A collection of <see cref="IGraphicsRenderer"/> that is itself a <see cref="IGraphicsRenderer"/> handling automatically
    /// <see cref="IGraphicsRenderer.Initialize"/> and <see cref="IGraphicsRenderer.Unload"/>.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="IGraphicsRenderer"/></typeparam>.
    [DataSerializer(typeof(ListAllSerializer<,>), Mode = DataSerializerGenericMode.TypeAndGenericArguments)]
    public abstract class GraphicsRendererCollection<T> : GraphicsRendererCollectionBase<T> where T : class, IGraphicsRenderer
    {
        protected override void DrawRenderer(RenderDrawContext context, T renderer)
        {
            renderer.Draw(context);
        }
    }
}