// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// A sheet (group) of sprites.
    /// </summary>
    [DataContract]
    [DataSerializerGlobal(typeof(ReferenceSerializer<SpriteSheet>), Profile = "Content")]
    [ContentSerializer(typeof(DataContentSerializer<SpriteSheet>))]
    public class SpriteSheet
    {
        /// <summary>
        /// The list of sprites.
        /// </summary>
        [NotNullItems]
        public List<Sprite> Sprites { get; } = new List<Sprite>();

        /// <summary>
        /// Find the index of a sprite in the group using its name.
        /// </summary>
        /// <param name="spriteName">The name of the sprite</param>
        /// <returns>The index value</returns>
        /// <remarks>If two sprites have the provided name then the first sprite found is returned</remarks>
        /// <exception cref="KeyNotFoundException">No sprite in the group have the given name</exception>
        public int FindImageIndex(string spriteName)
        {
            if (Sprites != null)
            {
                for (int i = 0; i < Sprites.Count; i++)
                {
                    if (Sprites[i].Name == spriteName)
                        return i;
                }
            }

            throw new KeyNotFoundException(spriteName);
        }

        /// <summary>
        /// Gets or sets the image of the group at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The image index</param>
        /// <returns>The image</returns>
        public Sprite this[int index]
        {
            get { return Sprites[index]; }
            set { Sprites[index] = value; }
        }

        /// <summary>
        /// Gets or sets the image of the group having the provided <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the image</param>
        /// <returns>The image</returns>
        public Sprite this[string name]
        {
            get { return Sprites[FindImageIndex(name)]; }
            set { Sprites[FindImageIndex(name)] = value; }
        }
    }
}