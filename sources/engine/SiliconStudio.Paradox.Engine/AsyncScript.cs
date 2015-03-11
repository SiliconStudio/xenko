using System.Threading.Tasks;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox
{
    /// <summary>
    /// A script which can be implemented as an async microthread.
    /// </summary>
    public abstract class AsyncScript : Script
    {
        /// <summary>
        /// Called once, as a microthread
        /// </summary>
        /// <returns></returns>
        public abstract Task Execute();
    }
}