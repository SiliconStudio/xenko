using System.Threading.Tasks;

namespace SiliconStudio.Quantum.Commands
{
    /// <summary>
    /// Base class for node commands that are not asynchronous.
    /// </summary>
    public abstract class SyncNodeCommandBase : NodeCommandBase
    {
        /// <inheritdoc/>
        public sealed override Task Execute(IGraphNode node, Index index, object parameter)
        {
            ExecuteSync(node, index, parameter);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Triggers the command synchromously.
        /// </summary>
        /// <param name="nodeent">The node on which to execute the command.</param>
        /// <param name="index">The index in the node on which to execute the command.</param>
        /// <param name="parameter">The parameter of the command.</param>
        /// <remarks>Implementations of this method should not trigger fire-and-forget actions.</remarks>
        protected abstract void ExecuteSync(IGraphNode node, Index index, object parameter);
    }
}