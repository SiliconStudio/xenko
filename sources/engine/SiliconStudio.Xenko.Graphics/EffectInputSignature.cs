// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Describes an input signature for an <see cref="Effect"/>.
    /// </summary>
    public partial class EffectInputSignature
    {
        private static readonly Dictionary<ObjectId, EffectInputSignature> RegisteredSignatures = new Dictionary<ObjectId, EffectInputSignature>();

        public readonly ObjectId Id;

        private EffectInputSignature()
        {
        }

        /// <summary>
        /// Gets the original create signature.
        /// </summary>
        /// <param name="signature">The signature.</param>
        /// <returns>VertexArrayLayout.</returns>
        internal static EffectInputSignature GetOrCreateLayout(EffectInputSignature signature)
        {
            EffectInputSignature registeredLayout;
            lock (RegisteredSignatures)
            {
                if (!RegisteredSignatures.TryGetValue(signature.Id, out registeredLayout))
                {
                    RegisteredSignatures.Add(signature.Id, signature);
                    registeredLayout = signature;
                }
            }
            return registeredLayout;
        }
    }
}