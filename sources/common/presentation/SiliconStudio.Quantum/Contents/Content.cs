using System;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// A helper class containing methods to manipulate contents.
    /// </summary>
    public static class Content
    {
        /// <summary>
        /// Retrieves the value itself or the value of one of its item, depending on the given <see cref="Index"/>.
        /// </summary>
        /// <param name="value">The value on which this method applies.</param>
        /// <param name="index">The index of the item to retrieve. If <see cref="Index.Empty"/> is passed, this method will return the value itself.</param>
        /// <param name="descriptor">The descriptor of the type of <paramref name="value"/>.</param>
        /// <returns>The value itself or the value of one of its item.</returns>
        public static object Retrieve(object value, Index index, ITypeDescriptor descriptor)
        {
            if (!index.IsEmpty)
            {
                var collectionDescriptor = descriptor as CollectionDescriptor;
                if (collectionDescriptor != null)
                {
                    return collectionDescriptor.GetValue(value, index.Int);
                }
                var dictionaryDescriptor = descriptor as DictionaryDescriptor;
                if (dictionaryDescriptor != null)
                {
                    return dictionaryDescriptor.GetValue(value, index.Value);
                }

                throw new NotSupportedException("Unable to retrieve the value at the given index, this collection is unsupported");
            }
            return value;
        }
    }
}
