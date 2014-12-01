// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using System.Runtime.InteropServices;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Shaders
{
    /// <summary>
    /// A helper class to compute a unique object id for a <see cref="ShaderMixinSource"/>.
    /// </summary>
    public class ShaderMixinObjectId
    {
        private static object generatorLock = new object();
        private static ShaderMixinObjectId generator;

        private static ParameterCollection parameters;

        private readonly NativeMemoryStream memStream;
        private readonly HashSerializationWriter writer;
        private ObjectIdBuilder objectIdBuilder;
        private IntPtr buffer;

        private ShaderMixinObjectId()
        {
            objectIdBuilder = new ObjectIdBuilder();
            buffer = Marshal.AllocHGlobal(65536);
            memStream = new NativeMemoryStream(buffer, 65536);
            writer = new HashSerializationWriter(memStream);
            writer.Context.SerializerSelector = new SerializerSelector();
            writer.Context.SerializerSelector.RegisterProfile("Default");
            writer.Context.SerializerSelector.RegisterSerializer(new ParameterKeyHashSerializer());
            writer.Context.SerializerSelector.RegisterSerializer(new ParameterCollectionHashSerializer());

            if (parameters == null)
                parameters = new ParameterCollection();
        }

        /// <summary>
        /// Computes a hash <see cref="ObjectId"/> for the specified mixin.
        /// </summary>
        /// <param name="mixin">The mixin.</param>
        /// <param name="mixinParameters">The mixin parameters.</param>
        /// <returns>EffectObjectIds.</returns>
        public static ObjectId Compute(ShaderMixinSource mixin, ShaderMixinParameters mixinParameters)
        {
            lock (generatorLock)
            {
                if (generator == null)
                {
                    generator = new ShaderMixinObjectId();
                }
                return generator.ComputeInternal(mixin, mixinParameters);
            }
        }

        private unsafe ObjectId ComputeInternal(ShaderMixinSource mixin, ShaderMixinParameters mixinParameters)
        {
            // Write to memory stream
            memStream.Position = 0;
            writer.Write(EffectBytecode.MagicHeader); // Write the effect bytecode magic header
            writer.Write(mixin);

            parameters.Clear();
            parameters.Set(CompilerParameters.GraphicsPlatformKey, mixinParameters.Get(CompilerParameters.GraphicsPlatformKey));
            parameters.Set(CompilerParameters.GraphicsProfileKey, mixinParameters.Get(CompilerParameters.GraphicsProfileKey));
            parameters.Set(CompilerParameters.DebugKey, mixinParameters.Get(CompilerParameters.DebugKey));
            writer.Write(parameters);

            // Compute hash
            objectIdBuilder.Reset();
            objectIdBuilder.Write((byte*)buffer, (int)memStream.Position);

            return objectIdBuilder.ComputeHash();
        }
    }
}