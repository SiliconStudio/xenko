// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;

using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Extensions for the <see cref="GraphicsDevice"/>
    /// </summary>
    public static class GraphicsDeviceExtensions
    {
        /// <summary>
        /// Draws a fullscreen quad with the specified effect and parameters.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="effect">The effect.</param>
        /// <param name="parameters">The parameters.</param>
        /// <exception cref="System.ArgumentNullException">effect</exception>
        public static void DrawQuad(this GraphicsDevice device, Effect effect, ParameterCollection parameters = null)
        {
            if (effect == null) throw new ArgumentNullException("effect");

            // Apply the effect
            if (parameters != null)
            {
                effect.Apply(device, parameters, false);
            }
            else
            {
                effect.Apply(device, false);
            }

            // Draw a full screen quad
            device.DrawQuad();

            // Unapply
            effect.UnbindResources(device);
        }

        /// <summary>
        /// Draws a fullscreen quad with the specified effect and parameters.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="effect">The effect.</param>
        /// <param name="parameterCollections">The parameter collections.</param>
        /// <exception cref="System.ArgumentNullException">effect</exception>
        public static void DrawQuad<TList>(this GraphicsDevice device, Effect effect, TList parameterCollections) where TList : class, IEnumerable<ParameterCollection>
        {
            if (effect == null) throw new ArgumentNullException("effect");

            // Apply the effect
            effect.Apply(device, parameterCollections, false);

            // Draw a full screen quad
            device.DrawQuad();

            // Unapply
            effect.UnbindResources(device);
        }
    }
}