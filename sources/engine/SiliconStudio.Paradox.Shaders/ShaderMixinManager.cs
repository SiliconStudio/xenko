// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Shaders
{
    /// <summary>
    /// Manages <see cref="IShaderMixinBuilder"/> and generation of shader mixins.
    /// </summary>
    public class ShaderMixinManager
    {
        private static readonly Dictionary<string, IShaderMixinBuilder> RegisteredBuilders = new Dictionary<string, IShaderMixinBuilder>();

        /// <summary>
        /// Registers a <see cref="IShaderMixinBuilder"/> with the specified pdxfx effect name.
        /// </summary>
        /// <param name="pdxfxEffectName">Name of the mixin.</param>
        /// <param name="builder">The builder.</param>
        /// <exception cref="System.ArgumentNullException">
        /// pdxfxEffectName
        /// or
        /// builder
        /// </exception>
        public static void Register(string pdxfxEffectName, IShaderMixinBuilder builder)
        {
            if (pdxfxEffectName == null)
                throw new ArgumentNullException("pdxfxEffectName");

            if (builder == null)
                throw new ArgumentNullException("builder");

            lock (RegisteredBuilders)
            {
                RegisteredBuilders[pdxfxEffectName] = builder;
            }
        }

        /// <summary>
        /// Determines whether the specified PDXFX effect is registered.
        /// </summary>
        /// <param name="pdxfxEffectName">Name of the PDXFX effect.</param>
        /// <returns><c>true</c> if the specified PDXFX effect is registered; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">pdxfxEffectName</exception>
        public static bool Contains(string pdxfxEffectName)
        {
            if (pdxfxEffectName == null) throw new ArgumentNullException("pdxfxEffectName");
            lock (RegisteredBuilders)
            {
                return RegisteredBuilders.ContainsKey(pdxfxEffectName);
            }
        }

        /// <summary>
        /// Tries to get a <see cref="IShaderMixinBuilder"/> by its name.
        /// </summary>
        /// <param name="pdxfxEffectName">Name of the mixin.</param>
        /// <param name="builder">The builder instance found or null if not found.</param>
        /// <returns><c>true</c> if the builder was found, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">pdxfxEffectName</exception>
        public static bool TryGet(string pdxfxEffectName, out IShaderMixinBuilder builder)
        {
            if (pdxfxEffectName == null)
                throw new ArgumentNullException("pdxfxEffectName");

            lock (RegisteredBuilders)
            {
                return RegisteredBuilders.TryGetValue(pdxfxEffectName, out builder);
            }
        }

        /// <summary>
        /// Generates a <see cref="ShaderMixinSourceTree" /> for the specified names and parameters.
        /// </summary>
        /// <param name="pdxfxEffectName">The name.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>The result of the mixin.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// pdxfxEffectName
        /// or
        /// properties
        /// </exception>
        /// <exception cref="System.ArgumentException">pdxfxEffectName</exception>
        public static ShaderMixinSourceTree Generate(string pdxfxEffectName, ParameterCollection properties)
        {
            if (pdxfxEffectName == null) throw new ArgumentNullException("pdxfxEffectName");

            if (properties == null)
                throw new ArgumentNullException("properties");

            IShaderMixinBuilder builder;
            Dictionary<string, IShaderMixinBuilder> builders;
            lock (RegisteredBuilders)
            {
                if (!TryGet(pdxfxEffectName, out builder))
                    throw new ArgumentException(string.Format("Pdxfx effect [{0}] not found", pdxfxEffectName), "pdxfxEffectName");

                builders = new Dictionary<string, IShaderMixinBuilder>(RegisteredBuilders);
            }

            // TODO cache mixin context and avoid to recreate one (check if if thread concurrency could occur here)
            var context = new ShaderMixinContext(properties, builders);
            var mixinTree = new ShaderMixinSourceTree() { Name = pdxfxEffectName };
            context.BeginChild(mixinTree);
            builder.Generate(mixinTree, context);
            context.EndChild();

            return mixinTree;
        }

        /// <summary>
        /// Un-register all registered <see cref="IShaderMixinBuilder"/>.
        /// </summary>
        public static void UnRegisterAll()
        {
            lock (RegisteredBuilders)
            {
                RegisteredBuilders.Clear();
            }
        }
    }
}