namespace SiliconStudio.Xenko.Input
{
    public enum TextInputEventType
    {
        /// <summary>
        /// When new text is entered
        /// </summary>
        Input,
        /// <summary>
        /// This the current text that has not yet been entered but is still being edited
        /// </summary>
        Composition,
    }
}