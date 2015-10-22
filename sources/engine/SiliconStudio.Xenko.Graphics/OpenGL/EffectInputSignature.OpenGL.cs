// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using System.Collections.Generic;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class EffectInputSignature
    {
        private readonly byte[] bytecode;

        // TODO: Maybe EffectInputSignature should be tied to a device?
        public Dictionary<string, int> Attributes { get; private set; }

        internal EffectInputSignature(ObjectId id, byte[] bytecode)
        {
            this.Id = id;
            this.bytecode = bytecode;
            Attributes = new Dictionary<string, int>();
        }

        internal byte[] NativeSignature
        {
            get
            {
                return bytecode;
            }
        }

        public static void OnDestroyed()
        {
            lock (RegisteredSignatures)
            {
                foreach (var inputLayout in RegisteredSignatures)
                {
                    inputLayout.Value.Attributes.Clear();
                }
            }
        }
    }
}
#endif