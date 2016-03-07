using System.Windows.Input;

namespace SiliconStudio.Presentation.Drawing
{
    public interface IDrawingViewCommand
    {
        /// <summary>
        /// Executes the command on the view.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="args">The event data.</param>
        void Execute(IDrawingView view, IDrawingController controller, InputEventArgs args);
    }
    
    public interface IDrawingViewCommand<in T> : IDrawingViewCommand
        where T : InputEventArgs
    {
        /// <summary>
        /// Executes the command on the view.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="args">The event data.</param>
        void Execute(IDrawingView view, IDrawingController controller, T args);
    }
}
