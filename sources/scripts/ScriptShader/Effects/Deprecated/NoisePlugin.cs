// Copyright (c) 2011 Silicon Studio

using System;
using System.Collections.Generic;
using System.IO;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Plugin used to render to a GBuffer from a MainPlugin.
    /// </summary>
    /// <remarks>
    /// This plugin depends on <see cref="MainPlugin"/> parameters.
    /// </remarks>
    public class NoisePlugin : RenderPassPlugin
    {
        static readonly int[,] GradientCoords = new int[16, 3]
                {
                    // 12 cube edges
                    { 0, 1, 1 }, { 0, 1, -1 }, { 0, -1, 1 }, { 0, -1, -1 }, { 1, 0, 1 }, { 1, 0, -1 }, { -1, 0, 1 }, { -1, 0, -1 }, { 1, 1, 0 }, { 1, -1, 0 }, { -1, 1, 0 }, { -1, -1, 0 },
                    // 4 more to make 16
                    { 1, 0, -1 }, { -1, 0, -1 }, { 0, -1, 1 }, { 0, 1, 1 }
                };

        static readonly byte[,] SimplexCoords4 = new byte[64, 4]
                {
                    { 0, 64, 128, 192 }, { 0, 64, 192, 128 }, { 0, 0, 0, 0 }, { 0, 128, 192, 64 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 64, 128, 192, 0 }, { 0, 128, 64, 192 },
                    { 0, 0, 0, 0 }, { 0, 192, 64, 128 }, { 0, 192, 128, 64 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 64, 192, 128, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 },
                    { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 64, 128, 0, 192 }, { 0, 0, 0, 0 }, { 64, 192, 0, 128 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 },
                    { 0, 0, 0, 0 }, { 128, 192, 0, 64 }, { 128, 192, 64, 0 }, { 64, 0, 128, 192 }, { 64, 0, 192, 128 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 128, 0, 192, 64 },
                    { 0, 0, 0, 0 }, { 128, 64, 192, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 },
                    { 128, 0, 64, 192 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 192, 0, 64, 128 }, { 192, 0, 128, 64 }, { 0, 0, 0, 0 }, { 192, 64, 128, 0 }, { 128, 64, 0, 192 },
                    { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 192, 64, 0, 128 }, { 0, 0, 0, 0 }, { 192, 128, 0, 64 }, { 192, 128, 64, 0 }
                };

        /// <summary>
        /// Initializes a new instance of the <see cref="NoisePlugin"/> class.
        /// </summary>
        public NoisePlugin() : this("Noise")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoisePlugin"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public NoisePlugin(string name)
            : base(name)
        {
        }

        public int Seed { get; set; }


        private void ComputePermutations(int seed, byte[] localPermut)
        {
            var random = new Random(seed);
            var availablePermutations = CreatePermutation(localPermut.Length);

            for (int i = 0; i < localPermut.Length; i++)
                localPermut[i] = this.GetNextValue(random, availablePermutations);
        }

        private List<byte> CreatePermutation(int size)
        {
            var availablePermutations = new List<byte>(size);
            for (int i = 0; i < size; i++)
                availablePermutations.Add((byte)i);
            return availablePermutations;
        }

        private byte GetNextValue(Random random, List<byte> permutations)
        {
            int value = random.Next(0, permutations.Count);
            byte result = permutations[value];
            permutations.RemoveAt(value);
            return result;
        }

        public unsafe override void Initialize()
        {
            base.Initialize();

            if (OfflineCompilation)
                return;

            var permutationTable = new byte[256];
            this.ComputePermutations(Seed, permutationTable);

            var pixels = new byte[256 * 256 * 4];
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    int offset = (i * 256 + j) * 4;
                    var value = (byte)permutationTable[(j + permutationTable[i]) & 0xFF];
                    pixels[offset] = (byte)(GradientCoords[value & 0x0F, 0] * 64 + 64); // Gradient x
                    pixels[offset + 1] = (byte)(GradientCoords[value & 0x0F, 1] * 64 + 64); // Gradient y
                    pixels[offset + 2] = (byte)(GradientCoords[value & 0x0F, 2] * 64 + 64); // Gradient z
                    pixels[offset + 3] = value; // Permuted index
                }
            }

            fixed (void* ptr = SimplexCoords4)
                this.Parameters.Set(SimplexNoiseKeys.SimplexTexture, Texture.New2D(GraphicsDevice, 64, 1, 1, PixelFormat.R8G8B8A8_UNorm, new [] { new DataBox((IntPtr)ptr, 64 * 4, 0) }));

            this.Parameters.Set(
                NoiseBaseKeys.PermTexture, Texture.New2D(GraphicsDevice, 256, 256, PixelFormat.R8G8B8A8_UNorm, pixels));
        }

        protected override void Destroy()
        {
            this.Parameters.Remove(SimplexNoiseKeys.SimplexTexture);
            this.Parameters.Remove(NoiseBaseKeys.PermTexture);

            base.Destroy();
        }
    }
}
