using System.Threading.Tasks;

namespace SiliconStudio.Quantum.Commands
{
    /// <summary>
    /// Base class for node commands that are not asynchronous.
    /// </summary>
    public abstract class SyncNodeCommandBase : NodeCommandBase
    {
        /// <inheritdoc/>
        public sealed override Task Execute(IContentNode content, Index index, object parameter)
        {
            ExecuteSync(content, index, parameter);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Triggers the command synchromously.
        /// </summary>
        /// <param name="content">The content on which to execute the command.</param>
        /// <param name="index">The index in the content on which to execute the command.</param>
        /// <param name="parameter">The parameter of the command.</param>
        /// <remarks>Implementations of this method should not trigger fire-and-forget actions.</remarks>
        protected abstract void ExecuteSync(IContentNode content, Index index, object parameter);
    }
}