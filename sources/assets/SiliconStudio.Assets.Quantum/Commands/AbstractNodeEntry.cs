namespace SiliconStudio.Assets.Quantum.Commands
{
    /// <summary>
    /// Define a value that can be set by <see cref="CreateNewInstanceCommand"/>.
    /// </summary>
    public abstract class AbstractNodeEntry
    {
        /// <summary>
        /// The display value, as a string.
        /// </summary>
        public abstract string DisplayValue { get; }

        /// <summary>
        /// Gets or creates a new value, used by <see cref="CreateNewInstanceCommand"/>.
        /// </summary>
        /// <param name="currentValue">The current value (might be kept if type didn't change).</param>
        /// <returns></returns>
        public abstract object GenerateValue(object currentValue);

        /// <summary>
        /// Returns true if value is matching the current entry.
        /// </summary>
        /// <param name="value">The value to check against.</param>
        /// <returns>True if it matches, otherwise false.</returns>
        public abstract bool IsMatchingValue(object value);
    }
}