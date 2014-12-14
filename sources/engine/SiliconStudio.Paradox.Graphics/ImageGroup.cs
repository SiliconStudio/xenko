using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A group of images.
    /// </summary>
    /// <typeparam name="T">The type of image</typeparam>
    [DataContract(Inherited = true)]
    public class ImageGroup<T> where T : ImageFragment
    {
        /// <summary>
        /// The list of sprites.
        /// </summary>
        public List<T> Images = new List<T>();

        /// <summary>
        /// Find the index of a sprite in the group using its name.
        /// </summary>
        /// <param name="spriteName">The name of the sprite</param>
        /// <returns>The index value</returns>
        /// <remarks>If two sprites have the provided name then the first sprite found is returned</remarks>
        /// <exception cref="KeyNotFoundException">No sprite in the group have the given name</exception>
        public int FindImageIndex(string spriteName)
        {
            if (Images != null)
            {
                for (int i = 0; i < Images.Count; i++)
                {
                    if (Images[i].Name == spriteName)
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
        public T this[int index]
        {
            get { return Images[index]; }
            set { Images[index] = value; }
        }

        /// <summary>
        /// Gets or sets the image of the group having the provided <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the image</param>
        /// <returns>The image</returns>
        public T this[string name]
        {
            get { return Images[FindImageIndex(name)]; }
            set { Images[FindImageIndex(name)] = value; }
        }
    }
}