// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SiliconStudio.Paradox.Effects
{

    /// <summary>
    /// Delegate used by <see cref="RenderPassExtensions.Foreach"/>.
    /// </summary>
    /// <param name="renderPass">The render pass.</param>
    /// <param name="level">The level reflects the deepness of this renderpass in the renderpass tree. Zero for the top level render pass.</param>
    public delegate void RenderPassForeach(RenderPass renderPass, int level);

    /// <summary>
    /// Extensions for <see cref="RenderPass"/>.
    /// </summary>
    public static partial class RenderPassExtensions
    {
        /// <summary>
        /// Gets a specific processor from a pass.
        /// </summary>
        /// <typeparam name="T">Type of the processor</typeparam>
        /// <param name="pass">The pass.</param>
        /// <returns>An instance of the processor.</returns>
        public static T GetProcessor<T>(this RenderPass pass) where T : Renderer
        {
            return pass.Renderers.OfType<T>().FirstOrDefault();
        }


        /// <summary>
        /// Returns all the descendants from a render pass.
        /// </summary>
        /// <param name="renderPass">The render pass.</param>
        /// <returns>An enumerator on descendants render pass</returns>
        public static IEnumerable<RenderPass> Descendants(this RenderPass renderPass)
        {
            foreach (var subRenderPass in renderPass.Children)
            {
                yield return subRenderPass;
                // Avoid unecessary descendants query
                // Go to descendants
                if (subRenderPass.Children.Count > 0)
                    foreach (var subSubRenderPass in subRenderPass.Descendants())
                        yield return subSubRenderPass;
            } 
        }

        /// <summary>
        /// Iterate on all render pass an execute an action
        /// </summary>
        /// <param name="renderPass">The render pass.</param>
        /// <param name="onRenderPass">The fire action.</param>
        /// <param name="level">The initial level.</param>
        public static void Foreach(this RenderPass renderPass, RenderPassForeach onRenderPass, int level = 0)
        {
            onRenderPass(renderPass, level);
            level++;
            if (renderPass.Children.Count > 0)
                foreach (var subRenderPass in renderPass.Children)
                    subRenderPass.Foreach(onRenderPass, level);
        }

        /// <summary>
        /// Iterate on all Returns all the descendants from a render pass.
        /// </summary>
        /// <param name="renderPass">The render pass.</param>
        /// <param name="writer">The output text writer.</param>
        /// <param name="printer">The printer is a convenient callback to override the default ToString of a RenderPass.</param>
        public static void Print(this RenderPass renderPass, TextWriter writer, Func<RenderPass, string> printer = null)
        {
            renderPass.Foreach((pass, level) => writer.WriteLine("{0}{1}", String.Concat(Enumerable.Repeat(" ", level)), printer != null ? printer(pass) : pass.ToString()));
        }

        /// <summary>
        /// Finds a descendent render pass by name.
        /// </summary>
        /// <param name="renderPass">The render pass.</param>
        /// <param name="name">The name.</param>
        /// <returns>An instance matching the name or null if not found</returns>
        public static RenderPass FindDescendantByName(this RenderPass renderPass, string name)
        {
            var passes = renderPass.Children;
            foreach (var pass in passes)
            {
                if (name == pass.Name)
                    return pass;
                if (pass.Children.Count == 0)
                    continue;
                var found = pass.FindDescendantByName(name);
                if (found != null)
                    return found;
            }
            return null;
        }

    }
}