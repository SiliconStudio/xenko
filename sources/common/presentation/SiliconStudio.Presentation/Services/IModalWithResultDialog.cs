namespace SiliconStudio.Presentation.Services
{
    public interface IModalWithResultDialog : IModalDialog
    {
        /// <summary>
        /// Gets or sets the <see cref="DialogResult"/> value for this dialog.
        /// </summary>
        DialogResult DialogResult { get; set; }
    }
}