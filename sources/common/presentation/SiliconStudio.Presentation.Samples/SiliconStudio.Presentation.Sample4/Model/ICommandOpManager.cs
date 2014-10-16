namespace SiliconStudio.Presentation.Sample4.Model
{
    public interface ICommandOpManager
    {
        bool Execute(string commandName, string parameter, bool canUndo);

        bool Undo();

        bool Redo();
    }
}