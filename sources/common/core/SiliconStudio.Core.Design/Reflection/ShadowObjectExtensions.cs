using System;

namespace SiliconStudio.Core.Reflection
{
    public static class ShadowObjectExtensions
    {
        /// <summary>
        /// Tries to get the value of a dynamic property.
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="instance">The instance object.</param>
        /// <param name="memberKey">The member key.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="value">The value attached.</param>
        /// <returns><c>true</c> if there is a value attached, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// instance
        /// or
        /// memberKey
        /// or
        /// attributeKey
        /// </exception>
        public static bool TryGetDynamicProperty<T>(this object instance, object memberKey, PropertyKey<T> attributeKey, out T value)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (memberKey == null) throw new ArgumentNullException(nameof(memberKey));
            if (attributeKey == null) throw new ArgumentNullException(nameof(attributeKey));

            ShadowObject shadow;
            value = default(T);
            object objValue = null;
            var result = (ShadowObject.TryGet(instance, out shadow) && shadow.TryGetValue(new ShadowObjectPropertyKey(memberKey, attributeKey), out objValue));
            if (result)
            {
                value = (T)objValue;
            }
            return result;
        }

        /// <summary>
        /// Sets a dynamic property.
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="instance">The instance object.</param>
        /// <param name="memberKey">The member key.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">
        /// instance
        /// or
        /// memberKey
        /// or
        /// attributeKey
        /// </exception>
        public static void SetDynamicProperty<T>(this object instance, object memberKey, PropertyKey<T> attributeKey, T value)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (memberKey == null) throw new ArgumentNullException(nameof(memberKey));
            if (attributeKey == null) throw new ArgumentNullException(nameof(attributeKey));
            ShadowObject.GetOrCreate(instance)[new ShadowObjectPropertyKey(memberKey, attributeKey)] = value;
        }
    }
}