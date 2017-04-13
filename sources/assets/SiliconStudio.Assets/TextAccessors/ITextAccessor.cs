// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
