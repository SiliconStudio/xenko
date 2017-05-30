// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.IO;
using System.Threading.Tasks;

namespace SiliconStudio.Assets.TextAccessors
{
    public interface ITextAccessor
    {
        /// <summary>
        /// Gets the underlying text.
        /// </summary>
        /// <returns></returns>
        string Get();

        /// <summary>
        /// Sets the underlying text.
        /// </summary>
        /// <param name="value"></param>
        void Set(string value);

        /// <summary>
        /// Writes the text to the given <see cref="StreamWriter"/>.
        /// </summary>
        /// <param name="streamWriter"></param>
        Task Save(Stream streamWriter);

        ISerializableTextAccessor GetSerializableVersion();
    }
}
