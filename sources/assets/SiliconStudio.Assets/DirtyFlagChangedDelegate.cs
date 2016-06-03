namespace SiliconStudio.Assets
{
    /// <summary>
    /// A delegate used for events raised when the dirty flag of an object has changed
    /// </summary>
    /// <param name="sender">The object that had its dirty flag changed.</param>
    /// <param name="oldValue">The old value of the dirty flag.</param>
    /// <param name="newValue">The new value of the dirty flag.</param>
    public delegate void DirtyFlagChangedDelegate<in T>(T sender, bool oldValue, bool newValue);
}