// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Tests.Helpers
{
    /// <summary>
    /// A static helper to generate deterministic <see cref="ItemId"/> for unit tests.
    /// </summary>
    public static class IdentifierGenerator
    {
        /// <summary>
        /// Gets a deterministic <see cref="ItemId"/> for a given integer seed.
        /// </summary>
        /// <param name="seed">The integer seed of the <see cref="ItemId"/>.</param>
        /// <returns>A <see cref="ItemId"/> that will always be the same for a given seed.</returns>
        public static ItemId Get(int seed)
        {
            var bytes = ToBytes(seed);
            return new ItemId(bytes);
        }

        /// <summary>
        /// Verifies that the given <see cref="ItemId"/> corresponds to the given seed value.
        /// </summary>
        /// <param name="guid">The <see cref="ItemId"/> to verify.</param>
        /// <param name="seed">The seed that should correspond to the <see cref="ItemId"/>.</param>
        /// <returns>True if the <paramref name="guid"/> match the seed, false otherwise.</returns>
        public static bool Match(ItemId guid, int seed)
        {
            var bytes = ToBytes(seed);
            var id = new ItemId(bytes);
            return guid == id;
        }

        private static byte[] ToBytes(int seed)
        {
            var bytes = new byte[16];
            for (int i = 0; i < 4; ++i)
            {
                bytes[4 * i] = (byte)seed;
                bytes[4 * i + 1] = (byte)(seed >> 8);
                bytes[4 * i + 2] = (byte)(seed >> 16);
                bytes[4 * i + 3] = (byte)(seed >> 24);
            }
            return bytes;
        }
    }
}
