// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using System.Runtime.InteropServices;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Data;
using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// A helper class to compute a unique object id for a <see cref="ShaderMixinSource"/>.
    /// </summary>
    [DataSerializerGlobal(typeof(ParameterKeyHashSerializer), Profile = "Hash")]
    [DataSerializerGlobal(typeof(ParameterCollectionHashSerializer), Profile = "Hash")]
    public class ShaderMixinObjectId
    {
        private static object generatorLock = new object();
        private static ShaderMixinObjectId generator;

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
            writer.Context.SerializerSelector = new SerializerSelector("Default", "Hash");
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

        /// <summary>
        /// Computes a hash <see cref="ObjectId"/> for the specified effect and compiler parameters.
        /// </summary>
        /// <param name="effectName">Name of the effect.</param>
        /// <param name="compilerParameters">The compiler parameters.</param>
        /// <returns>
        /// EffectObjectIds.
        /// </returns>
        public static ObjectId Compute(string effectName, ShaderMixinParameters compilerParameters)
        {
            lock (generatorLock)
            {
                if (generator == null)
                {
                    generator = new ShaderMixinObjectId();
                }
                return generator.ComputeInternal(effectName, compilerParameters);
            }
        }

        private unsafe ObjectId ComputeInternal(ShaderMixinSource mixin, ShaderMixinParameters mixinParameters)
        {
            // Write to memory stream
            memStream.Position = 0;
            writer.Write(EffectBytecode.MagicHeader); // Write the effect bytecode magic header
            writer.Write(mixin);

            writer.Write(mixinParameters.Get(CompilerParameters.GraphicsPlatformKey));
            writer.Write(mixinParameters.Get(CompilerParameters.GraphicsProfileKey));
            writer.Write(mixinParameters.Get(CompilerParameters.DebugKey));

            // Compute hash
            objectIdBuilder.Reset();
            objectIdBuilder.Write((byte*)buffer, (int)memStream.Position);

            return objectIdBuilder.ComputeHash();
        }

        private unsafe ObjectId ComputeInternal(string effectName, ShaderMixinParameters compilerParameters)
        {
            // Write to memory stream
            memStream.Position = 0;
            writer.Write(EffectBytecode.MagicHeader); // Write the effect bytecode magic header
            writer.Write(effectName);

            writer.Write(compilerParameters);

            // Compute hash
            objectIdBuilder.Reset();
            objectIdBuilder.Write((byte*)buffer, (int)memStream.Position);

            return objectIdBuilder.ComputeHash();
        }
    }
}