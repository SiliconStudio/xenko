namespace SiliconStudio.Presentation.Services
{
    /// <summary>
    /// A structure representing the check status and the button pressed by the user to close a checkable message box.
    /// </summary>
    public struct CheckedMessageBoxResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckedMessageBoxResult"/> structure.
        /// </summary>
        /// <param name="result">The result of the message box.</param>
        /// <param name="isChecked">The check status of the message box.</param>
        public CheckedMessageBoxResult(MessageBoxResult result, bool? isChecked)
        {
            Result = result;
            IsChecked = isChecked;
        }

        /// <summary>
        /// Gets the result of the message box.
        /// </summary>
        public MessageBoxResult Result { get; }

        /// <summary>
        /// Gets the check status of the message box.
        /// </summary>
        public bool? IsChecked { get; }
    }
}
