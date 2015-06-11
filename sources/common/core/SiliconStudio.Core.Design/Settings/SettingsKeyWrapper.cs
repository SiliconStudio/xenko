using System;

using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Settings
{
    /// <summary>
    /// An helper class to wrap a <see cref="SettingsKey"/> in the context of a given <see cref="SettingsProfile"/> into a simple object.
    /// </summary>
    public abstract class SettingsKeyWrapper
    {
        protected readonly SettingsProfile Profile;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsKeyWrapper"/> class.
        /// </summary>
        /// <param name="key">The <see cref="SettingsKey"/> represented by this instance.</param>
        /// <param name="profile">The <see cref="SettingsProfile"/> in which this settings key is contained.</param>
        protected SettingsKeyWrapper(SettingsKey key, SettingsProfile profile)
        {
            Key = key;
            Profile = profile;
        }

        /// <summary>
        /// Gets the <see cref="SettingsKey"/> associated to this <see cref="SettingsKeyWrapper"/>.
        /// </summary>
        [DataMemberIgnore]
        public SettingsKey Key { get; private set; }

        /// <summary>
        /// Gets the current value of the <see cref="SettingsKey"/>.
        /// </summary>
        public abstract object Value { get; set; }

        /// <summary>
        /// Create a new instance of the correct implementation of <see cref="SettingsKeyWrapper"/> that matches the given settings key.
        /// </summary>
        /// <param name="key">The settings key for which to create a instance.</param>
        /// <param name="profile">The <see cref="SettingsProfile"/> in which this settings key is contained.</param>
        /// <returns>A new instance of the <see cref="SettingsKeyWrapper"/> class.</returns>
        public static SettingsKeyWrapper Create(SettingsKey key, SettingsProfile profile)
        {
            var result = (SettingsKeyWrapper)Activator.CreateInstance(typeof(SettingsKeyWrapper<>).MakeGenericType(key.Type), key, profile);
            return result;
        }
    }

    /// <summary>
    /// An helper class to wrap a <see cref="SettingsValueKey{T}"/> in the context of a given <see cref="SettingsProfile"/> into a simple object.
    /// </summary>
    /// <typeparam name="T">The type of value in the <see cref="SettingsValueKey{T}"/>.</typeparam>
    public class SettingsKeyWrapper<T> : SettingsKeyWrapper
    {
        private readonly SettingsValueKey<T> key;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsKeyWrapper{T}"/> class.
        /// </summary>
        /// <param name="key">The <see cref="SettingsValueKey{T}"/> represented by this instance.</param>
        /// <param name="profile">The <see cref="SettingsProfile"/> in which this settings key is contained.</param>
        public SettingsKeyWrapper(SettingsValueKey<T> key, SettingsProfile profile)
            : base(key, profile)
        {
            this.key = key;
        }

        /// <summary>
        /// Gets or sets the current value that can be pushed later to the <see cref="SettingsValueKey{T}"/> represented by this instance.
        /// </summary>
        [InlineProperty]
        [NotNull]
        public T TypedValue { get { return GetValue(); } set { SetValue(value); } }

        /// <inheritdoc/>
        public override object Value { get; set; }
        
        private void SetValue(T value)
        {
            key.SetValue(value, Profile);
        }

        private T GetValue()
        {
            return key.GetValue(true, Profile);
        }

    }
}