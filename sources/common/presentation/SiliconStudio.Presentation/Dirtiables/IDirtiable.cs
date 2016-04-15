namespace SiliconStudio.Presentation.Dirtiables
{
    public interface IDirtiable
    {
        /// <summary>
        /// Gets the dirty state of this object.
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Updates the <see cref="IsDirty"/> property to the given value.
        /// </summary>
        /// <param name="value">The new value for the dirty flag.</param>
        void UpdateDirtiness(bool value);
    }
}
