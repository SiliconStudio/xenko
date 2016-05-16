namespace SiliconStudio.Presentation.Services
{
    /// <summary>
    /// An internal interface representing a modal dialog.
    /// </summary>
    public interface IModalDialogInternal : IModalDialog
    {
        /// <summary>
        /// Gets or sets the result of the modal dialog.
        /// </summary>
        DialogResult Result { get; set; }
    }
}
